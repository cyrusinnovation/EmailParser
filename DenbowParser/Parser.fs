module DenbowParser.Parser

open MimeKit
open EmailParser.Types
open EmailParser.Utils.Collections
open EmailParser.Utils.Mime
open EmailParser.Utils.Text
open EmailParser.Utils.Uri
open DenbowParser.Utils

type State =
    | PreIntro
    | Intro
    | RSVPLink
    | EventTitle
    | EventDateTime
    | EventDescription
    | EventTrailer

type MessagePart =
    | IntroPart of list<string>
    | RsvpLinkPart of string
    | TitlePart of string
    | DateTimePart of string
    | DescriptionPart of list<string>
            
let rec parse (mail: seq<string>) (state: State) : list<MessagePart> = 
    let mailText = List.ofSeq mail
    match state with
    | PreIntro  -> preIntro mailText
    | Intro -> intro mailText
    | RSVPLink -> rsvpLink mailText
    | EventTitle -> eventTitle mailText
    | EventDateTime -> eventDateTime mailText
    | EventDescription -> eventDescription mailText
    | EventTrailer -> eventTrailer mailText

and preIntro (mailText: list<string>)  = 
    let linesAfterPreIntro = mailText |> Seq.skipWhile (fun str -> str.Trim().StartsWith("[http") || System.String.IsNullOrWhiteSpace(str))
    parse linesAfterPreIntro Intro

and intro (mailText: list<string>) = 
    let introLines, rest = mailText |> takeAndSkipUntil startsWithEventDate
    let introPart = List.ofSeq(introLines) |> List.map (fun str -> str.Trim()) |> IntroPart
    introPart :: (parse rest RSVPLink)

and rsvpLink (mailText: list<string>) = 
    let link = mailText.Head |> extractEventUrlFrom |> RsvpLinkPart
    link :: (parse mailText EventTitle)

and eventTitle (mailText: list<string>) = 
    let title = mailText.Head |>  extractTitleFrom |> TitlePart
    let rest = mailText.Tail |> Seq.skipWhile isBlank
    title :: (parse rest EventDateTime)

and eventDateTime (mailText: list<string>) = 
    let date = mailText.Head.Trim() |> DateTimePart
    let rest = mailText.Tail |> Seq.skipWhile isBlank
    date :: (parse rest EventDescription)

and eventDescription (mailText: list<string>) = 
    let descriptionLines, rest = mailText |> takeAndSkipUntil containsCalendarLink

    let description = Seq.append descriptionLines [ Seq.head rest ]
                                       |> Seq.map (fun str -> str.Trim())
                                       |> Seq.map removeCalendarLink  
                                       |> Seq.filter (isBlank >> not)
                                       |> List.ofSeq 
                                       |> DescriptionPart

    let more = rest |> Seq.skip 1 |> Seq.skipWhile isBlank |> List.ofSeq
    match more with 
        | [] -> []
        | next :: _ when startsWithEventDate next -> description :: (parse more RSVPLink)
        | _ -> description :: (parse more EventTrailer)

and eventTrailer (mailText: list<string>) = 
    if startsWithBullet mailText.Head then
        []
    else
        let firstLine = mailText.Head
        let info, rest = mailText.Tail |> takeAndSkipUntil startsWithLinkOrBullet
        let eventInfo = firstLine :: (List.ofSeq info)

        let headline = eventInfo.Head
        let rsvpLink = extractEventUrlFrom headline |> RsvpLinkPart
        let eventTitle = extractTitleFrom headline |> TitlePart

        let eventDetails = eventInfo.Tail
        let date = eventDetails |> List.filter startsWithMonth 
                                |> List.map (fun str -> str.Trim()) 
                                |> List.head 
                                |> DateTimePart

        let description = eventDetails |> List.filter (isBlank >> not) 
                                       |> List.filter (startsWithMonth >> not)
                                       |> List.map (fun str -> str.Trim())
                                       |> DescriptionPart

        rsvpLink :: eventTitle :: date :: description :: (parse rest EventTrailer)

let extractRemainingDataForCurrentCalendarEntry (messageParts: list<MessagePart>) = 
    messageParts |> takeAndSkipUntil (function 
                                        | RsvpLinkPart(_) -> true 
                                        | _ -> false )

let calendarEntryFrom (rsvpLinkPart: MessagePart) (messageParts: seq<MessagePart>) = 
    let rsvp = match rsvpLinkPart with | RsvpLinkPart(link) -> link | _ -> "" 

    let date = messageParts         |> extractWithEmptyStringDefault (function | DateTimePart(dateTime) -> Some(dateTime)                | _ -> None)
    let title = messageParts        |> extractWithEmptyStringDefault (function | TitlePart(aTitle)      -> Some(aTitle)                  | _ -> None)
    let description = messageParts  |> extractWithEmptyStringDefault (function | DescriptionPart(desc)  -> Some(String.concat "\n" desc) | _ -> None)

    {
        EventDate = (dateAndTimeFrom date);
        EventTitle = title;
        EventLocation = None;
        EventDescription = description;
        RsvpLink = (uriFrom rsvp)
    }

let rec calendarEntriesFrom (messageParts: list<MessagePart>) : list<CalendarEntry> =
    let rsvpLinkPart = messageParts.Head
    let (thisEntryData, rest) = extractRemainingDataForCurrentCalendarEntry messageParts.Tail

    let calendarEntry = calendarEntryFrom rsvpLinkPart thisEntryData
    let remainingParts = List.ofSeq rest

    match remainingParts with 
        | [] -> [ calendarEntry ]
        | RsvpLinkPart(_) :: items -> calendarEntry :: (calendarEntriesFrom remainingParts)
        | _ -> sprintf "Nonempty list not starting with RSVP link part: [%s]" (remainingParts.ToString()) |> failwith

let parseIntoEmailData (sender: string) (sentDate: System.DateTime) (messageParts: list<MessagePart>) : EmailData =
    let intro = messageParts |> extractWithEmptyStringDefault (function | IntroPart(intro) -> Some(String.concat "\n" intro)  | _ -> None)
    let nonIntroParts = messageParts |> List.filter (function |IntroPart(_) -> false | _ -> true)
    let calendarEntries = calendarEntriesFrom nonIntroParts

    { MailDate = sentDate; MailSender = sender; MailIntro = intro; CalendarEntries = calendarEntries }

let parseMail (message: MimeMessage) : EmailData = 
    let messageData = messageDataFor message
    let messageParts = parse messageData.MessageLines PreIntro
    parseIntoEmailData messageData.Sender messageData.SentDate messageParts