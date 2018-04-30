module result

open System

type Result = {
    filename: string
    oldPath: string
    fullOldPath: string
    potentials: string seq
    first: string option
    random: string option
    closest: string option
    editDistance: string option
}