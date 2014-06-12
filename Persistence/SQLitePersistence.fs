module EmailParser.SQLitePersistence

open System
open System.Linq
open FSharp.Data.Sql
open EmailParser.Types
open EmailParser.Utils.Date

type Provider = SqlDataProvider< ConnectionString = @"Data Source=test.sqlitedb;Version=3",
                                  DatabaseVendor = Common.DatabaseProviderTypes.SQLITE,
                                  UseOptionTypes = true >
type DataContext = Provider.dataContext
type EmailType = DataContext.``[main].[emails]Entity``
type CalendarEntryType = DataContext.``[main].[calendar_entries]Entity``

let loadEmailData (dataContext: DataContext) emailData =
    let email = dataContext.``[main].[emails]``.Create()
    email.date <- emailData.MailDate.ToString("yyyy-MM-dd hh:mm:ss") |> Some
    email.sender <- emailData.MailSender |> Some
    email.intro <- emailData.MailIntro |> Some
    dataContext.SubmitUpdates()
    email

let loadCalendarEntry (dataContext: DataContext) (email: EmailType) calendarEntry = 
    let calendar_entry = dataContext.``[main].[calendar_entries]``.Create()
    calendar_entry.date <- calendarEntry.EventDate.ToString("yyyy-MM-dd hh:mm:ss") |> Some
    calendar_entry.title <- calendarEntry.EventTitle |> Some
    calendar_entry.location <- calendarEntry.EventLocation |> Some
    calendar_entry.description <- calendarEntry.EventDescription |> Some
    calendar_entry.rsvp <- calendarEntry.RsvpLink.AbsoluteUri |> Some
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

let dateFrom = function
    | None -> DateTime.Today.AddDays(7.0)
    | Some(dateString) -> DateTime.Parse(dateString)

let toCalendarEntry (dbCalendarEntry: CalendarEntryType) = 
    {
        EventDate = (dbCalendarEntry.date |> dateFrom);
        EventTitle = dbCalendarEntry.title.Value;
        EventLocation = dbCalendarEntry.location.Value;
        EventDescription = dbCalendarEntry.description.Value;
        RsvpLink = new System.Uri(dbCalendarEntry.rsvp.Value)        
    }

let retrieveCalendarEntriesFromTodayByDate() =
    let dataContext = Provider.GetDataContext()
    query {    // Can't filter using string comparison or use optional int columns with SQLProvider.
            for calendarEntry in dataContext.``[main].[calendar_entries]`` do
            sortBy calendarEntry.date
            select calendarEntry  } |> Seq.filter isTodayOrLater 
                                    |> Seq.toList
                                    |> List.map toCalendarEntry
    