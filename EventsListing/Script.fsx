#I "bin/Debug"
#r "ParserTypes.dll"
#r "EventsListing.dll"

open EmailParser.Types
open EmailParser.HtmlGenerator

let firstEntry = {
    EventDate = System.DateTime.Today;
    EventTitle = "title1";
    EventLocation = "location1";
    EventDescription = "A nice description1";
    RsvpLink = new System.Uri("http://www.facebook.com")
}

let secondEntry = {
    EventDate = System.DateTime.Today;
    EventTitle = "title2";
    EventLocation = "location2";
    EventDescription = "A nice description2";
    RsvpLink = new System.Uri("http://www.google.com")
}

let thirdEntry = {
    EventDate = System.DateTime.Today.AddDays(1.0);
    EventTitle = "title3";
    EventLocation = "location3";
    EventDescription = "A nice description3";
    RsvpLink = new System.Uri("http://www.foobar.com")
}

let calendarEntries = [ firstEntry ; secondEntry ; thirdEntry ] 
htmlFrom calendarEntries
