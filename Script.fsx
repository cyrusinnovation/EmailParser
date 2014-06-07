#r "System.Xml.Linq.dll"
#r "Packages/FSharp.Data.2.0.8/lib/net40/FSharp.Data.dll"
// #r "Packages/MarkupSanitizer.1.0.0.1/lib/net35/TathamOddie.MarkupSanitizer.dll"
// #r "Packages/Html2Xhtml.1.1.2.4/lib/net40/Html2Xhtml.dll"
#r "lib/Html2Xhtml.dll"

open FSharp.Data
//open MarkupSanitizer
open Corsis.Xhtml

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let html = System.IO.File.ReadAllText("ODonnell.html")
let streamWriter = System.Action<System.IO.StreamWriter>(fun (writer: System.IO.StreamWriter) -> writer.Write(html))
let xhtml = Html2Xhtml.RunAsFilter(streamWriter).ReadToEnd
//let xhtml = Sanitizer.SanitizeMarkup(html)

//type Root = XmlProvider<"""file://.ODonnell.html""">
