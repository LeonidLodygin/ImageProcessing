module Arguments

open CpuImageProcessing
open Argu

let first (x, _, _) = x
let second (_, x, _) = x
let third (_, _, x) = x

type Kernel =
    | Gauss5x5
    | Gauss7x7
    | Edges
    | Sharpen
    | Emboss

let kernelParser kernel =
    match kernel with
    | Gauss5x5 -> gaussianBlurKernel
    | Gauss7x7 -> gaussianBlur7x7Kernel
    | Edges -> edgesKernel
    | Sharpen -> sharpenKernel
    | Emboss -> embossKernel

type CliArguments =
    | [<First; AltCommandLine("-ft")>] Filter of inputPath: string * outputPath: string * filter: Kernel
    | [<First; AltCommandLine("-rt")>] Rotate of inputPath: string * outputPath: string * side: bool

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Filter _ ->
                "Apply filter on image or the image directory. Required parameters: input path, output path and filter(Gauss5x5, Gauss7x7, Sharpen, Edges, Emboss)."
            | Rotate _ ->
                "Rotate the image or the image directory 90 degrees to the right or left. Required parameters: input path, output path and side(right or left)."
