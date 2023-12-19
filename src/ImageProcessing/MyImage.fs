/// <summary>
/// Module for working with images
/// </summary>
module ImageProcessing.MyImage

open System
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

/// <summary>
/// Type to represent images
/// </summary>
[<Struct>]
type MyImage =
    val Data: array<byte>
    val Width: int
    val Height: int
    val Name: string

    new(data, width, height, name) =
        { Data = data
          Width = width
          Height = height
          Name = name }

/// <summary>
/// Load image as MyImage type
/// </summary>
let loadAsImage (file: string) =
    let img = Image.Load<L8> file

    let buf = Array.zeroCreate<byte> (img.Width * img.Height)

    img.CopyPixelDataTo(Span<byte> buf)
    MyImage(buf, img.Width, img.Height, System.IO.Path.GetFileName file)

/// <summary>
/// Save MyImage in a specific directory
/// </summary>
let saveImage (image: MyImage) file =
    let img = Image.LoadPixelData<L8>(image.Data, image.Width, image.Height)
    img.Save file
