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

open MeetupParser.Utils
open MeetupParser.Parser

let inputString = System.IO.File.ReadAllText("Email.meetup")
let message = inputString |> loadMimeMessageFrom 

let messageData = messageDataFor message inputString
let messageLines = messageData.MessageLines

let message2 = System.IO.File.ReadAllText("Email2.meetup") |> loadMimeMessageFrom 
let message2Data = messageDataFor message2

let messageParts = parse messageLines Header
let parsed = parseMail message