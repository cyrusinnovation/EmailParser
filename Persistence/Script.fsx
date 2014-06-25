#I "bin/Debug/"
#r "EmailParserUtils.dll"
#r "ParserTypes.dll"
#r "System.Data.SQLite.dll"
#r "FSharp.Data.SQLProvider.dll"

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
// System.Environment.SetEnvironmentVariable("MONO_PATH", "lib")  //Easier just to put DLL at top level of project.

// Setting CurrentDirectory above makes the .fs file load when run in F# Interactive, but 
// when the .fsx file is in the editor the #load directive still fails - can't see System.Data.SQLite.dll
#load "SQLitePersistence.fs"

open System
open System.Linq
open FSharp.Data.Sql
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
    EventDate = { Date = System.DateTime.Today; Time = System.DateTime.Today |> Some }
    EventTitle = "title";
    EventLocation = "location" |> Some;
    EventDescription = "A nice description";
    RsvpLink = new System.Uri("http://www.google.com") |> Some
}

let yesterdayEntry = {
    EventDate = { Date = System.DateTime.Today.AddDays(-2.0); Time = None }
    EventTitle = "title2";
    EventLocation = "location2" |> Some; 
    EventDescription = "A nice description2";
    RsvpLink = new System.Uri("http://www.facebook.com") |> Some
}

let emailData = {
    MailDate = System.DateTime.Today;
    MailSender = "Charlie O'Donnell <charlie@odonnell.com>";
    MailIntro = "an\nintro";
    OriginalMessage = "The Original Message String\n";
    CalendarEntries = [calendarEntry; yesterdayEntry]
}

loadMail emailData

let retrieved = retrieveCalendarEntriesFromTodayByDate()

List.head retrieved
List.tail retrieved

let rows =  query { 
                for calendarEntry in dataContext.``[main].[calendar_entries]`` do
                sortBy calendarEntry.timestamp
                sortBy calendarEntry.title
                select calendarEntry  } |> Seq.toList

rows |> List.map (fun row -> row.Delete())
dataContext.SubmitUpdates() 

let shouldBeEmpty =  query { 
                        for calendarEntry in dataContext.``[main].[calendar_entries]`` do
                        select calendarEntry  } |> Seq.toList

let emails = query { for email in dataContext.``[main].[emails]`` do
                     select email  } |> Seq.toList

emails |> List.map (fun row -> row.Delete())
dataContext.SubmitUpdates() 

let noEmails =  query { for email in dataContext.``[main].[emails]`` do
                        select email  } |> Seq.toList

