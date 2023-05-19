module GpuProcessing

open Brahma.FSharp
open MyImage
open GpuKernels

let applyFilter (filter: float32[][]) kernel (clContext: ClContext) localWorkSize (queue: MailboxProcessor<Msg>) =
    let kernel = applyFilterProcessor kernel localWorkSize

    fun (img: MyImage) ->

        let mutable input =
            clContext.CreateClArray<_>(img.Data, HostAccessMode.NotAccessible)

        let mutable output =
            clContext.CreateClArray(
                img.Data.Length,
                HostAccessMode.NotAccessible,
                allocationMode = AllocationMode.Default
            )

        let filterD = (Array.length filter) / 2
        let filter = Array.concat filter

        let clFilter =
            clContext.CreateClArray<_>(filter, HostAccessMode.NotAccessible, DeviceAccessMode.ReadOnly)

        let result = Array.zeroCreate (img.Height * img.Width)

        let result =
            queue.PostAndReply(fun ch ->
                Msg.CreateToHostMsg(kernel queue clFilter filterD input img.Height img.Width output, result, ch))

        queue.Post(Msg.CreateFreeMsg clFilter)
        queue.Post(Msg.CreateFreeMsg input)
        queue.Post(Msg.CreateFreeMsg output)
        MyImage(result, img.Width, img.Height, img.Name)

let rotate side kernel (clContext: ClContext) localWorkSize (queue: MailboxProcessor<Msg>) =
    let kernel = rotateKernelProcessor kernel localWorkSize

    fun (img: MyImage) ->

        let mutable input =
            clContext.CreateClArray<_>(img.Data, HostAccessMode.NotAccessible)

        let mutable output =
            clContext.CreateClArray(
                img.Data.Length,
                HostAccessMode.NotAccessible,
                allocationMode = AllocationMode.Default
            )

        let result = Array.zeroCreate img.Data.Length

        let result =
            queue.PostAndReply(fun ch ->
                Msg.CreateToHostMsg(kernel side queue input img.Height img.Width output, result, ch))

        queue.Post(Msg.CreateFreeMsg input)
        queue.Post(Msg.CreateFreeMsg output)
        MyImage(result, img.Height, img.Width, img.Name)

let mirror side kernel (clContext: ClContext) localWorkSize (queue: MailboxProcessor<Msg>) =
    let kernel = mirrorKernelProcessor kernel localWorkSize

    fun (img: MyImage) ->

        let mutable input =
            clContext.CreateClArray<_>(img.Data, HostAccessMode.NotAccessible)

        let mutable output =
            clContext.CreateClArray(
                img.Data.Length,
                HostAccessMode.NotAccessible,
                allocationMode = AllocationMode.Default
            )

        let result = Array.zeroCreate img.Data.Length

        let result =
            queue.PostAndReply(fun ch ->
                Msg.CreateToHostMsg(kernel side queue input img.Height img.Width output, result, ch))

        queue.Post(Msg.CreateFreeMsg input)
        queue.Post(Msg.CreateFreeMsg output)
        MyImage(result, img.Width, img.Height, img.Name)

let fishEye kernel (clContext: ClContext) localWorkSize (queue: MailboxProcessor<Msg>) =
    let kernel = fishEyeKernelProcessor kernel localWorkSize

    fun (img: MyImage) ->

        let mutable input =
            clContext.CreateClArray<_>(img.Data, HostAccessMode.NotAccessible)

        let mutable output =
            clContext.CreateClArray(
                img.Data.Length,
                HostAccessMode.NotAccessible,
                allocationMode = AllocationMode.Default
            )

        let result = Array.zeroCreate img.Data.Length

        let result =
            queue.PostAndReply(fun ch -> Msg.CreateToHostMsg(kernel queue input img.Height img.Width output, result, ch))

        queue.Post(Msg.CreateFreeMsg input)
        queue.Post(Msg.CreateFreeMsg output)
        MyImage(result, img.Width, img.Height, img.Name)
