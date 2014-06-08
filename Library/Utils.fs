module EmailParser.Utils

let takeAndSkipWhile f sequence = Seq.takeWhile f sequence, Seq.skipWhile f sequence

let takeAndSkipUntil f = takeAndSkipWhile (f >> not)

