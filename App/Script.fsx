#I "bin/Debug/"
#r "ParserTypes.dll"
#r "EmailParserUtils.dll"
#r "MimeKitLite.dll"
#r "HtmlAgilityPack.dll"
#r "ODonnellParser.dll"

open EmailParser.Utils.Mime
open EmailParser.Utils.Text
open ODonnellParser.Parser
open ODonnellParser.Utils
open EmailParser.Utils.Collections
open System.Text.RegularExpressions

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
// let message = loadMimeMessageFrom("../ODonnellParser/Email.odonnell")
let message = loadMimeMessageFrom("../ODonnellParser/Email.denbow")

let messageParts = parse message.MessageLines PreIntro
let nonIntroParts = messageParts |> List.filter (function |IntroPart(_) -> false | _ -> true)
let date = extractDateStringFrom nonIntroParts.Head    
let time = extractTimeStringFrom nonIntroParts.Tail.Head
let (thisEntryData, rest) = extractRemainingDataForCurrentCalendarEntry nonIntroParts.Tail.Tail

let calendarEntry = calendarEntryFrom date time thisEntryData

let parsed = parseMail message