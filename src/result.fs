module result

open System

type Result = {
    filename: string
    oldPath: string
    fullOldPath: string

    potentials: string seq option
    resultRef: string option

    first: string option
    random: string option
    closest: string option
    editDistance: string option

    message: string option
    time: int
}

let buildResolveObj filename oldPath fullOldPath =
    { Result.filename = filename
      oldPath = oldPath
      fullOldPath = fullOldPath
      potentials = None
      resultRef = None
      first = None
      random = None
      closest = None
      editDistance = None
      message = None
      time = 0 }
