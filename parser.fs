module Parser

open System
open System.Text.RegularExpressions
open IO

// Adapted from https://github.com/alantheprice/es6-import/blob/master/src/lets.js
// TODO: Look into using an f# parser library instead of regex (fparsec)
let IMPORT_REGEX =
    "(?:^|\\n)\\s*import\\s+[{}a-zA-Z1-9\\-,_\\s]*from\\s+['`\"](.*)['`\"]";
let REQUIRE_REGEX = "require\\(\\s*['`\"](.*)['`\"]\\s*\\)";

let getCaptureGroup regexStr n (str: string) =
    Regex.Matches(str, regexStr, RegexOptions.Compiled)
    |> Seq.cast<Match>
    |> Seq.map (fun (m: Match) -> m.Groups |> Seq.item n |> (fun x -> x.Value))

let getImportMatches = getCaptureGroup IMPORT_REGEX 1
let getRequireMatches = getCaptureGroup REQUIRE_REGEX 1

///<summary>
/// Returns a list of all the paths referenced in import and require statements
///</summary>
let getRefsFromFileContent fileContent =
    getImportMatches fileContent
    |> Seq.append (getRequireMatches fileContent)
    |> Seq.distinct

