module MeetupParser.Utils

open System
open System.Text.RegularExpressions
open MimeKit
open EmailParser.Utils.Mime
open EmailParser.Utils.Date
open EmailParser.Utils.Text
open EmailParser.Types

let messageDataFor (message: MimeMessage) (originalMessageString: string) =
    {
        Sender = senderOf message;
        SentDate = dateOf message;
        MessageLines = (textOf message |> splitIntoLines);
        EntireMessage = originalMessageString
    }

let startsWithTitle (line: string) = line.Trim().StartsWith("What:")

let extractTitleFrom (titleLine: string) = 
    let startIndex = titleLine.IndexOf(": ") + 2
    titleLine.Substring(startIndex).Trim()

let startsWithEventDate (line: string) = line.Trim().StartsWith("When:")

let extractDateTimeFrom (dateTimeLine: string) = 
    let startIndex = dateTimeLine.IndexOf(": ") + 2
    dateTimeLine.Substring(startIndex).Trim()

let startsWithLocation (line: string) = line.Trim().StartsWith("Where:")

let isRsvpLine (line: string) = line.Trim() |> normalizeSpace |> (fun str -> str.StartsWith("Click here to say"))

let extractRsvpLinkFrom (rsvpLine: string) = 
    let startIndex = rsvpLine.IndexOf("http")
    rsvpLine.Substring(startIndex).Trim()

let dateFrom = function
    | dayOfWeek :: month :: day :: year :: rest when isDayOfWeek dayOfWeek && isMonth month && isYear year -> dateFromMonthDayYear month day year
    | dayOfWeek :: month :: day :: rest when isDayOfWeek dayOfWeek && isMonth month -> dateFromMonthDay month day 
    | month :: day :: year :: rest when isMonth month && isYear year -> dateFromMonthDayYear month day year
    | month :: day :: rest when isMonth month -> dateFromMonthDay month day
    | other -> failwith(sprintf "unable to parse date: %A" other)

let dateAndTimeFrom (dateTimeString: string) = 
    let normalized = dateTimeString.Trim()  |> normalizeSpace
                                            |> regexReplace @"\s+:" ":"     //No space around colons in time
                                            |> regexReplace @":\s+" ":"
                                            |> regexReplace @"\s+," ","     //No space before commas
                                            |> regexReplaceIgnoreCase @"\s+am\s*" "am" //No space before am or pm
                                            |> regexReplaceIgnoreCase @"\s+pm\s*" "pm" 

    let parts = normalized.Split(' ')

    let date = List.ofArray parts |> dateFrom 

    match hoursAndMinutesFrom parts.[parts.Length - 1] with
        | Some(hours, minutes) -> 
            let dateTime = date.AddHours(hours).AddMinutes(minutes)
            { Date = dateTime ; Time = Some(dateTime) }
        | None -> { Date = date ; Time = None }
