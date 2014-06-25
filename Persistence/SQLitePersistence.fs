module EmailParser.SQLitePersistence

open System
open System.Linq
open FSharp.Data.Sql
open EmailParser.Types
open EmailParser.Utils.Date

type Provider = SqlDataProvider< ConnectionString = @"Data Source=events.sqlitedb;Version=3",
                                  DatabaseVendor = Common.DatabaseProviderTypes.SQLITE,
                                  UseOptionTypes = true >
type DataContext = Provider.dataContext
type EmailType = DataContext.``[main].[emails]Entity``
type CalendarEntryType = DataContext.``[main].[calendar_entries]Entity``

let loadEmailData (dataContext: DataContext) emailData =
    let email = dataContext.``[main].[emails]``.Create()
    email.date <- emailData.MailDate.ToString("yyyy-MM-dd HH:mm:ss") |> Some
    email.timestamp <- emailData.MailDate |> secondsSinceEpoch |> Some 
    email.sender <- emailData.MailSender |> Some
    email.intro <- emailData.MailIntro |> Some
    email.entire_message <- emailData.OriginalMessage |> Some
    dataContext.SubmitUpdates()
    email

let loadCalendarEntry (dataContext: DataContext) (email: EmailType) calendarEntry = 
    let calendar_entry = dataContext.``[main].[calendar_entries]``.Create()
    calendar_entry.date <- calendarEntry.EventDate.Date.ToString("yyyy-MM-dd") |> Some
    calendar_entry.time <- calendarEntry.EventDate.Time |> Option.map (fun time -> time.ToString("HH:mm:ss"))
    calendar_entry.timestamp <- calendarEntry.EventDate.Date |> secondsSinceEpoch |> Some 
    calendar_entry.title <- calendarEntry.EventTitle |> Some
    calendar_entry.location <- calendarEntry.EventLocation
    calendar_entry.description <- calendarEntry.EventDescription |> Some
    calendar_entry.rsvp <- calendarEntry.RsvpLink |> Option.map (fun uri -> uri.AbsoluteUri)
    calendar_entry.email <- email.id
    dataContext.SubmitUpdates()

let loadMail (emailData: EmailData) =
    let dataContext = Provider.GetDataContext()
    let email = loadEmailData dataContext emailData
    emailData.CalendarEntries |> List.iter (loadCalendarEntry dataContext email)

// ************************** DB READ *********************************** //

let isTodayOrLater (calendarEntry: CalendarEntryType) = 
    match calendarEntry.date with
        | None -> true
        | Some(dateString) -> DateTime.Parse(dateString) >= DateTime.Today

let dateAndTimeFrom (date: option<string>) (time: option<string>) : DateAndTime = 
    match date with
    | None -> { Date = DateTime.Today.AddDays(7.0); Time = None }
    | Some(dateString) -> 
        match time with 
        | None -> { Date = DateTime.Parse(dateString) ; Time = None }
        | Some(timeString) -> 
            let dateTime = DateTime.Parse(dateString + " " + timeString)
            { Date = dateTime; Time = Some(dateTime) }

let toCalendarEntry (dbCalendarEntry: CalendarEntryType) = 
    {
        EventDate = (dateAndTimeFrom dbCalendarEntry.date dbCalendarEntry.time);
        EventTitle = dbCalendarEntry.title.Value;
        EventLocation = dbCalendarEntry.location;
        EventDescription = dbCalendarEntry.description.Value;
        RsvpLink = dbCalendarEntry.rsvp |> Option.map (fun link -> new System.Uri(link))      
    }

let retrieveCalendarEntriesFromTodayByDate() =
    let dataContext = Provider.GetDataContext()
    let todayTimestamp = DateTime.Today |> secondsSinceEpoch
    query {    // Can't filter using string comparison with SQLProvider.
            for calendarEntry in dataContext.``[main].[calendar_entries]`` do
            where (calendarEntry.timestamp.Value >= todayTimestamp)
            sortBy calendarEntry.timestamp
            sortBy calendarEntry.title
            select calendarEntry  } |> Seq.toList
                                    |> List.map toCalendarEntry
    