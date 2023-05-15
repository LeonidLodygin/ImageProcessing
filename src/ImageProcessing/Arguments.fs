module Arguments

open Argu
open Kernels
open Types
open Brahma.FSharp

let modificationParser modification =
    match modification with
    | Gauss5x5 -> CpuProcessing.applyFilter gaussianBlurKernel
    | Gauss7x7 -> CpuProcessing.applyFilter gaussianBlur7x7Kernel
    | Edges -> CpuProcessing.applyFilter edgesKernel
    | Sharpen -> CpuProcessing.applyFilter sharpenKernel
    | Emboss -> CpuProcessing.applyFilter embossKernel
    | ClockwiseRotation -> CpuProcessing.rotate Right
    | CounterClockwiseRotation -> CpuProcessing.rotate Left
    | MirrorVertical -> CpuProcessing.mirror Vertical
    | MirrorHorizontal -> CpuProcessing.mirror Horizontal
    | FishEye -> CpuProcessing.fishEye

let modificationGpuParser modification =
    match modification with
    | Gauss5x5 -> GpuProcessing.applyFilter gaussianBlurKernel
    | Gauss7x7 -> GpuProcessing.applyFilter gaussianBlur7x7Kernel
    | Edges -> GpuProcessing.applyFilter edgesKernel
    | Sharpen -> GpuProcessing.applyFilter sharpenKernel
    | Emboss -> GpuProcessing.applyFilter embossKernel
    | ClockwiseRotation -> GpuProcessing.rotate Right
    | CounterClockwiseRotation -> GpuProcessing.rotate Left
    | MirrorVertical -> GpuProcessing.mirror Vertical
    | MirrorHorizontal -> GpuProcessing.mirror Horizontal
    | FishEye -> GpuProcessing.fishEye

let deviceParser device =
    match device with
    | AnyGpu -> Platform.Any
    | Nvidia -> Platform.Nvidia
    | Amd -> Platform.Amd
    | Intel -> Platform.Intel

type CliArguments =
    | [<Mandatory; AltCommandLine("-i")>] InputPath of inputPath: string
    | [<Mandatory; AltCommandLine("-o")>] OutputPath of outputPath: string
    | [<AltCommandLine("-ag"); Last>] Agents
    | [<AltCommandLine("-sag"); Last>] SuperAgents of count: int
    | [<AltCommandLine("-mod")>] Modifications of modifications: List<Modifications>
    | [<AltCommandLine("-gpu")>] GpGpu of device: Devices

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Agents -> "Apply modifications to an image using agents"
            | SuperAgents _ -> "Apply modifications to an image using super agents"
            | Modifications _ -> "Set of modifications to image or image array"
            | InputPath _ -> "Input directory or path to the image"
            | OutputPath _ -> "Output directory or path to saved image"
            | GpGpu _ -> "Processing on Gpu"
