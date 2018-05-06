module Path

open System
open System.IO
open FSharp.Core


let normalizePath = Path.GetFullPath

let private altSep = Path.AltDirectorySeparatorChar
let private sepChar = Path.DirectorySeparatorChar
let pathSep = Path.DirectorySeparatorChar.ToString()

let pathJoin m n = normalizePath (Path.Combine(m, n))

let toAbsolute (filePath: string) =
    Path.GetFullPath(filePath)

let baseName (filePath: string) =
    Path.GetFileName(filePath)

let dirName (filePath: string) =
    Path.GetDirectoryName(filePath)

let private hasExtn (path: string) = Path.HasExtension(path)
let private endsWithSep (path: string) = path.EndsWith(pathSep)

let cwd = Directory.GetCurrentDirectory() + "/"

let private nor a b = not (a || b)

///<summary>
/// Append a slash only if the path is a directory and does not have a slash.
///</summary>
let shouldAppendSlash (path: string) = nor (hasExtn path) (endsWithSep path)

let appendDirectorySeparatorChar (path: string) =
    if shouldAppendSlash path then path + pathSep else path

///<summary>
/// borrowed from https://stackoverflow.com/a/32113484/567493
///</summary>
let getRelativePath (fromPath: string) (toPath: string) =
    if String.IsNullOrEmpty(fromPath) || String.IsNullOrEmpty(toPath) then
        None
    else
        let fromUri = new Uri(appendDirectorySeparatorChar fromPath)
        let toUri = new Uri(appendDirectorySeparatorChar toPath)

        if fromUri.Scheme <> toUri.Scheme then
            Some toPath
        else
            let relativeUri = fromUri.MakeRelativeUri(toUri)
            let relativePath = Uri.UnescapeDataString(relativeUri.ToString())
            if String.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase) then
                Some (relativePath.Replace(altSep, sepChar))
            else
                Some relativePath