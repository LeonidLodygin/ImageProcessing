namespace ImageProcessing

open Argu
open Arguments
open MyImage
open Types
open ImageArrayProcessing
open Agents

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
            | _ ->
                let composition = List.reduce (>>) listOfFunc

                match System.IO.Path.GetExtension inputPath with
                | "" ->
                    if parser.Contains(Agents) then
                        arrayOfImagesProcessing inputPath outputPath composition On
                    elif parser.Contains(SuperAgents) then
                        let countOfAgents = parser.GetResult(SuperAgents)
                        superImageProcessing inputPath outputPath composition countOfAgents
                    else
                        arrayOfImagesProcessing inputPath outputPath composition Off
                | _ ->
                    let image = loadAsImage inputPath
                    let filtered = composition image
                    saveImage filtered outputPath
        else
            printfn $"No modifications for image processing"
        0
