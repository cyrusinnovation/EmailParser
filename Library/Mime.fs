module Mime

open MimeKit

let loadMimeMessageFrom(filepath: string) = MimeMessage.Load(filepath)

let rec htmlPartsOf (message: MimeMessage) : list<string> = htmlPartsFromMimeEntity message.Body

and htmlPartsFromMimeEntity (entity: MimeEntity) : list<string> = 
    match entity with
    | :? MessagePart as part -> htmlPartsOf part.Message
    | :? Multipart as parts -> parts |> Seq.map htmlPartsFromMimeEntity |> Seq.concat |> List.ofSeq
    | :? MimePart as mimePart -> htmlPartsFromMimePart mimePart
    | _ -> []

and htmlPartsFromMimePart (mimePart: MimePart) : list<string> = 
    match mimePart with
    | attachment when attachment.IsAttachment -> []
    | :? TextPart as textPart -> htmlPartFrom textPart
    | anythingElse -> []

and htmlPartFrom (textPart: TextPart) : list<string> = 
    match textPart with
    | html when html.ContentType.Matches("text", "html") -> [ html.Text ]
    | anyOtherText -> []

