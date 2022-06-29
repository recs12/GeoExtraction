(*
    Commande line standalone executable.
    Generate csv with following columns:
    FolderPath, Filename,TotalNumberEngravings, Engraving, LineNumber, Text, OverwritingText
    Look for *.GEO files
*)

open System
open System.IO
open FSharp.Collections


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
    // let tuplesGravures = Array.Parallel.map (fun x -> (fst(x)+6, LookUpForGravure listOfWords  (fst(x)))) tuplesTxt
    let tuplesGravures = Array.Parallel.map (fun (x,_) -> ((x+6), LookUpForGravure listOfWords  x )) tuplesTxt

    // We create 3 arrays and zip them.
    let _index:array<int> = [|1 .. tuplesGravures.Length|]

    // unzip method could be used.
    let (colOfLineNumbers, colOfGravures) = Array.unzip (tuplesGravures)

    // zip3
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

    printfn "[INPUT] Enter the root folder for GEOs:" // No quotes needed in the input path.
    let rootGeoDir = Console.ReadLine()

    printfn "[INPUT] Enter the path to the directory you want to create the report:" // No quotes needed in the input path.
    let reportCSVDir = Console.ReadLine()
    let CSVFile = Path.Combine(reportCSVDir, "gravures.csv")

    createCSV  CSVFile |>ignore
    for _file in (globglob rootGeoDir) do
        let fileName_ = Path.GetFileName(_file) 
        let directoryName_ = Path.GetDirectoryName(_file) 
        let (totalGravures, idxAndGravure) = GetFileGravures _file
        idxAndGravure 
            |> Array.iter (fun (idx, line, mark) -> appendCSV CSVFile directoryName_ fileName_ totalGravures idx  (line + 1)  mark)
            
    printfn "CSV successfully created:\n%A" CSVFile
    printfn "Press any key to exit..."
    Console.ReadLine()|> ignore
    0