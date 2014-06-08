module ODonnellParser.Utils

open System.Text.RegularExpressions

let removeEventUrlFrom (eventHeader: string) = 
    let startOfEventUrl = eventHeader.IndexOf(" (http")
    eventHeader.Substring(0, startOfEventUrl)

let splitTimeFromTitle (eventHeader: string) = 
    let separatorIndex = eventHeader.IndexOf(" - ")
    let time = eventHeader.Substring(0, separatorIndex)
    let title = eventHeader.Substring(separatorIndex + 3)
    (time, title)

let extractRsvpLink (rsvpLinkText: string) = 
    let startIndex = rsvpLinkText.IndexOf("(") + 1
    let endIndex = rsvpLinkText.IndexOf(")")
    let length = endIndex - startIndex
    rsvpLinkText.Substring(startIndex, length)

let (|ADateHeader|_|) (line: string) =
    let regex = Regex(@"^(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday),? ")
    let m = regex.Match(line)
    if m.Success
    then Some()
    else None

let (|AnEventHeader|_|) (line: string) =
    let regex = Regex(@"^\d\d?:?\d?\d?[AP]M -")
    let m = regex.Match(line)
    if m.Success
    then Some()
    else None
