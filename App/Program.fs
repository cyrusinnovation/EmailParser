open EmailParser

[<EntryPoint>]
let main argv = 
    let xhtml = convertToXhtml "ODonnell.html"
    printfn "%s" xhtml
    0
