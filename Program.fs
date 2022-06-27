(*
    Commande line standalone executable.
    Generate csv with following columns:
    Look for *.GEO files
    FolderPath, Filename,TotalNumberEngravings, Engraving, LineNumber, Text, OverwritingText
*)

open System
open System.IO

// cmd will prompt for root path
let csv = @"C:\Users\recs\OneDrive - Premier Tech\Bureau\GEO\MGM TEST FILES\GEO\engraves.csv"


let globglob (folder:string) =
    let files = Directory.GetFileSystemEntries(folder, "*.GEO", SearchOption.AllDirectories)
    files
    

let FindIndex arr elem = 
    arr 
    |> Array.tryFindIndex ((=) elem)
    

let countNumberOfTxt (listOfWords : array<string>) :int =
    let result = Array.filter (fun x -> x = "TXT") listOfWords
    result.Length

let countNumberOfTxtAndIndex (listOfWords : array<string>) :array<int*string> =
    let result = Array.filter (fun x -> x = "TXT") listOfWords
    let _end:int = result.Length - 1
    let txtsIndex = [|0 .. _end|]
    Array.zip txtsIndex result
    

let LookUpForTXT (collection:string[]) (idx:option<int>) :string =
    match idx with
    | Some idx -> collection[(+) idx 6] // add six extra line to get the gravure index line
    | None -> ""
    

let GetFileGravure geoPath=
    let CollectionElements = File.ReadAllLines(geoPath)
    let numberOfInstanceOfTxt: int =  countNumberOfTxt CollectionElements
    
    let indexWhereTXT = FindIndex CollectionElements "TXT"
    let gravure = LookUpForTXT CollectionElements indexWhereTXT

    (gravure, indexWhereTXT)
    // (total, {(gravure, line);(gravure, line);(gravure, line);})
    

let createCSV (_path:string) = 
    use file = File.Create(_path)
    let bytes = System.Text.Encoding.UTF8.GetBytes("FolderPath, Filename,TotalNumberEngravings, Engraving, LineNumber, Text, OverwritingText\n")
    file.WriteAsync(ReadOnlyMemory bytes)


let appendCSV (_path:string) folderPath filename totalNumberOfEngravings engraving engravingLineNumber  engravingText overwritingText =
    use file = File.AppendText(_path)
    file.WriteLine($"{folderPath},{filename},{totalNumberOfEngravings},{engraving},{engravingLineNumber},{engravingText},{overwritingText},")




[<EntryPoint>]
let main args =
    createCSV  csv|>ignore
    for _file in (globglob args[0]) do

        let fileName_ = Path.GetFileName(_file) 
        let directoryName_ = Path.GetDirectoryName(_file) 

        let (grave, num) = GetFileGravure _file
        let numValue = 
            match num with
            | Some(num) -> num.ToString()
            | None -> ""
        
        // match numberofTxt with 
        // | 1 -> appendCSV
        // | _ -> loop appendCSV
        
        appendCSV csv directoryName_ fileName_ "?" "?"  numValue  grave  |> ignore
    0



// todo : check if agrs[0] is provided with match case.
// todo : check for number of TXT in file.
// todo : think of how to loop to the number of number of TXT
// todo : line of the gravure minus 1
// todo : look for gravure with index number of number
// exemple:: give index -> get content
   



