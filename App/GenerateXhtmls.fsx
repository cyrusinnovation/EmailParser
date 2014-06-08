#r "System.Xml.Linq.dll"
#r "../Packages/FSharp.Data.2.0.8/lib/net40/FSharp.Data.dll"

#I "../Library/bin/Debug/"
#r "EmailParser.dll"
#r "MimeKitLite.dll"
#r "TathamOddie.MarkupSanitizer.dll"
#r "AntiXssLibrary.dll"
#r "HtmlSanitizationLibrary.dll"

open FSharp.Data
open Mime
open Xhtml

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let message = loadMimeMessageFrom("../Library/ODonnell.eml")
let htmls = htmlPartsOf message

printfn "%s" htmls.Head

let xhtmls = List.map xmlFromHtml htmls

printfn "%s" xhtmls.Head.MarkupText

xhtmls |> Seq.iteri (fun index xhtml -> System.IO.File.WriteAllText((sprintf "TempXhtml-%d.xhtml" index), xhtml.MarkupText))


type Root = FSharp.Data.XmlProvider<"file://./TempXhtml-0.xhtml">