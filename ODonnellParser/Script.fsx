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
open ODonnellParser.Parser
open ODonnellParser.Utils

let inputString = System.IO.File.ReadAllText("Email.odonnell")
let message = inputString |> loadMimeMessageFrom

let messageData = messageDataFor message inputString
let messageParts = parse messageData.MessageLines PreIntro
let nonIntroParts = messageParts |> List.filter (function |IntroPart(_) -> false | _ -> true)
let date = extractDateStringFrom nonIntroParts.Head    
let time = extractTimeStringFrom nonIntroParts.Tail.Head
let (thisEntryData, rest) = extractRemainingDataForCurrentCalendarEntry nonIntroParts.Tail.Tail

let calendarEntry = calendarEntryFrom date time thisEntryData

let parsed = parseMail message