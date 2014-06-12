module DenbowParser.Parser

open EmailParser.Types
open EmailParser.Utils.Collections


type State =
    | PreIntro
    | Intro
    | EventDate
    | RSVPLink
    | EventHeader
    | EventDateTime
    | EventDescription
    | EventTrailer
    | EmailFooter

type MessagePart =
    | IntroPart of list<string>
    | RsvpLinkPart of string
    | TitlePart of string
    | DateTimePart of string
    | DescriptionPart of list<string>


//let parseMail (message: EmailMessage) : EmailData = 
//    let messageParts = parse message.MessageLines PreIntro
//    parseIntoEmailData message.Sender message.SentDate messageParts