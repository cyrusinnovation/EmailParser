module EmailParser.SQLiteLoader

open SQLite
open EmailParser.Types

type Database (path: string) =
    inherit SQLiteConnection(path)

    member this.Setup = 
        base.CreateTable<EmailData>() |> ignore
        base.CreateTable<CalendarEntry>() |> ignore
