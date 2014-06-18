#I "bin/Debug/"
#r "ParserTypes.dll"
#r "EmailParserUtils.dll"
#r "MimeKitLite.dll"

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
#load "Utils.fs"
#load "Parser.fs"

open EmailParser.Utils.Mime
open EmailParser.Utils.Text
open EmailParser.Utils.Collections
open System.Text.RegularExpressions
open DenbowParser.Parser
open DenbowParser.Utils

let message = System.IO.File.ReadAllText("Email.denbow") |> loadMimeMessageFrom 

let messageData = messageDataFor message
let messageLines = messageData.MessageLines

let messageParts = parse messageLines PreIntro
let nonIntroParts = messageParts |> List.filter (function |IntroPart(_) -> false | _ -> true)
let (thisEntryData, rest) = extractRemainingDataForCurrentCalendarEntry nonIntroParts.Tail

let calendarEntry = calendarEntryFrom (Seq.head thisEntryData) (Seq.skip 1 thisEntryData)

let parsed = parseMail message