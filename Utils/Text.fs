module EmailParser.Utils.Text

open System.IO
open System.Text
open System.Text.RegularExpressions

let isBlank (text: string) = System.String.IsNullOrWhiteSpace(text)

let startsWithOneOf (strings: seq<string>) (text: string) = 
    strings |> Seq.exists (fun prefix -> text.StartsWith(prefix))

let stringToStream (text: string) = new MemoryStream(Encoding.UTF8.GetBytes(text))

let replace (toBeReplaced: string) (replacement: string) (text: string) =
    text.Replace(toBeReplaced, replacement)

let regexReplace (pattern: string) (replacement: string) (text: string) =
    Regex.Replace(text, pattern, replacement)

let regexReplaceIgnoreCase (pattern: string) (replacement: string) (text: string) =
    Regex.Replace(text, pattern, replacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

let splitIntoLines (text: string) = 
    let lineArray = text.Replace("\r\n", "\n").Split( [|'\n'|] )
    List.ofArray lineArray

let normalizeSpace (text: string) = text |> regexReplace " +"  " "

let asciiSubstitutions = [
        ("’",  "'") ;
        ("–",  "-") ;
        ("‘",  "'") ;
        ("”",  "\"") ;
        ("“",  "\"") ;
        ("…",  "...") ;
        ("£",  "GBP") ;
        ("•",  "*") ;
        (" ",  " ") ;
        ("é",  "e") ;
        ("ï",  "i") ;
        ("´",  "'") ;
        ("—",  "-") ;
        ("·",  "*") ;
        ("„",  "\"") ;
        ("€",  "EUR") ;
        ("®",  "(R) ;") ;
        ("¹",  "(1) ;") ;
        ("«",  "\"") ;
        ("è",  "e") ;
        ("á",  "a") ;
        ("™",  "TM") ;
        ("»",  "\"") ;
        ("ç",  "c") ;
        ("½",  "1/2") ;
        ("­",  "-") ;
        ("°",  " degrees ") ;
        ("ä",  "a") ;
        ("É",  "E") ;
        ("‚",  ",") ;
        ("ü",  "u") ;
        ("í",  "i") ;
        ("ë",  "e") ;
        ("ö",  "o") ;
        ("à",  "a") ;
        ("¬",  " ") ;
        ("ó",  "o") ;
        ("â",  "a") ;
        ("ñ",  "n") ;
        ("ô",  "o") ;
        ("¨",  "") ;
        ("å",  "a") ;
        ("ã",  "a") ;
        ("ˆ",  "") ;
        ("©",  "(c) ;") ;
        ("Ä",  "A") ;
        ("Ï",  "I") ;
        ("ò",  "o") ;
        ("ê",  "e") ;
        ("î",  "i") ;
        ("Ü",  "U") ;
        ("Á",  "A") ;
        ("ß",  "ss") ;
        ("¾",  "3/4") ;
        ("È",  "E") ;
        ("¼",  "1/4") ;
        ("†",  "+") ;
        ("³",  "'") ;
        ("²",  "'") ;
        ("Ø",  "O") ;
        ("¸",  ",") ;
        ("Ë",  "E") ;
        ("ú",  "u") ;
        ("Ö",  "O") ;
        ("û",  "u") ;
        ("Ú",  "U") ;
        ("Œ",  "Oe") ;
        ("º",  "?") ;
        ("‰",  "0/00") ;
        ("Å",  "A") ;
        ("ø",  "o") ;
        ("˜",  "~") ;
        ("æ",  "ae") ;
        ("ù",  "u") ;
        ("‹",  "<") ;
        ("±",  "+/-") ]

let asciify (text: string) = 
    asciiSubstitutions 
        |> List.fold (fun textSoFar (toBeReplaced, replacement) -> replace toBeReplaced replacement textSoFar) text
