module EmailParser.Utils.Uri


let uriFrom (uriString: string) = 
    let link = match uriString with
                        | str when System.String.IsNullOrWhiteSpace(str) -> None
                        | str when not (str.Contains("://")) -> "http://" + str |> Some
                        | str -> Some(str)
    match link with 
        | Some(str) -> try new System.Uri(str) |> Some with | ex -> None 
        | None -> None
