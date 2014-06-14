module EmailParser.Utils.Date

open System
open EmailParser.Utils.Text

let secondsSinceEpoch (date: DateTime) =
    let timeSpanSince1970 = date.Subtract(new DateTime(1970,1,1,0,0,0))
    Convert.ToInt64(timeSpanSince1970.TotalSeconds)

let monthIndexFrom (monthString: string) = 
    let months = [| "jan"; "feb"; "mar"; "apr"; "may"; "jun"; "jul"; "aug"; "sep"; "oct"; "nov"; "dec" |]
    let thisMonth = monthString.Substring(0,3).ToLower()
    let index = Array.IndexOf(months, thisMonth)
    if index = -1 then 1 else (index + 1)

let dateFromMonthDay (monthString: string) (dayString: string) = 
    let monthIndex =  monthString.Substring(0,3).ToLower() |> monthIndexFrom
    let day = regexReplace @"\D*" "" dayString |> Convert.ToInt32
    let year = DateTime.Today.Year

    let proposedDate = new DateTime(year, monthIndex, day)
    if proposedDate < DateTime.Today.AddMonths(-1) then       // Events have no years. If the date is less than a month ago, this may be a next year event
        proposedDate.AddYears(1)
    else
        proposedDate

//TODO Distinguish between events with a midnight time and those with no time at all            
let hoursAndMinutesFrom (timeString: string) =      //Assumption that this ends with am/pm
    if System.String.IsNullOrWhiteSpace(timeString) then
        (0.0, 0.0)
    else
        let time = timeString.Substring(0, timeString.Length - 2).Trim() |> regexReplace @"\s+" ""
        let hoursAndMinutes = time.Split(':') |> Array.map (fun numString -> (Convert.ToDouble numString))
        let hours = if timeString.ToLower().EndsWith("pm") 
                    then hoursAndMinutes.[0] + 12.0
                    else hoursAndMinutes.[0]
        let minutes = if hoursAndMinutes.Length = 2 then hoursAndMinutes.[1] else 0.0
        hours, minutes