module EmailParser.Utils.Text

open System.Text.RegularExpressions

let splitIntoLines (text: string) = 
    let lineArray = text.Replace("\r\n", "\n").Split( [|'\n'|] )
    List.ofArray lineArray

 