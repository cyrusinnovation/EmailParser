module EmailParser.Types

open System

type CalendarEntry = {
    EventDate: DateTime;
    EventTitle: string;
    EventLocation: string;
    EventDescription: string;
    RsvpLink: Uri
}

type EmailData = {
    MailDate: DateTime;
    MailSender: string;
    MailIntro: string;
    CalendarEntries: list<CalendarEntry>
}