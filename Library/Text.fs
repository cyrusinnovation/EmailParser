module EmailParser.Text

let splitIntoLines (text: string) = 
    let lineArray = text.Replace("\r\n", "\n").Split( [|'\n'|] )
    List.ofArray lineArray