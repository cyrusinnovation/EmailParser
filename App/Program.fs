open EmailParser.Utils.Mime
open EmailParser.Utils.Text
open ODonnellParser.Parser

[<EntryPoint>]
let main argv = 
    System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
    let message = loadMimeMessageFrom("../ODonnellParser/ODonnell.eml")
    let sender = senderOf message
    let sentDate = dateOf message
    let lines = textOf message |> splitIntoLines

    let parsed = parseMail sender sentDate lines
    0
