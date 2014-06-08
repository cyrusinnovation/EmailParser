#I "../Library/bin/Debug/"
#r "EmailParser.dll"
#r "MimeKitLite.dll"
#r "HtmlAgilityPack.dll"

open Mime
open Html

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let message = loadMimeMessageFrom("../Library/ODonnell.eml")
let htmls = htmlPartsOf message

let html = htmls.Head

let data = dataFromHtml(html)
let documentNode = data.DocumentNode

let contentNode = documentNode.ChildNodes.FindFirst("p").ParentNode
