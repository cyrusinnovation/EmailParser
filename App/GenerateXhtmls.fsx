#I "../Library/bin/Debug/"
#r "EmailParser.dll"
#r "MimeKitLite.dll"
#r "HtmlAgilityPack.dll"

open EmailParser.Mime
open EmailParser.Html
open EmailParser.Text
open EmailParser.ODonnell

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let message = loadMimeMessageFrom("../Library/ODonnell.eml")
let lines = textOf message |> splitIntoLines

let parsed = parseMail lines