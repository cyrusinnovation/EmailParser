#I "bin/Debug/"
#r "EmailParserUtils.dll"
#r "MimeKitLite.dll"
#r "HtmlAgilityPack.dll"
#r "ODonnellParser.dll"

open EmailParser.Utils.Mime
open EmailParser.Utils.Text
open ODonnellParser.Parser

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let message = loadMimeMessageFrom("../ODonnellParser/ODonnell.eml")
let lines = textOf message |> splitIntoLines

let parsed = parseMail lines