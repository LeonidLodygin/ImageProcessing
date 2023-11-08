/// <summary>
/// Module with kernels for image processing on the GPU
/// </summary>
module GpuKernels

open Types
open Brahma.FSharp

/// <summary>
/// Compilation of kernel to apply filter to the image
/// </summary>
let applyFilterKernel (clContext: ClContext) =

    let kernel =
        <@
            fun (r: Range1D) (img: ClArray<_>) imgW imgH (filter: ClArray<_>) filterD (result: ClArray<_>) ->
                let p = r.GlobalID0
                let pw = p % imgW
                let ph = p / imgW
                let mutable res = 0.0f

                for i in ph - filterD .. ph + filterD do
                    for j in pw - filterD .. pw + filterD do
                        let mutable d = 0uy

                        if i < 0 || i >= imgH || j < 0 || j >= imgW then
                            d <- img[p]
                        else
                            d <- img[i * imgW + j]

                        let f = filter[(i - ph + filterD) * (2 * filterD + 1) + (j - pw + filterD)]
                        res <- res + (float32 d) * f

                result[p] <- byte (int res)
        @>

    clContext.Compile kernel

/// <summary>
/// Asynchronous application of the filter kernel to the image
/// </summary>
let applyFilterProcessor
    (kernel: ClProgram<Range1D, ClArray<byte> -> int -> int -> ClArray<float32> -> int -> ClArray<byte> -> unit>)
    localWorkSize
    =

    fun (commandQueue: MailboxProcessor<_>) (filter: ClArray<float32>) filterD (img: ClArray<byte>) imgH imgW (result: ClArray<_>) ->
        let ndRange = Range1D.CreateValid(imgH * imgW, localWorkSize)
        let kernel = kernel.GetKernel()
        commandQueue.Post(Msg.MsgSetArguments(fun () -> kernel.KernelFunc ndRange img imgW imgH filter filterD result))
        commandQueue.Post(Msg.CreateRunMsg<_, _> kernel)
        result

/// <summary>
/// Compilation of kernel to rotate the image
/// </summary>
let rotateKernel (clContext: ClContext) =

    let kernel =
        <@
            fun (r: Range1D) (img: ClArray<_>) imgW imgH (i: int) (result: ClArray<_>) ->
                let p = r.GlobalID0

                if p / imgW < imgH then
                    if i = 1 then
                        result[(p % imgW) * imgH + imgH - 1 - p / imgW] <- img[p]
                    else
                        result[imgH * (imgW - 1 - p % imgW) + p / imgW] <- img[p]
        @>


    clContext.Compile kernel

/// <summary>
/// Asynchronous application of the rotation kernel to the image
/// </summary>
let rotateKernelProcessor
    (kernel: ClProgram<Range1D, ClArray<byte> -> int -> int -> int -> ClArray<byte> -> unit>)
    localWorkSize
    side
    =

    fun (commandQueue: MailboxProcessor<_>) (img: ClArray<byte>) imgH imgW (result: ClArray<_>) ->
        let ndRange = Range1D.CreateValid(imgH * imgW, localWorkSize)
        let kernel = kernel.GetKernel()
        let i = if side = Right then 1 else 0
        commandQueue.Post(Msg.MsgSetArguments(fun () -> kernel.KernelFunc ndRange img imgW imgH i result))
        commandQueue.Post(Msg.CreateRunMsg<_, _> kernel)
        result

/// <summary>
/// Compilation of kernel to reflect the image
/// </summary>
let mirrorKernel (clContext: ClContext) =

    let kernel =
        <@
            fun (r: Range1D) (img: ClArray<_>) imgW imgH i (result: ClArray<_>) ->
                let p = r.GlobalID0

                if p / imgW < imgH then
                    if i = 1 then
                        result[p - p % imgW + imgW - 1 - p % imgW] <- img[p]
                    else
                        result[(imgH - 1 - p / imgW) * imgW + p % imgW] <- img[p]
        @>


    clContext.Compile kernel

/// <summary>
/// Asynchronous application of the reflection kernel to the image
/// </summary>
let mirrorKernelProcessor
    (kernel: ClProgram<Range1D, ClArray<byte> -> int -> int -> int -> ClArray<byte> -> unit>)
    localWorkSize
    side
    =

    fun (commandQueue: MailboxProcessor<_>) (img: ClArray<byte>) imgH imgW (result: ClArray<_>) ->
        let ndRange = Range1D.CreateValid(imgH * imgW, localWorkSize)
        let kernel = kernel.GetKernel()
        let i = if side = Vertical then 1 else 0
        commandQueue.Post(Msg.MsgSetArguments(fun () -> kernel.KernelFunc ndRange img imgW imgH i result))
        commandQueue.Post(Msg.CreateRunMsg<_, _> kernel)
        result

/// <summary>
/// Compilation of kernel to apply FishEye to the image
/// </summary>
let fishEyeKernel (clContext: ClContext) =

    let kernel =
        <@
            fun (r: Range1D) (img: ClArray<_>) imgW imgH (result: ClArray<_>) ->
                let distortion = 0.5f
                let p = r.GlobalID0

                if p / imgW < imgH then
                    let h = float32 imgH
                    let w = float32 imgW
                    let xnd = (2.0f * float32 (p / imgW) - h) / h
                    let ynd = (2.0f * float32 (p % imgW) - w) / w
                    let radius = xnd * xnd + ynd * ynd

                    let xdu, ydu =
                        if 1.0f - distortion * radius = 0.0f then
                            xnd, ynd
                        else
                            xnd / (1.0f - distortion * radius), ynd / (1.0f - distortion * radius)

                    let xu = int ((xdu + 1.0f) * h) / 2
                    let yu = int ((ydu + 1.0f) * w) / 2

                    if 0 <= xu && xu < int h && 0 <= yu && yu < int w then
                        result[p] <- img[xu * imgW + yu]
        @>


    clContext.Compile kernel

/// <summary>
/// Asynchronous application of the fisheye kernel to the image
/// </summary>
let fishEyeKernelProcessor
    (kernel: ClProgram<Range1D, ClArray<byte> -> int -> int -> ClArray<byte> -> unit>)
    localWorkSize
    =

    fun (commandQueue: MailboxProcessor<_>) (img: ClArray<byte>) imgH imgW (result: ClArray<_>) ->
        let ndRange = Range1D.CreateValid(imgH * imgW, localWorkSize)
        let kernel = kernel.GetKernel()
        commandQueue.Post(Msg.MsgSetArguments(fun () -> kernel.KernelFunc ndRange img imgW imgH result))
        commandQueue.Post(Msg.CreateRunMsg<_, _> kernel)
        result
