module DenbowParser.Utils

//open System
open MimeKit
open EmailParser.Utils.Mime
open EmailParser.Utils.Html
open EmailParser.Utils.Text
open EmailParser.Utils.Date
open EmailParser.Types

let messageDataFor (message: MimeMessage) =
    let messageLines = htmlPartsOf message |> String.concat "\n" |> toPlainText |> splitIntoLines
    {
        Sender = senderOf message;
        SentDate = dateOf message;
        MessageLines = messageLines
    }
            
let startsWithEventDate (line: string) =
    line.Trim() |> startsWithOneOf [ "MON "; "TUE "; "WED "; "THU "; "FRI "; "SAT "; "SUN " ]

let startsWithBullet (line: string) = line.Trim() |> startsWithOneOf ["*"]

let startsWithLinkOrBullet (line: string) = line.Trim() |> startsWithOneOf [ "["; "*" ]

let startsWithMonth (line: string) =
    line.Trim().ToLower() |> startsWithOneOf [ "jan "; "january ";
                                               "feb "; "february ";
                                               "mar "; "march ";
                                               "apr "; "april ";
                                               "may "; 
                                               "jun "; "june ";
                                               "jul "; "july ";
                                               "aug "; "august ";
                                               "sep "; "september ";
                                               "oct "; "october ";
                                               "nov "; "november ";
                                               "dec "; "december " ]

let extractEventUrlFrom (eventHeader: string) = 
    let startOfEventUrl = eventHeader.IndexOf("[")
    let startingWithEventUrl = eventHeader.Substring(startOfEventUrl + 1)
    let endIndex = startingWithEventUrl.IndexOf("]")
    startingWithEventUrl.Substring(0, endIndex)

let extractTitleFrom (eventHeader: string) = 
    let startIndex = eventHeader.IndexOf("]") + 1
    eventHeader.Substring(startIndex).Trim()

let toDateTime (dateTimeString: string) = 
    let normalized = regexReplace " +"  " " dateTimeString
    let parts = normalized.Split(' ')

    let date = dateFromMonthDay parts.[0] parts.[1]
    let hours, minutes = hoursAndMinutesFrom parts.[2]
    date.AddHours(hours).AddMinutes(minutes)

let containsCalendarLink (descriptionLine: string) =
    let normalizedLine = descriptionLine.ToLower() |> regexReplace " +"  " "
    normalizedLine.Contains("view in calendar")

let removeCalendarLink (descriptionLine: string) =
    if containsCalendarLink descriptionLine then
        descriptionLine |> regexReplace @"\s*\|?\s*\[http.*" ""        //Remove everything after " | [http" with or without pipe
    else
        descriptionLine