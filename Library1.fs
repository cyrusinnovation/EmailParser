module EmailParser

open FSharp.Data
open Corsis.Xhtml

let convertToXhtml htmlPath = 
    let html = System.IO.File.ReadAllText(htmlPath)
    let streamWriter = System.Action<System.IO.StreamWriter>(fun (writer: System.IO.StreamWriter) -> writer.Write(html))
    Html2Xhtml.RunAsFilter(streamWriter).ReadToEnd()
