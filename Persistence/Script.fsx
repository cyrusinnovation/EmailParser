#I "bin/Debug/"
#r "ParserTypes.dll"
#r "DbLoader.dll"
#r "System.Data.SQLite.dll"
#r "FSharp.Data.SQLProvider.dll"

open System
open System.Linq
open FSharp.Data.Sql

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
// System.Environment.SetEnvironmentVariable("MONO_PATH", "lib")  //Easier just to put DLL at top level of project.

open EmailParser.Types
open EmailParser.SQLitePersistence


type Provider = SqlDataProvider< ConnectionString = @"Data Source=events.sqlitedb;Version=3",
                                  DatabaseVendor = Common.DatabaseProviderTypes.SQLITE,
                                  UseOptionTypes = true >
let dataContext = Provider.GetDataContext()

let email = dataContext.``[main].[emails]``.Create()
email.date <- Some("2014-12-12 12:00:00")
email.timestamp <- Some(1418385600L)
email.sender <- Some("sender")
email.intro <- Some("intro")
dataContext.SubmitUpdates() 

let calendarEntry = {
    EventDate = System.DateTime.Today;
    EventTitle = "title";
    EventLocation = "location" |> Some;
    EventDescription = "A nice description";
    RsvpLink = new System.Uri("http://www.google.com") |> Some
}

let yesterdayEntry = {
    EventDate = System.DateTime.Today.AddDays(-1.0);
    EventTitle = "title2";
    EventLocation = "location2" |> Some; 
    EventDescription = "A nice description2";
    RsvpLink = new System.Uri("http://www.facebook.com") |> Some
}

let emailData = {
    MailDate = System.DateTime.Today;
    MailSender = "Charlie O'Donnell <charlie@odonnell.com>";
    MailIntro = "an\nintro";
    CalendarEntries = [calendarEntry; yesterdayEntry]
}

loadMail emailData

let retrieved = retrieveCalendarEntriesFromTodayByDate()

List.head retrieved
List.tail retrieved

let rows =  query { 
                for calendarEntry in dataContext.``[main].[calendar_entries]`` do
                sortBy calendarEntry.timestamp
                select calendarEntry  } |> Seq.toList

rows |> List.map (fun row -> row.Delete())
dataContext.SubmitUpdates() 

let shouldBeEmpty =  query { 
                        for calendarEntry in dataContext.``[main].[calendar_entries]`` do
                        sortBy calendarEntry.timestamp
                        select calendarEntry  } |> Seq.toList

let emails = query { for email in dataContext.``[main].[emails]`` do
                     sortBy email.timestamp
                     select email  } |> Seq.toList

emails |> List.map (fun row -> row.Delete())
dataContext.SubmitUpdates() 

let noEmails =  query { for email in dataContext.``[main].[emails]`` do
                        sortBy email.timestamp
                        select email  } |> Seq.toList

