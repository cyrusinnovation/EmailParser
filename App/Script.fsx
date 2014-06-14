#I "bin/Debug/"
#r "ParserTypes.dll"
#r "EmailParserUtils.dll"
#r "MimeKitLite.dll"
#r "HtmlAgilityPack.dll"
#r "ODonnellParser.dll"
#r "DenbowParser.dll"

open EmailParser.Utils.Mime
open EmailParser.Utils.Text
open EmailParser.Utils.Collections
open System.Text.RegularExpressions

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
(*
let message = loadMimeMessageFrom("../ODonnellParser/Email.odonnell")
let parse = ODonnellParser.Parser.parse
let extractDateStringFrom = ODonnellParser.Parser.extractDateStringFrom
let extractTimeStringFrom = ODonnellParser.Parser.extractTimeStringFrom
let extractRemainingDataForCurrentCalendarEntry = ODonnellParser.Parser.extractRemainingDataForCurrentCalendarEntry
let calendarEntryFrom = ODonnellParser.Parser.calendarEntryFrom
let parseMail = ODonnellParser.Parser.parseMail
let PreIntro = ODonnellParser.Parser.PreIntro
let IntroPart = ODonnellParser.Parser.IntroPart

*)


let message = loadMimeMessageFrom("../DenbowParser/Email.denbow")
let parse = DenbowParser.Parser.parse
let extractDateStringFrom = DenbowParser.Parser.extractDateStringFrom
let extractTimeStringFrom = DenbowParser.Parser.extractTimeStringFrom
let extractRemainingDataForCurrentCalendarEntry = DenbowParser.Parser.extractRemainingDataForCurrentCalendarEntry
let calendarEntryFrom = DenbowParser.Parser.calendarEntryFrom
let parseMail = DenbowParser.Parser.parseMail
let PreIntro = DenbowParser.Parser.PreIntro
let IntroPart = DenbowParser.Parser.IntroPart

let messageParts = parse message.MessageLines PreIntro
let nonIntroParts = messageParts |> List.filter (function |IntroPart(_) -> false | _ -> true)
let date = extractDateStringFrom nonIntroParts.Head    
let time = extractTimeStringFrom nonIntroParts.Tail.Head
let (thisEntryData, rest) = extractRemainingDataForCurrentCalendarEntry nonIntroParts.Tail.Tail

let calendarEntry = calendarEntryFrom date time thisEntryData

let parsed = parseMail message