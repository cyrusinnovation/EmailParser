module EmailParser.Utils.Text

let splitIntoLines (text: string) = 
    let lineArray = text.Replace("\r\n", "\n").Split( [|'\n'|] )
    List.ofArray lineArray