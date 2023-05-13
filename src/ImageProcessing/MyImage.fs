module MyImage

open System
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

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

let loadAsImage (file: string) =
    let img = Image.Load<L8> file

    let buf = Array.zeroCreate<byte> (img.Width * img.Height)

    img.CopyPixelDataTo(Span<byte> buf)
    MyImage(buf, img.Width, img.Height, System.IO.Path.GetFileName file)

let saveImage (image: MyImage) file =
    let img = Image.LoadPixelData<L8>(image.Data, image.Width, image.Height)
    img.Save file
