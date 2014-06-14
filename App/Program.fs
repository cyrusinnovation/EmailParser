open EmailParser.Utils.Mime
open EmailParser.Utils.Text
open EmailParser.SQLitePersistence
open EmailParser.HtmlGenerator

let usage() =
    printfn "usage: EmailParser.exe MAIL_FILE_1 [MAIL_FILE_2 ...]"
    printfn ""
    printfn "This program parses files containing emails in MIME"
    printfn "multipart format such as .eml files. The mails should"
    printfn "contain Calendar of Events information. The information"
    printfn "parsed from the files is stored in a database and then"
    printfn "an HTML report is constructed containing all the events"
    printfn "in the database from the current day onward."
    printfn ""
    printfn "The mail files passed on the command line must have specific "
    printfn "file extensions corresponding to the sender, which determines"
    printfn "the parser to be used. These extensions are:"
    printfn ""
    printfn "   .denbow - extension for Frank Denbow's mails"
    printfn "   .odonnell - extension for Charlie O'Donnell's mails"
    printfn ""
    printfn "Output is written into a file named Events.html in the "
    printfn "current working directory."

let selectParseFunction (fileName: string) = 
    match fileName with
        | filename when filename.EndsWith(".denbow")  -> DenbowParser.Parser.parseMail
        | filename when filename.EndsWith(".odonnell")  -> ODonnellParser.Parser.parseMail
        | _ -> sprintf "Unrecognized file extension for file: %s" fileName |> failwith

let loadDataFrom (filename: string) =
    let message = loadMimeMessageFrom(filename)
    let parseFunction = selectParseFunction filename
    let parsed = parseFunction message
    loadMail parsed
    ()

let writeEventsFile() = 
    let eventsList = retrieveCalendarEntriesFromTodayByDate()
    let html = htmlFrom eventsList
    System.IO.File.WriteAllText("Events.html", html)
    ()

[<EntryPoint>]
let main argv = 
    if argv.Length = 0 
    then usage()
    else argv |> Array.iter loadDataFrom

    writeEventsFile()
    0
