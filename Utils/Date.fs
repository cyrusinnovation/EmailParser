module EmailParser.Utils.Date

open System
open EmailParser.Utils.Text
open System.Text.RegularExpressions

let secondsSinceEpoch (date: DateTime) =
    let timeSpanSince1970 = date.Subtract(new DateTime(1970,1,1,0,0,0))
    Convert.ToInt64(timeSpanSince1970.TotalSeconds)

let months = [| "jan"; "feb"; "mar"; "apr"; "may"; "jun"; "jul"; "aug"; "sep"; "oct"; "nov"; "dec" |]

let monthIndexFrom (monthString: string) = 
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

let dateFromMonthDayYear (monthString: string) (dayString: string) (yearString: string) = 
    let monthIndex =  monthString.Substring(0,3).ToLower() |> monthIndexFrom
    let day = regexReplace @"\D*" "" dayString |> Convert.ToInt32
    let year = regexReplace @"\D*" "" yearString |> Convert.ToInt32
    new DateTime(year, monthIndex, day)

let isDayOfWeek (dateString: string) =
    let possibleWeekday = dateString.Trim().ToLower()
    possibleWeekday.StartsWith("mon") ||
        possibleWeekday.StartsWith("tue") ||
        possibleWeekday.StartsWith("wed") ||
        possibleWeekday.StartsWith("thu") ||
        possibleWeekday.StartsWith("fri") ||
        possibleWeekday.StartsWith("sat") ||
        possibleWeekday.StartsWith("sun")

let isMonth (dateString: string) =
    let possibleMonth = dateString.Trim().ToLower()
    possibleMonth |> startsWithOneOf months

let isYear (dateString: string) =
    Regex.Match(dateString, @"^\d\d\d\d").Success

let isTime (possibleTime: string) = 
    Regex.Match(possibleTime, @"^\d.*[ap]m$").Success

//Assumes that if there is a time, it ends with am/pm
let hoursAndMinutesFrom (timeString: string) : option<float * float> =      
    if isTime timeString then
        let time = timeString.Substring(0, timeString.Length - 2).Trim() |> regexReplace @"\s+" ""
        let hoursAndMinutes = time.Split(':') |> Array.map (fun numString -> (Convert.ToDouble numString))
        let hours = if timeString.Trim().ToLower().EndsWith("am") 
                    then hoursAndMinutes.[0] 
                    else hoursAndMinutes.[0] + 12.0
        let minutes = if hoursAndMinutes.Length = 2 then hoursAndMinutes.[1] else 0.0
        Some(hours, minutes)
    else
        None