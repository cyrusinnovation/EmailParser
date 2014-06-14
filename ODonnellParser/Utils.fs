module ODonnellParser.Utils

open System
open System.Text.RegularExpressions
open MimeKit
open EmailParser.Utils.Mime
open EmailParser.Utils.Date
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
     
let dateFrom (dateString: string) = 
    let normalizingRegex = Regex(@"^[^\s]*\s+([A-Za-z]+)\s+(\d+).*")  // Chop off day of the week and "th" at the end
    let capturedGroups : GroupCollection = normalizingRegex.Match(dateString).Groups 
    let month = capturedGroups.Item(1).Value
    let day = capturedGroups.Item(2).Value
    dateFromMonthDay month day

let dateAndTimeFrom (dateString: string) (timeString: string) : DateAndTime =
    let date = dateFrom dateString
    match hoursAndMinutesFrom timeString with
        | Some(hours, minutes) -> 
            let dateTime = date.AddHours(hours).AddMinutes(minutes)
            { Date = dateTime ; Time = Some(dateTime) }
        | None -> { Date = date ; Time = None }
