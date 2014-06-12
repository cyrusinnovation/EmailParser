module EmailParser.Utils.Html

open System
open EmailParser.Utils.Text

let dataFromHtml (html: string) = 
    let htmlDocument = new HtmlAgilityPack.HtmlDocument()
    htmlDocument.LoadHtml(html)
    htmlDocument

let normalizeTag (tagName: string) (html: string) =
    let openTagPattern  = @"<\s*" + tagName + @"([^>])*>"
    let closeTagPattern = @"(<\s*(/)\s*" + tagName + @"\s*>)" 

    html |> regexReplaceIgnoreCase  openTagPattern   ("<"  + tagName + ">")
         |> regexReplaceIgnoreCase  closeTagPattern  ("</" + tagName + ">")


let toPlainText (html: string) = 
    // Thanks to paceman at http://www.codeproject.com/Articles/11902/Convert-HTML-to-Plain-Text

    html |> replace "\r" " "
         // Replace line breaks with space
         // because browsers insert spaces
         |> replace "\n" " "
         // Remove step-formatting
         |> replace "\t" String.Empty 
         // Remove repeating spaces because browsers ignore them
         |> regexReplace @" +"  " "

         // Remove the header
         |> normalizeTag  "head"
         |> regexReplaceIgnoreCase  @"(<head>).*(</head>)"      String.Empty

         // remove all scripts
         |> normalizeTag "script"
         |> regexReplaceIgnoreCase  @"(<script>).*(</script>)"  String.Empty

         // remove all styles
         |> normalizeTag "style"
         |> regexReplaceIgnoreCase  @"(<style>).*(</style>)"    String.Empty

         // replace <td> tags with tabs
         |> regexReplaceIgnoreCase  @"<\s*td[^>]*>"          "\t"

         // insert line breaks in place of <BR> tags
         |> regexReplaceIgnoreCase  @"<\s*br[^>]*>"          "\n"

         // insert line breaks with stars in place of <LI> tags
         |> regexReplaceIgnoreCase  @"<\s*li[^>]*>"          "\n * "

         // insert line paragraphs (double line breaks) in place
         // of <P>, <DIV> and <TR> tags
         |> regexReplaceIgnoreCase  @"<\s*p[^>]*>"           "\n\n"
         |> regexReplaceIgnoreCase  @"<\s*div[^>]*>"         "\n\n"
         |> regexReplaceIgnoreCase  @"<\s*tr[^>]*>"          "\n\n"

         // insert URLs in brackets
         |> regexReplaceIgnoreCase  @"<\s*a +.*href=""?([^ "">]+)[^>]*>"         "[$1]"

         // Remove remaining tags like <a>, links, images,
         // comments etc - anything that's enclosed inside < >
         |> regexReplaceIgnoreCase  @"<[^>]*>"                 String.Empty

         // replace special characters:
         |> regexReplaceIgnoreCase "&nbsp;"                     " "
         |> regexReplaceIgnoreCase "&bull;"                     " * "
         |> regexReplaceIgnoreCase "&lt;"                       "<"
         |> regexReplaceIgnoreCase "&gt;"                       ">"
         |> regexReplaceIgnoreCase "&lsaquo;"                   "<"
         |> regexReplaceIgnoreCase "&rsaquo;"                   ">"
         |> regexReplaceIgnoreCase "&trade;"                    "(tm)"
         |> regexReplaceIgnoreCase "&copy;"                     "(c)"
         |> regexReplaceIgnoreCase "&reg;"                      "(r)"
         |> regexReplaceIgnoreCase "&frasl;"                    "/"

         // Remove all others. More can be added, see
         // http://hotwired.lycos.com/webmonkey/reference/special_characters/
         |> regexReplaceIgnoreCase @"&.{2,6};"                String.Empty

         // Remove extra line breaks and tabs:
         // Prepare first to remove any whitespaces in between
         // the escaped characters and remove redundant tabs in between line breaks
         |> regexReplaceIgnoreCase "\n\s+\n"                   "\n\n"
         |> regexReplaceIgnoreCase "\t +\t"                    "\t\t"
         |> regexReplaceIgnoreCase "\t +\n"                    "\t\n"
         |> regexReplaceIgnoreCase "\n +\t"                    "\n\t"

         // replace over 2 breaks with 2
         |> regexReplaceIgnoreCase "\n\n\n+"                   "\n\n"
         |> regexReplaceIgnoreCase "\t\t\t\t\t+"               "\t\t\t\t"


