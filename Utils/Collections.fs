module EmailParser.Utils.Collections

let takeAndSkipWhile f sequence = Seq.takeWhile f sequence, Seq.skipWhile f sequence

let takeAndSkipUntil f = takeAndSkipWhile (f >> not)

let extractWithEmptyStringDefault matchFunction items = 
    items |> Seq.tryPick matchFunction |> (function | Some(aTitle) -> aTitle | None -> "")
