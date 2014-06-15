EmailParser
===========

An F# project to parse/scrape MIME mail "calendar of events" messages, store the results in a database, 
and display the consolidated result.

This is currently a command line application. EmailParser.exe takes a list of email files whose paths
should be specified relative to the application directory. The output is an Events.html file in the 
application directory.

The suffixes of the email files indicate which parser will be used on them. Currently, the two mails
that can be parsed are the weekly NYC tech/startup calendars of events from Charlie O'Donnell
(which should be in a file with the .odonnell suffix) and from Frank Denbow (which should be in a
file with the .denbow suffix). The files should contain emails in MIME format.

The html report contains a listing of all events from the current date forward. The events are stored
in a local sqlite database (events.sqlitedb).

This application will run under mono (and in fact was entirely written using Xamarin Studio and mono on 
OS X). Currently compilation is entirely dependent on an IDE build.
