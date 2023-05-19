module Arguments

open Argu
open Kernels
open Types
open Brahma.FSharp

let first (x, _, _, _, _, _) = x
let second (_, x, _, _, _, _) = x
let third (_, _, x, _, _, _) = x
let fourth (_, _, _, x, _, _) = x
let fifth (_, _, _, _, x, _) = x
let sixth (_, _, _, _, _, x) = x



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

let modificationGpuParser modification cortege =
    match modification with
    | Gauss5x5 -> GpuProcessing.applyFilter gaussianBlurKernel (first cortege)
    | Gauss7x7 -> GpuProcessing.applyFilter gaussianBlur7x7Kernel (first cortege)
    | Edges -> GpuProcessing.applyFilter edgesKernel (first cortege)
    | Sharpen -> GpuProcessing.applyFilter sharpenKernel (first cortege)
    | Emboss -> GpuProcessing.applyFilter embossKernel (first cortege)
    | ClockwiseRotation -> GpuProcessing.rotate (second cortege)
    | CounterClockwiseRotation -> GpuProcessing.rotate (third cortege)
    | MirrorVertical -> GpuProcessing.mirror (fourth cortege)
    | MirrorHorizontal -> GpuProcessing.mirror (fifth cortege)
    | FishEye -> GpuProcessing.fishEye (sixth cortege)

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
