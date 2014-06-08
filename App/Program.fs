open EmailParser.Mime
open EmailParser.Html
open EmailParser.Text
open EmailParser.ODonnell

[<EntryPoint>]
let main argv = 
    System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
    let message = loadMimeMessageFrom("../Library/ODonnell.eml")
    let lines = textOf message |> splitIntoLines
    let parsed = parseMail lines
    0
