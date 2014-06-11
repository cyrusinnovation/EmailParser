module EmailParser.Utils.Date

open System

let secondsSinceEpoch (date: DateTime) =
    let timeSpanSince1970 = date.Subtract(new DateTime(1970,1,1,0,0,0))
    Convert.ToInt64(timeSpanSince1970.TotalSeconds)