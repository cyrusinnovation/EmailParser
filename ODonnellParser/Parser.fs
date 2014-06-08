module ODonnellParser.Parser

open EmailParser.Parser
open EmailParser.Utils.Collections
open ODonnellParser.Utils

type State =
    | PreIntro
    | Intro
    | Date
    | EventHeader
    | EventLocation
    | EventDescription
    | RSVPLink

type MessagePart =
    | IntroPart of list<string>
    | DatePart of string
    | TimePart of string
    | TitlePart of string
    | LocationPart of list<string>
    | DescriptionPart of list<string>
    | RsvpLinkPart of string
            
let rec parse (mailText: seq<string>) (state: State) : list<MessagePart> = 
    match state with
    | PreIntro  -> preIntro mailText
    | Intro -> intro mailText
    | Date -> date mailText
    | EventHeader -> eventHeader mailText
    | EventLocation -> eventLocation mailText
    | EventDescription -> eventDescription mailText
    | RSVPLink -> rsvpLink mailText

and preIntro (mailText: seq<string>)  = 
    let linesAfterPreIntro = mailText |> Seq.skipWhile (fun str -> str.StartsWith("http:"))
    parse linesAfterPreIntro Intro

and intro (mailText: seq<string>) = 
    let introLines, rest = mailText |> takeAndSkipUntil (fun str -> str.StartsWith("______________________________________"))
    let introPart = List.ofSeq(introLines) |> IntroPart
    let restWithoutDivider = rest |> Seq.skip 1 |> Seq.skipWhile (fun str -> str.Length = 0)
    introPart :: (parse restWithoutDivider Date)

and date (mailText: seq<string>) = 
    let date = Seq.head mailText |> DatePart
    let rest = mailText |> Seq.skip 1 |> Seq.skipWhile (fun str -> str.Length = 0)
    date :: (parse rest EventHeader)

and eventHeader (mailText: seq<string>) = 
    let header = Seq.head mailText
    let timeAndTitle = removeEventUrlFrom header
    let (time, title) = splitTimeFromTitle timeAndTitle
    let rest = mailText |> Seq.skip 1 |> Seq.skipWhile (fun str -> str.Length = 0)
    TimePart(time) :: TitlePart(title) :: (parse rest EventLocation)

and eventLocation (mailText: seq<string>) =
    let locationLines, rest = mailText |> takeAndSkipUntil (fun str -> str.Length = 0)
    let locationPart = List.ofSeq(locationLines) |> LocationPart
    let restWithoutEmptyLine = rest |> Seq.skipWhile (fun str -> str.Length = 0)
    locationPart :: (parse restWithoutEmptyLine EventDescription)

and eventDescription (mailText: seq<string>) =
    let descriptionLines, rest = mailText |> takeAndSkipUntil (fun str -> str.StartsWith("RSVP (http"))
    let description = List.ofSeq(descriptionLines) |> DescriptionPart
    description :: (parse rest RSVPLink)

and rsvpLink (mailText: seq<string>) = 
    let link = mailText |> Seq.head |> extractRsvpLink |> RsvpLinkPart
    let rest = mailText |> Seq.skip 1 |> Seq.skipWhile (fun str -> str.Length = 0 || str.StartsWith("_____________________"))
    let next = rest |> Seq.head

    match next with
        | ADateHeader -> link :: (parse rest Date)
        | AnEventHeader -> link :: (parse rest EventHeader)
        | _ -> [link]


let extractDateStringFrom (datePart: MessagePart) = 
    match datePart with
    | DatePart(dateString) -> dateString
    | _ -> sprintf "Expected date part but got %s" (datePart.ToString()) |> failwith

let extractTimeStringFrom = function 
    | TimePart(timeString) -> timeString
    | _ -> "12AM"

let extractRemainingDataForCurrentCalendarEntry (messageParts: list<MessagePart>) = 
    messageParts.Tail |> takeAndSkipUntil (function 
                                            | DatePart(_) -> true 
                                            | TimePart(_) -> true 
                                            | _ -> false )

let calendarEntryFrom (date: string) (time: string) (messageParts: seq<MessagePart>) = 
    let title = messageParts        |> extractWithEmptyStringDefault (function | TitlePart(aTitle)      -> Some(aTitle)                  | _ -> None)
    let location = messageParts     |> extractWithEmptyStringDefault (function | LocationPart(loc)      -> Some(String.concat "\n" loc)  | _ -> None)
    let description = messageParts  |> extractWithEmptyStringDefault (function | DescriptionPart(desc)  -> Some(String.concat "\n" desc) | _ -> None)
    let rsvp = messageParts         |> extractWithEmptyStringDefault (function | RsvpLinkPart(link)     -> Some(link)                    | _ -> None)

    {
        EventDate = (dateAndTimeFrom date time);
        EventTitle = title;
        EventLocation = location;
        EventDescription = description;
        RsvpLink = (uriFrom rsvp)
    }

let rec calendarEntriesFrom (datePart: MessagePart) (messageParts: list<MessagePart>) : list<CalendarEntry> =
    let date = extractDateStringFrom datePart    
    let time = extractTimeStringFrom messageParts.Head
    let (thisEntryData, rest) = extractRemainingDataForCurrentCalendarEntry messageParts.Tail

    let calendarEntry = calendarEntryFrom date time thisEntryData
    let remainingParts = List.ofSeq rest

    match remainingParts with 
        | [] -> [ calendarEntry ]
        | TimePart(timeString) :: items -> calendarEntry :: (calendarEntriesFrom datePart remainingParts)
        | (DatePart(_) as nextDate) :: items -> calendarEntry :: (calendarEntriesFrom nextDate items)
        | _ -> sprintf "Nonempty list not starting with date or time part: [%s]" (remainingParts.ToString()) |> failwith


let parseIntoEmailData (sender: string) (sentDate: System.DateTime) (messageParts: list<MessagePart>) : EmailData =
    let intro = messageParts |> List.choose (function |IntroPart(intro) -> Some(intro) | _ -> None) |> List.head
    let nonIntroParts = messageParts |> List.filter (function |IntroPart(_) -> false | _ -> true)
    let calendarEntries = calendarEntriesFrom nonIntroParts.Head nonIntroParts.Tail

    { MailDate = sentDate; MailSender = sender; MailIntro = intro; CalendarEntries = calendarEntries }

let parseMail (sender: string) (sentDate: System.DateTime) (mailText: list<string>) = 
    let messageParts = parse mailText PreIntro
    parseIntoEmailData sender sentDate messageParts