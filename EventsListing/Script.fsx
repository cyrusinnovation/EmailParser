#I "bin/Debug"
#r "ParserTypes.dll"
#r "EmailParserUtils.dll"

#load "HtmlGenerator.fs"

open EmailParser.Types
open EmailParser.HtmlGenerator

let firstEntry = {
    EventDate = { Date = System.DateTime.Today ; Time = None };
    EventTitle = "title1";
    EventLocation = Some("location1");
    EventDescription = "A nice description1";
    RsvpLink = new System.Uri("http://www.facebook.com") |> Some
}

let secondEntry = {
    EventDate = { Date = System.DateTime.Today; Time = System.DateTime.Today |> Some };
    EventTitle = "title2";
    EventLocation = Some("location2");
    EventDescription = "A nice description2";
    RsvpLink = new System.Uri("http://www.google.com") |> Some
}

let thirdEntry = {
    EventDate = { Date = System.DateTime.Today.AddDays(1.0); Time = None };
    EventTitle = "title3";
    EventLocation = None;
    EventDescription = "A nice description3";
    RsvpLink = None
}

let calendarEntries = [ firstEntry ; secondEntry ; thirdEntry ] 
htmlFrom calendarEntries
