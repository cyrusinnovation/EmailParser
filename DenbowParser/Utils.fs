module DenbowParser.Utils

//open System
//open System.Text.RegularExpressions
open MimeKit
open EmailParser.Utils.Mime
open EmailParser.Utils.Html
open EmailParser.Utils.Text
open EmailParser.Types

let messageDataFor (message: MimeMessage) =
    let messageLines = htmlPartsOf message |> String.concat "\n" |> toPlainText |> splitIntoLines
    {
        Sender = senderOf message;
        SentDate = dateOf message;
        MessageLines = messageLines
    }
