module DenbowParser.Parser

open EmailParser.Types
open EmailParser.Utils.Collections

(*
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
    *)



// let parseMail (message: EmailMessage) : EmailData = 
//    let messageParts = parse message.MessageLines PreIntro
//    parseIntoEmailData message.Sender message.SentDate messageParts