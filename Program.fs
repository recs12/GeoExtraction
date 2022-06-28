(*
    Commande line standalone executable.
    Generate csv with following columns:
    FolderPath, Filename,TotalNumberEngravings, Engraving, LineNumber, Text, OverwritingText
    Look for *.GEO files
*)

open System
open System.IO

// cmd will prompt for output path.
// let report = @"C:\Users\recs\OneDrive - Premier Tech\Bureau\GEO\MGM TEST FILES\GEO\engraves.csv"
let report = Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"), "Downloads" )

let globglob (folder:string) =
    Directory.GetFileSystemEntries(folder, "*.GEO", SearchOption.AllDirectories)

// Give the TXT line number.
let LookUpForGravure (collection:string[]) (idx:int) :string =
    collection[(+) idx 6] // add six extra line to get the gravure index line

let countNumberOfTxtAndLineNumbers (listOfWords : array<string>) =
    // We create an indexed content of the file
    let _end:int = listOfWords.Length - 1
    let lineNumber:array<int> = [|0 .. _end|]
    let tuplesAll = Array.zip lineNumber listOfWords

    // We filter only the lines with <"TXT">
    let tuplesTxt = Array.filter (fun x -> snd(x) = "TXT") tuplesAll

    // We translate to the 6th line after the tag <"TXT">
    let tuplesGravures = Array.map (fun x -> (fst(x)+6, LookUpForGravure listOfWords  (fst(x)))) tuplesTxt

    // We create 3 arrays and zip them.
    let _index:array<int> = [|1 .. tuplesGravures.Length|]
    let colOfLineNumbers:array<int> = Array.map  (fun x -> fst(x))  tuplesGravures
    let colOfGravures:array<string> = Array.map  (fun x -> snd(x))  tuplesGravures
    let tuplesWithIndex = Array.zip3 _index colOfLineNumbers colOfGravures
    
    // Number of gravures in the .GEO
    let total:int = tuplesGravures.Length

    // (total , (index * line * gravure))
    (total, tuplesWithIndex)

let GetFileGravures (geoPath:string)=
    let CollectionElements = File.ReadAllLines(geoPath)
    countNumberOfTxtAndLineNumbers CollectionElements

let createCSV (_path:string) = 
    use file = File.Create(_path)
    let bytes = System.Text.Encoding.UTF8.GetBytes("FolderPath, Filename,TotalNumberEngravings, Engraving, LineNumber, Text, OverwritingText\n")
    printfn "|%*s|%*s|%*s|%*s|%*s|%*s|%*s|" 85 "FolderPath" 20 "Filename" 3 "Tot" 3 "Num" 5 "Line" 10 "Text" 10 "Overwrite" 
    file.WriteAsync(ReadOnlyMemory bytes)

let appendCSV (_path:string) (folderPath:string) (filename:string) (totalNumberOfEngravings:int) (engraving:int) (engravingLineNumber:int)  (engravingText:string) =
    use file = File.AppendText(_path)
    // add padding
    printfn "|%*s|%*s|%*i|%*i|%*i|%*s|%*s|" 85 folderPath 20 filename 3 totalNumberOfEngravings 3 engraving 5 engravingLineNumber 10 engravingText 10 "-"
    file.WriteLine($"{folderPath},{filename},{totalNumberOfEngravings},{engraving},{engravingLineNumber},{engravingText},")


[<EntryPoint>]
let main args =
    printfn "Enter the path to the directory you want to create the report:"
    let reportCSVDir = Console.ReadLine()
    let CSVFile = Path.Combine(reportCSVDir, "gravures.csv")
    printfn "Given path: %A" CSVFile
    createCSV  CSVFile |>ignore
    for _file in (globglob args[0]) do
        let fileName_ = Path.GetFileName(_file) 
        let directoryName_ = Path.GetDirectoryName(_file) 
        let (totalGravures, idxAndGravure) = GetFileGravures _file
        for idx, line, mark in idxAndGravure do
            appendCSV CSVFile directoryName_ fileName_ totalGravures idx  (line + 1)  mark |> ignore
    0   