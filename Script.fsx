#r "System.Xml.Linq.dll"
#r "Packages/FSharp.Data.2.0.8/lib/net40/FSharp.Data.dll"
// #r "Packages/MarkupSanitizer.1.0.0.1/lib/net35/TathamOddie.MarkupSanitizer.dll"
// #r "Packages/Html2Xhtml.1.1.2.4/lib/net40/Html2Xhtml.dll"
#r "Packages/MimeKitLite.0.36.0.0/lib/net40/MimeKitLite.dll"
#r "lib/Html2Xhtml.dll"

open FSharp.Data
//open MarkupSanitizer
open Corsis.Xhtml
open MimeKit

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let message = MimeMessage.Load("ODonnell.eml")

let rec htmlPartsOf (message: MimeMessage) : List<string> = htmlPartsFromMimeEntity message.Body

and htmlPartsFromMimeEntity (entity: MimeEntity) : List<string> = 
    match entity with
    | :? MessagePart as part -> htmlPartsOf part.Message
    | :? Multipart as parts -> parts.ToSet() |> Seq.iter htmlPartsFromMimeEntity |> List.collect 
    | :? MimePart as mimePart -> htmlPartsFromMimePart mimePart
    | _ -> []

and htmlPartsFromMimePart (mimePart: MimePart) : List<string> = 
    match mimePart with
    | attachment when attachment.IsAttachment -> ()
    | :? TextPart as textPart -> renderTextPart textPart
    | imagePart when imagePart.ContentType.Matches("image", "*") -> 
        use content = imagePart.ContentObject.Open()
        renderImage content

and renderTextPart (textPart: TextPart) = 
    match textPart with
    | html when html.ContentType.Matches("text", "html") -> renderHtml html
    | anyOtherText -> renderText anyOtherText

and renderImage imageContent = ()
and renderHtml html = printfn "%s" html.Text 
and renderText text = printfn "%s" text.Text

renderMessage message

let html = System.IO.File.ReadAllText("ODonnell.html")
let streamWriter = System.Action<System.IO.StreamWriter>(fun (writer: System.IO.StreamWriter) -> writer.Write(html))
let xhtml = Html2Xhtml.RunAsFilter(streamWriter).ReadToEnd
//let xhtml = Sanitizer.SanitizeMarkup(html)

//type Root = XmlProvider<"""file://.ODonnell.html""">


