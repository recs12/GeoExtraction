open System
open System.IO


let Glob (folder:string) =
    let files = Directory.GetFileSystemEntries(folder, "*.GEO", SearchOption.AllDirectories)
    files
    

let FindIndex arr elem = 
    arr 
    |> Array.tryFindIndex ((=) elem)
    

let LookUpForTXT (collection:string[]) (idx:option<int>) :string =
    match idx with
    | Some idx -> collection[(+) idx 6] // add six extra line to get the gravure index line
    | None -> ""
    

let GetFileGravure geoPath=
    let CollectionElements = File.ReadAllLines(geoPath)
    // let NumberOfGravure = Array.countBy CollectionElements "TXT" 
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


let csv = @"C:\Users\recs\OneDrive - Premier Tech\Bureau\GEO\MGM TEST FILES\GEO\engraves.csv"


[<EntryPoint>]
let main args =
    createCSV  csv|>ignore
    for _file in (Glob args[0]) do
        let fileName_ = Path.GetFileName(_file) 
        let directoryName_ = Path.GetDirectoryName(_file) 
        let (grave, num) = GetFileGravure _file
     
        let numValue = 
            match num with
            | Some(num) -> num.ToString()
            | None -> ""
        appendCSV csv directoryName_ fileName_ "?" "?"  numValue  grave  |> ignore
    0



// todo : check if agrs[0] is provided with match case.
// todo : check for number of TXT in file.
// todo : think of how to loop to the number of number of TXT
// todo : line of the gravure minus 1
   



