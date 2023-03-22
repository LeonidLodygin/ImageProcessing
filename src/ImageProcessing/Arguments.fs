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
    | Gauss5x5 -> applyFilterToImage gaussianBlurKernel
    | Gauss7x7 -> applyFilterToImage gaussianBlur7x7Kernel
    | Edges -> applyFilterToImage edgesKernel
    | Sharpen -> applyFilterToImage sharpenKernel
    | Emboss -> applyFilterToImage embossKernel
    | ClockwiseRotation -> rotate90DegreesImage Right
    | CounterClockwiseRotation -> rotate90DegreesImage Left

type CliArguments =
    | [<Mandatory; AltCommandLine("-i")>] InputPath of inputPath: string
    | [<Mandatory; AltCommandLine("-o")>] OutputPath of outputPath: string
    | [<AltCommandLine("-ag"); Last>] Agents
    | [<AltCommandLine("-sag"); Last>] SuperAgents of count: int
    | [<AltCommandLine("-mod")>] Modifications of modifications: List<Modifications>

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Agents -> "Apply modifications to an image using agents"
            | SuperAgents _ -> "Apply modifications to an image using super agents"
            | Modifications _ -> "Set of modifications to image or image array"
            | InputPath _ -> "Input directory or path to the image"
            | OutputPath _ -> "Output directory or path to saved image"
