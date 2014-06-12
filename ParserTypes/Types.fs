module EmailParser.Types

open System

type EmailMessage = {
    Sender: string;
    SentDate: DateTime;
    MessageLines: list<string>
}

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