﻿#I "bin/Debug/"
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

let calendarEntry = {
    EventDate = System.DateTime.Today;
    EventTitle = "title";
    EventLocation = "location";
    EventDescription = "A nice description";
    RsvpLink = new System.Uri("http://www.google.com")
}

let yesterdayEntry = {
    EventDate = System.DateTime.Today.AddDays(-1.0);
    EventTitle = "title2";
    EventLocation = "location2";
    EventDescription = "A nice description2";
    RsvpLink = new System.Uri("http://www.facebook.com")
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
