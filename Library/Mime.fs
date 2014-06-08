module Mime

open MimeKit

type ReadableContent =
    | HtmlPart of string
    | TextPart of string

let loadMimeMessageFrom(filepath: string) = MimeMessage.Load(filepath)

let rec contentPartsOf (message: MimeMessage) : list<ReadableContent> = contentPartsFromMimeEntity message.Body

and contentPartsFromMimeEntity (entity: MimeEntity) : list<ReadableContent> = 
    match entity with
    | :? MessagePart as part -> contentPartsOf part.Message
    | :? Multipart as parts -> parts |> Seq.map contentPartsFromMimeEntity |> Seq.concat |> List.ofSeq
    | :? MimePart as mimePart -> contentPartsFromMimePart mimePart
    | _ -> []

and contentPartsFromMimePart (mimePart: MimePart) : list<ReadableContent> = 
    match mimePart with
    | attachment when attachment.IsAttachment -> []
    | :? TextPart as textPart -> contentPartFrom textPart
    | anythingElse -> []

and contentPartFrom (textPart: TextPart) : list<ReadableContent> = 
    match textPart with
    | html when html.ContentType.Matches("text", "html") -> [ HtmlPart(html.Text) ]
    | anyOtherText -> [ TextPart(anyOtherText.Text) ]

let htmlPartsOf (message: MimeMessage) : list<string> = 
    contentPartsOf message 
    |> List.choose (function | HtmlPart(content) -> Some(content) | _ -> None )

let textOf (message: MimeMessage) : string = 
    contentPartsOf message 
    |> List.choose (function | TextPart(content) -> Some(content) | _ -> None)
    |> String.concat "\r\n"
