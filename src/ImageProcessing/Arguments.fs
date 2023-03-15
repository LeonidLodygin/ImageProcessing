module Arguments

open CpuImageProcessing
open Argu

type Modifications =
    | Gauss5x5
    | Gauss7x7
    | Edges
    | Sharpen
    | Emboss
    | ClockwiseRotation
    | CounterClockwiseRotation

let modificationParser modification =
    match modification with
    | Gauss5x5 -> applyFilter gaussianBlurKernel
    | Gauss7x7 -> applyFilter gaussianBlur7x7Kernel
    | Edges -> applyFilter edgesKernel
    | Sharpen -> applyFilter sharpenKernel
    | Emboss -> applyFilter embossKernel
    | ClockwiseRotation -> rotate90Degrees Right
    | CounterClockwiseRotation -> rotate90Degrees Left

type CliArguments =
    | [<Mandatory; AltCommandLine("-i")>] InputPath of inputPath: string
    | [<Mandatory; AltCommandLine("-o")>] OutputPath of outputPath: string
    | [<AltCommandLine("-mod")>] Modifications of modifications: List<Modifications>

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Modifications _ -> "Set of modifications to image or image array"
            | InputPath _ -> "Input directory or path to the image"
            | OutputPath _ -> "Output directory or path to saved image"
