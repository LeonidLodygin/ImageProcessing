module CpuImageProcessing

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

let loadAs2DArray (filePath: string) =
    let img = Image.Load<L8> filePath
    let res = Array2D.zeroCreate img.Height img.Width

    for i in 0 .. img.Width - 1 do
        for j in 0 .. img.Height - 1 do
            res[j, i] <- img.Item(i, j).PackedValue

    printfn $"H=%A{img.Height} W=%A{img.Width}"
    res

let loadAsMyImage (filePath: string) =
    let img = Image.Load<L8> filePath

    let buffer = Array.zeroCreate<byte> (img.Width * img.Height)

    img.CopyPixelDataTo(Span<byte> buffer)
    MyImage(buffer, img.Width, img.Height, System.IO.Path.GetFileName filePath)

let save2DByteArrayAsImage (imageData: byte[,]) filePath =
    let height = imageData.GetLength 0
    let width = imageData.GetLength 1
    printfn $"H=%A{height} W=%A{width}"

    let flat2dArray array2D =
        seq {
            for x in [ 0 .. (Array2D.length1 array2D) - 1 ] do
                for y in [ 0 .. (Array2D.length2 array2D) - 1 ] do
                    yield array2D[x, y]
        }
        |> Array.ofSeq

    let img = Image.LoadPixelData<L8>(flat2dArray imageData, width, height)
    img.Save filePath

let saveMyImage (image: MyImage) filePath =
    let img = Image.LoadPixelData<L8>(image.Data, image.Width, image.Height)
    img.Save filePath

let gaussianBlurKernel =
    [| [| 1; 4; 6; 4; 1 |]
       [| 4; 16; 24; 16; 4 |]
       [| 6; 24; 36; 24; 6 |]
       [| 4; 16; 24; 16; 4 |]
       [| 1; 4; 6; 4; 1 |] |]
    |> Array.map (Array.map (fun x -> (float32 x) / 100.0f))

let edgesKernel =
    [| [| 0; 0; -1; 0; 0 |]
       [| 0; 0; -1; 0; 0 |]
       [| 0; 0; 2; 0; 0 |]
       [| 0; 0; 0; 0; 0 |]
       [| 0; 0; 0; 0; 0 |] |]
    |> Array.map (Array.map float32)

let gaussianBlur7x7 =
    [| [| 0; 0; 1; 2; 1; 0; 0 |]
       [| 0; 3; 13; 22; 13; 3; 0 |]
       [| 1; 13; 59; 97; 59; 13; 1 |]
       [| 2; 22; 97; 159; 97; 22; 2 |]
       [| 1; 13; 59; 97; 59; 13; 1 |]
       [| 0; 3; 13; 22; 13; 3; 0 |]
       [| 0; 0; 1; 2; 1; 0; 0 |] |]
    |> Array.map (Array.map (fun x -> (float32 x) / 1003.0f))

let sharpen =
    [| [| -1; -1; -1; -1; -1 |]
       [| -1; 2; 2; 2; -1 |]
       [| -1; 2; 8; 2; -1 |]
       [| -1; 2; 2; 2; -1 |]
       [| -1; -1; -1; -1; -1 |] |]
    |> Array.map (Array.map (fun x -> (float32 x) / 8.0f))

let emboss =
    [| [| -1f; -1f; -1f; -1f; 0f |]
       [| -1f; -1f; -1f; 0f; 1f |]
       [| -1f; -1f; 0f; 1f; 1f |]
       [| -1f; 0f; 1f; 1f; 1f |]
       [| 0f; 1f; 1f; 1f; 1f |] |]

let applyFilter (filter: float32[][]) (img: byte[,]) =
    let imgHeight = img.GetLength 0
    let imgWidth = img.GetLength 1

    let filterD = (Array.length filter) / 2

    let filter = Array.concat filter

    let processPixel px py =
        let dataToHandle =
            [| for i in px - filterD .. px + filterD do
                   for j in py - filterD .. py + filterD do
                       if i < 0 || i >= imgHeight || j < 0 || j >= imgWidth then
                           float32 img[px, py]
                       else
                           float32 img[i, j] |]

        Array.fold2 (fun s x y -> s + x * y) 0.0f filter dataToHandle

    Array2D.mapi (fun x y _ -> byte (processPixel x y)) img

/// Rotating the picture 90 degrees. The "side" variable takes two values of the String type: right and left.
let rotate90Degrees (side: string) (image: byte[,]) =
    let height = image.GetLength 0
    let width = image.GetLength 1
    let res = Array2D.zeroCreate width height

    if side = "right" then
        for i in 0 .. width - 1 do
            for j in 0 .. height - 1 do
                res[i, height - 1 - j] <- image[j, i]
    elif side = "left" then
        for i in 0 .. width - 1 do
            for j in 0 .. height - 1 do
                res[width - 1 - i, j] <- image[j, i]
    else
        failwith $"Wrong value of 'side' variable. Look at function description for more info."

    res
