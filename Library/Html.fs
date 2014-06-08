module EmailParser.Html

let dataFromHtml(html: string) = 
    let htmlDocument = new HtmlAgilityPack.HtmlDocument()
    htmlDocument.LoadHtml(html)
    htmlDocument