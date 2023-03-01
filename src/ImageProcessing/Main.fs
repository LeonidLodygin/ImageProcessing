namespace ImageProcessing

open Argu
open Arguments
open CpuImageProcessing
open ImageArrayProcessing

module Main =

    [<EntryPoint>]
    let main (argv: string array) =
        let parser = ArgumentParser.Create<CliArguments>().ParseCommandLine argv


        match parser with
        | result when result.Contains(Filter) ->
            let tripleResult = parser.GetResult(Filter)
            let inputPath = first tripleResult
            let outputPath = second tripleResult
            let kernel = third tripleResult |> kernelParser

            match System.IO.Path.GetExtension inputPath with
            | "" -> ArrayOfImagesProcessing inputPath outputPath (applyFilter kernel)
            | _ ->
                let arr = loadAs2DArray inputPath
                let filtered = applyFilter kernel arr
                save2DByteArrayAsImage filtered outputPath

        | result when result.Contains(Rotate) ->
            let tripleResult = parser.GetResult(Rotate)
            let inputPath = first tripleResult
            let outputPath = second tripleResult
            let side = third tripleResult

            match System.IO.Path.GetExtension inputPath with
            | "" -> ArrayOfImagesProcessing inputPath outputPath (rotate90Degrees side)
            | _ ->
                let arr = loadAs2DArray inputPath
                let filtered = rotate90Degrees side arr
                save2DByteArrayAsImage filtered outputPath
        | _ -> printfn $"Unexpected command"

        0
