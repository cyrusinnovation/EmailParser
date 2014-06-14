module EmailParser.HtmlGenerator

open System
open EmailParser.Types
open EmailParser.Utils.Text

type HtmlAccumulator = {
    Html: string;
    DateTime: DateAndTime
}

let addHeader calendarEntry accumulator =    
    let currentEntryDate = calendarEntry.EventDate

    if currentEntryDate.Date.ToString("D") = accumulator.DateTime.Date.ToString("D") then accumulator.Html
    else accumulator.Html + "<h3>" + currentEntryDate.Date.ToString("D") + "</h3>\n\n"

let addEntryTime calendarEntry htmlSoFar = 
    match calendarEntry.EventDate.Time with
    | None -> htmlSoFar + "<h4>(all day) - "
    | Some(dateTime) -> htmlSoFar + "<h4>" + dateTime.ToString("t").ToLower() + " - "

let addEntryTitle calendarEntry htmlSoFar = 
    htmlSoFar + calendarEntry.EventTitle + "</h4>\n\n"

let addEntryDescription calendarEntry htmlSoFar =
    htmlSoFar + "<p>" + calendarEntry.EventDescription + "</p>\n"

let addEntryLocation calendarEntry htmlSoFar =
    match calendarEntry.EventLocation with
        | None -> htmlSoFar
        | Some(location) -> htmlSoFar + "<p>" + location + "</p>\n"

let addEntryRsvp calendarEntry htmlSoFar =
    match calendarEntry.RsvpLink with
        | Some(uri) -> htmlSoFar + "<p>RSVP: <a href=" + uri.AbsoluteUri + ">" + uri.AbsoluteUri + "</a></p>\n"
        | None -> htmlSoFar

let htmlEntryFor (accumulator: HtmlAccumulator) (calendarEntry: CalendarEntry) = 
    let entryHtml = accumulator |> (addHeader calendarEntry)
                                |> (addEntryTime calendarEntry)
                                |> (addEntryTitle calendarEntry)
                                |> (addEntryDescription calendarEntry)
                                |> (addEntryLocation calendarEntry)
                                |> (addEntryRsvp calendarEntry)

    { Html = entryHtml; DateTime = calendarEntry.EventDate }

let rec toHtml accumulator calendarEntries =
    match calendarEntries with
        | calendarEntry :: remainingEntries -> toHtml (htmlEntryFor accumulator calendarEntry) remainingEntries    
        | [] -> { Html = accumulator.Html + "\n</body>\n</html>" ; DateTime = accumulator.DateTime }

let htmlFrom calendarEntries = 
    let initialState = { Html = "<html>\n<body>\n" ; DateTime = { Date = DateTime.Today.AddDays(-1.0) ; Time = None } }
    let accumulated = toHtml initialState calendarEntries
    accumulated.Html
