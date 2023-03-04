namespace ImageProcessing

open Argu
open Arguments
open CpuImageProcessing
open ImageArrayProcessing

module Main =

    [<EntryPoint>]
    let main (argv: string array) =
        let parser = ArgumentParser.Create<CliArguments>().ParseCommandLine argv
        let inputPath = parser.GetResult(InputPath)
        let outputPath = parser.GetResult(OutputPath)

        if parser.Contains(Modifications) then
            let listOfFunc = parser.GetResult(Modifications) |> List.map modificationParser
            match listOfFunc with
            | [] -> printfn $"List of modifications is empty"
            | _ -> let composition = List.reduce (>>) listOfFunc
                   match System.IO.Path.GetExtension inputPath with
                   | "" -> arrayOfImagesProcessing inputPath outputPath composition
                   | _ ->
                       let arr = loadAs2DArray inputPath
                       let filtered = composition arr
                       save2DByteArrayAsImage filtered outputPath
        else
            printfn $"No modifications for image processing"

        0
