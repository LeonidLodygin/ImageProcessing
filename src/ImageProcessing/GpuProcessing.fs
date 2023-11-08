/// <summary>
/// Module with functions for image processing on the GPU
/// </summary>
module GpuProcessing

open Brahma.FSharp
open MyImage
open GpuKernels

/// <summary>
/// Filter application
/// </summary>
/// <param name="filter">A two-dimensional array applied to an image as a filter</param>
/// <param name="kernel">Compiled kernel for filter application</param>
/// <param name="clContext">Abstraction over OpenCL context</param>
/// <param name="localWorkSize">Local workgroup size</param>
/// <param name="queue">Command queue capable of handling messages of type Msg</param>
/// <param name="image">Image with type MyImage</param>
/// <returns>Image with type MyImage</returns>
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

/// <summary>
/// Rotate of image
/// </summary>
/// <param name="side">The side to which the image will be rotated</param>
/// <param name="kernel">Compiled kernel for rotation application</param>
/// <param name="clContext">Abstraction over OpenCL context</param>
/// <param name="localWorkSize">Local workgroup size</param>
/// <param name="queue">Command queue capable of handling messages of type Msg</param>
/// <param name="image">Image with type MyImage</param>
/// <returns>Image with type MyImage</returns>
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

/// <summary>
/// Reflection of image
/// </summary>
/// <param name="side">The side to which the image will be reflected</param>
/// <param name="kernel">Compiled kernel for reflection application</param>
/// <param name="clContext">Abstraction over OpenCL context</param>
/// <param name="localWorkSize">Local workgroup size</param>
/// <param name="queue">Command queue capable of handling messages of type Msg</param>
/// <param name="image">Image with type MyImage</param>
/// <returns>Image with type MyImage</returns>
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

/// <summary>
/// Applying fisheye filter to the image
/// </summary>
/// <param name="kernel">Compiled kernel for fisheye filter application</param>
/// <param name="clContext">Abstraction over OpenCL context</param>
/// <param name="localWorkSize">Local workgroup size</param>
/// <param name="queue">Command queue capable of handling messages of type Msg</param>
/// <param name="image">Image with type MyImage</param>
/// <returns>Image with type MyImage</returns>
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
