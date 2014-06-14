module DenbowParser.Parser

open MimeKit
open EmailParser.Types
open EmailParser.Utils.Collections
open EmailParser.Utils.Mime
open EmailParser.Utils.Text
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
    let header = List.head mailText
    let link = extractEventUrlFrom header |> RsvpLinkPart
    link :: (parse mailText EventTitle)

and eventTitle (mailText: list<string>) = 
    let header = List.head mailText
    let link = extractTitleFrom header |> TitlePart
    let rest = List.tail mailText |> Seq.skipWhile isBlank
    link :: (parse rest EventDateTime)

and eventDateTime (mailText: list<string>) = 
    let date = (List.head mailText).Trim() |> DateTimePart
    let rest = List.tail mailText |> Seq.skipWhile isBlank
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
    let thisLine = List.head mailText
    if startsWithBullet thisLine then
        []
    else
        let firstLine = List.head mailText
        let info, rest = List.tail mailText |> takeAndSkipUntil startsWithLinkOrBullet
        let eventInfo = firstLine :: (List.ofSeq info)

        let headline = List.head eventInfo
        let rsvpLink = extractEventUrlFrom headline |> RsvpLinkPart
        let eventTitle = extractTitleFrom headline |> TitlePart

        let eventDetails = List.tail eventInfo
        let date = eventDetails |> List.filter startsWithMonth 
                                |> List.map (fun str -> str.Trim()) 
                                |> List.head 
                                |> DateTimePart

        let description = eventDetails |> List.filter (isBlank >> not) 
                                       |> List.filter (startsWithMonth >> not)
                                       |> List.map (fun str -> str.Trim())
                                       |> DescriptionPart

        rsvpLink :: eventTitle :: date :: description :: (parse rest EventTrailer)

//let parseMail (message: MimeMessage) : EmailData = 
//    let messageData = messageDataFor message
//    let messageParts = parse messageData.MessageLines PreIntro
//    parseIntoEmailData messageData.Sender messageData.SentDate messageParts