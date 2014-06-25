module MeetupParser.Parser

open MimeKit
open EmailParser.Types
open EmailParser.Utils.Collections
open EmailParser.Utils.Text
open EmailParser.Utils.Uri

open MeetupParser.Utils


type State =
    | Header
    | EventTitle
    | EventDateTime
    | EventLocation
    | EventDescription
    | RSVPLink
    | MessageTrailer

type MessagePart =
    | TitlePart of string
    | DateTimePart of string
    | LocationPart of list<string>
    | DescriptionPart of list<string>
    | RsvpLinkPart of string

let rec parse (mail: seq<string>) (state: State) : list<MessagePart> = 
    let mailText = List.ofSeq mail
    match state with
    | Header  -> header mailText
    | EventTitle -> eventTitle mailText
    | EventDateTime -> eventDateTime mailText
    | EventLocation -> eventLocation mailText
    | EventDescription -> eventDescription mailText
    | RSVPLink -> rsvpLink mailText
    | MessageTrailer -> messageTrailer mailText

and header (mailText: list<string>) = 
    let headerLines, rest = mailText |> takeAndSkipUntil startsWithTitle
    (parse rest EventTitle)

and eventTitle (mailText: list<string>) = 
    let title = mailText.Head |>  extractTitleFrom |> TitlePart
    let ignore, rest = mailText |> takeAndSkipUntil startsWithEventDate
    title :: (parse rest EventDateTime)

and eventDateTime (mailText: list<string>) = 
    let date = mailText.Head |> extractDateTimeFrom |> DateTimePart
    let ignore, rest = mailText |> takeAndSkipUntil startsWithLocation
    date :: (parse rest EventLocation)

and eventLocation (mailText: list<string>) = 
    let locationLines, rest = mailText.Tail |> takeAndSkipUntil isBlank
    let location = locationLines |> List.ofSeq |> LocationPart
    location :: (parse rest EventDescription)

and eventDescription (mailText: list<string>) = 
    let descriptionLines, rest = mailText.Tail |> takeAndSkipUntil isRsvpLine
    let description = descriptionLines |> List.ofSeq |> DescriptionPart
    description :: (parse rest RSVPLink)

and rsvpLink (mailText: list<string>) = 
    let link = mailText.Head |> extractRsvpLinkFrom |> RsvpLinkPart
    link :: (parse mailText.Tail MessageTrailer)

and messageTrailer (mailText: list<string>) = []

let calendarEntryFrom (messageParts: seq<MessagePart>) = 

    let title = messageParts        |> extractWithEmptyStringDefault (function | TitlePart(aTitle)      -> Some(aTitle)                  | _ -> None)
    let date = messageParts         |> extractWithEmptyStringDefault (function | DateTimePart(dateTime) -> Some(dateTime)                | _ -> None)
    let location = messageParts     |> extractWithEmptyStringDefault (function | LocationPart(loc)      -> Some(String.concat "\n" loc)  | _ -> None)
    let description = messageParts  |> extractWithEmptyStringDefault (function | DescriptionPart(desc)  -> Some(String.concat "\n" desc) | _ -> None)
    let rsvp = messageParts         |> extractWithEmptyStringDefault (function | RsvpLinkPart(link)     -> Some(link)                    | _ -> None)

    {
        EventDate = (dateAndTimeFrom date);
        EventTitle = title;
        EventLocation = Some(location);
        EventDescription = description.Trim();
        RsvpLink = (uriFrom rsvp)
    }

let parseIntoEmailData (sender: string) (sentDate: System.DateTime) (messageParts: list<MessagePart>) : EmailData =
    let calendarEntries = [ calendarEntryFrom messageParts ]

    { MailDate = sentDate; MailSender = sender; MailIntro = ""; CalendarEntries = calendarEntries }

let parseMail (message: MimeMessage) : EmailData = 
    let messageData = messageDataFor message
    let messageParts = parse messageData.MessageLines Header
    parseIntoEmailData messageData.Sender messageData.SentDate messageParts