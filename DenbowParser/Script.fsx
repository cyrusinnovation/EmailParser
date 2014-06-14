#I "bin/Debug/"
#r "ParserTypes.dll"
#r "EmailParserUtils.dll"
#r "MimeKitLite.dll"
#r "HtmlAgilityPack.dll"
#r "DenbowParser.dll"

open EmailParser.Utils.Mime
open EmailParser.Utils.Text
open EmailParser.Utils.Collections
open System.Text.RegularExpressions
open DenbowParser.Parser
open DenbowParser.Utils

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let message = loadMimeMessageFrom "Email.denbow"

let messageData = messageDataFor message
let messageLines = messageData.MessageLines

let messageParts = parse messageLines PreIntro
let nonIntroParts = messageParts |> List.filter (function |IntroPart(_) -> false | _ -> true)
let (thisEntryData, rest) = extractRemainingDataForCurrentCalendarEntry nonIntroParts.Tail

let calendarEntry = calendarEntryFrom (Seq.head thisEntryData) (Seq.skip 1 thisEntryData)

let parsed = parseMail message