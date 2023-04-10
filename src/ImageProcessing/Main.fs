namespace ImageProcessing

open Argu
open Arguments
open MyImage
open Types
open ImageArrayProcessing
open Agents
open Brahma.FSharp

module Main =

    [<EntryPoint>]
    let main (argv: string array) =
        let parser = ArgumentParser.Create<CliArguments>().ParseCommandLine argv
        let inputPath = parser.GetResult(InputPath)
        let outputPath = parser.GetResult(OutputPath)
        if parser.Contains(Modifications) then
            let listOfFunc = parser.GetResult(Modifications)
            let filters =
                if parser.Contains(GpGpu) then
                    let device = parser.GetResult(GpGpu)
                    match device with
                    | AnyGpu ->
                        List.map (fun n -> modificationGpuParser n (ClContext(ClDevice.GetFirstAppropriateDevice())) 64) listOfFunc
                    | Nvidia ->
                        List.map (fun n -> modificationGpuParser n (ClContext(ClDevice.GetFirstAppropriateDevice(Platform.Nvidia))) 64) listOfFunc
                    | Amd ->
                        List.map (fun n -> modificationGpuParser n (ClContext(ClDevice.GetFirstAppropriateDevice(Platform.Amd))) 64) listOfFunc
                    | Intel ->
                        List.map (fun n -> modificationGpuParser n (ClContext(ClDevice.GetFirstAppropriateDevice(Platform.Intel))) 64) listOfFunc
                else
                    listOfFunc |> List.map modificationParser
            let composition = List.reduce (>>) filters
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
