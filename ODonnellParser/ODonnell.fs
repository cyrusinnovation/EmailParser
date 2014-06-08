module ODonnellParser.Parser

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

let parseMail (mailText: list<string>) = parse mailText PreIntro