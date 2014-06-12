module ODonnellParser.Utils

open System
open System.Text.RegularExpressions
open MimeKit
open EmailParser.Utils.Mime
open EmailParser.Utils.Text
open EmailParser.Types

let messageDataFor (message: MimeMessage) =
    {
        Sender = senderOf message;
        SentDate = dateOf message;
        MessageLines = (textOf message |> splitIntoLines)
    }

let removeEventUrlFrom (eventHeader: string) = 
    let startOfEventUrl = eventHeader.IndexOf(" (http")
    eventHeader.Substring(0, startOfEventUrl)

let splitTimeFromTitle (eventHeader: string) = 
    let separatorIndex = eventHeader.IndexOf(" - ")
    let time = eventHeader.Substring(0, separatorIndex)
    let title = eventHeader.Substring(separatorIndex + 3)
    (time, title)

let extractRsvpLink (rsvpLinkText: string) = 
    let startIndex = rsvpLinkText.IndexOf("(") + 1
    let endIndex = rsvpLinkText.IndexOf(")")
    let length = endIndex - startIndex
    rsvpLinkText.Substring(startIndex, length)

let (|ADateHeader|_|) (line: string) =
    let regex = Regex(@"^(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday),? ")
    let m = regex.Match(line)
    if m.Success
    then Some()
    else None

let (|AnEventHeader|_|) (line: string) =
    let regex = Regex(@"^\d\d?:?\d?\d?[AP]M -")
    let m = regex.Match(line)
    if m.Success
    then Some()
    else None

let monthIndexFrom (monthString: string) = 
    let months = [| "jan"; "feb"; "mar"; "apr"; "may"; "jun"; "jul"; "aug"; "sep"; "oct"; "nov"; "dec" |]
    let thisMonth = monthString.Substring(0,3).ToLower()
    let index = Array.IndexOf(months, thisMonth)
    if index = -1 then 1 else (index + 1)
     
let dateFrom (dateString: string) = 
    let normalizingRegex = Regex(@"^[^\s]*\s+([A-Za-z]+)\s+(\d+).*")  // Chop off day of the week and "th" at the end
    let capturedGroups : GroupCollection = normalizingRegex.Match(dateString).Groups 
    let month = capturedGroups.Item(1).Value
    let day = capturedGroups.Item(2).Value
    let year = DateTime.Today.Year
    let proposedDate = new DateTime(year, (monthIndexFrom month),  (Convert.ToInt32 day))
    if proposedDate < DateTime.Today.AddMonths(-1) then       // Events have no years. If the date is less than a month ago, this may be a next year event
        proposedDate.AddYears(1)
    else
        proposedDate

let hoursAndMinutesFrom (timeString: string) =
    let time = timeString.Substring(0, timeString.Length - 2)
    let hoursAndMinutes = time.Split(':') |> Array.map (fun numString -> (Convert.ToDouble numString))
    let hours = if timeString.ToLower().EndsWith("pm") 
                then hoursAndMinutes.[0] + 12.0
                else hoursAndMinutes.[0]
    let minutes = if hoursAndMinutes.Length = 2 then hoursAndMinutes.[1] else 0.0
    (hours, minutes)

let dateAndTimeFrom (dateString: string) (timeString: string) =
    let date = dateFrom dateString
    let hours, minutes = hoursAndMinutesFrom timeString
    date.AddHours(hours).AddMinutes(minutes)

let uriFrom (uriString: string) = 
    try 
        new Uri(uriString)
    with 
        | ex -> new Uri("file://unparsable.uri/")