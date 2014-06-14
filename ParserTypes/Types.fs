module EmailParser.Types

open System

type EmailMessage = {
    Sender: string;
    SentDate: DateTime;
    MessageLines: list<string>
}

type DateAndTime = {
    Date: DateTime;
    Time: option<DateTime>;    //Same datetime as EventDate, but allowing None if no time was specified
}

type CalendarEntry = {
    EventDate: DateAndTime;
    EventTitle: string;
    EventLocation: option<string>;
    EventDescription: string;
    RsvpLink: option<Uri>
}

type EmailData = {
    MailDate: DateTime;
    MailSender: string;
    MailIntro: string;
    CalendarEntries: list<CalendarEntry>
}