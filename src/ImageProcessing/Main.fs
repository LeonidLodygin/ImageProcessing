namespace ImageProcessing

open Argu
open Arguments
open CpuImageProcessing
open ImageArrayProcessing

module Main =

    [<EntryPoint>]
    let main (argv: string array) =
        (*let parser = ArgumentParser.Create<CliArguments>().ParseCommandLine argv
        let inputPath = parser.GetResult(InputPath)
        let outputPath = parser.GetResult(OutputPath)

        if parser.Contains(Modifications) then
            let listOfFunc = parser.GetResult(Modifications) |> List.map modificationParser

            match listOfFunc with
            | [] -> printfn $"List of modifications is empty"
            | _ ->
                let composition = List.reduce (>>) listOfFunc

                match System.IO.Path.GetExtension inputPath with
                | "" -> arrayOfImagesProcessing inputPath outputPath composition
                | _ ->
                    let arr = loadAs2DArray inputPath
                    let filtered = composition arr
                    save2DByteArrayAsImage filtered outputPath
        else
            printfn $"No modifications for image processing"*)
        let image = loadAsImage "C:\Users\Леонид\Desktop\input\mtest.jpg"
        let filtered = rotate90DegreesImage Right image
        saveImage filtered "C:\Users\Леонид\Desktop\output\mtest.jpg"
        let image2d = loadAs2DArray "C:\Users\Леонид\Desktop\input\mtest.jpg"
        let filtered2 = rotate90Degrees Right image2d
        save2DByteArrayAsImage filtered2 "C:\Users\Леонид\Desktop\output\mtest2.jpg"
        0
