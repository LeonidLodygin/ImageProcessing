﻿module GpuKernels

open Types
open Brahma.FSharp

let applyFilterKernel (clContext: ClContext) localWorkSize =

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

    let kernel = clContext.Compile kernel

    fun (commandQueue: MailboxProcessor<_>) (filter: ClArray<float32>) filterD (img: ClArray<byte>) imgH imgW (result: ClArray<_>) ->
        let ndRange = Range1D.CreateValid(imgH * imgW, localWorkSize)
        let kernel = kernel.GetKernel()
        commandQueue.Post(Msg.MsgSetArguments(fun () -> kernel.KernelFunc ndRange img imgW imgH filter filterD result))
        commandQueue.Post(Msg.CreateRunMsg<_, _> kernel)
        result

let rotateKernel (clContext: ClContext) localWorkSize side =

    let kernel =
        match side with
        | Right ->
            <@
                fun (r: Range1D) (img: ClArray<_>) imgW imgH (result: ClArray<_>) ->
                    let p = r.GlobalID0

                    if p / imgW < imgH then
                        result[(p % imgW) * imgH + imgH - 1 - p / imgW] <- img[p]
            @>
        | Left ->
            <@
                fun (r: Range1D) (img: ClArray<_>) imgW imgH (result: ClArray<_>) ->
                    let p = r.GlobalID0

                    if p / imgW < imgH then
                        result[imgH * (imgW - 1 - p % imgW) + p / imgW] <- img[p]
            @>


    let kernel = clContext.Compile kernel

    fun (commandQueue: MailboxProcessor<_>) (img: ClArray<byte>) imgH imgW (result: ClArray<_>) ->
        let ndRange = Range1D.CreateValid(imgH * imgW, localWorkSize)
        let kernel = kernel.GetKernel()
        commandQueue.Post(Msg.MsgSetArguments(fun () -> kernel.KernelFunc ndRange img imgW imgH result))
        commandQueue.Post(Msg.CreateRunMsg<_, _> kernel)
        result

let mirrorKernel (clContext: ClContext) localWorkSize side =

    let kernel =
        match side with
        | Vertical ->
            <@
                fun (r: Range1D) (img: ClArray<_>) imgW imgH (result: ClArray<_>) ->
                    let p = r.GlobalID0

                    if p / imgW < imgH then
                        result[p - p % imgW + imgW - 1 - p % imgW] <- img[p]
            @>
        | Horizontal ->
            <@
                fun (r: Range1D) (img: ClArray<_>) imgW imgH (result: ClArray<_>) ->
                    let p = r.GlobalID0
                    result[(imgH - 1 - p / imgW) * imgW + p % imgW] <- img[p]
            @>


    let kernel = clContext.Compile kernel

    fun (commandQueue: MailboxProcessor<_>) (img: ClArray<byte>) imgH imgW (result: ClArray<_>) ->
        let ndRange = Range1D.CreateValid(imgH * imgW, localWorkSize)
        let kernel = kernel.GetKernel()
        commandQueue.Post(Msg.MsgSetArguments(fun () -> kernel.KernelFunc ndRange img imgW imgH result))
        commandQueue.Post(Msg.CreateRunMsg<_, _> kernel)
        result

let fishEyeKernel (clContext: ClContext) localWorkSize =

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


    let kernel = clContext.Compile kernel

    fun (commandQueue: MailboxProcessor<_>) (img: ClArray<byte>) imgH imgW (result: ClArray<_>) ->
        let ndRange = Range1D.CreateValid(imgH * imgW, localWorkSize)
        let kernel = kernel.GetKernel()
        commandQueue.Post(Msg.MsgSetArguments(fun () -> kernel.KernelFunc ndRange img imgW imgH result))
        commandQueue.Post(Msg.CreateRunMsg<_, _> kernel)
        result
