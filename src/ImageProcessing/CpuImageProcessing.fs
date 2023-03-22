module CpuImageProcessing

open System
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

type Side =
    | Right
    | Left

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

    printfn $"%A{System.IO.Path.GetFileName filePath} successfully loaded."
    res

let loadAsImage (file: string) =
    let img = Image.Load<L8> file

    let buf = Array.zeroCreate<byte> (img.Width * img.Height)

    img.CopyPixelDataTo(Span<byte> buf)
    MyImage(buf, img.Width, img.Height, System.IO.Path.GetFileName file)

let flat2dArray arr =
    seq {
        for x in 0 .. (Array2D.length1 arr) - 1  do
            for y in 0 .. (Array2D.length2 arr) - 1 do
                yield arr[x, y]
    }
    |> Array.ofSeq

let save2DByteArrayAsImage (imageData: byte[,]) filePath =
    let height = Array2D.length1 imageData
    let width = Array2D.length2 imageData
    let img = Image.LoadPixelData<L8>(flat2dArray imageData, width, height)
    img.Save filePath
    printfn $"%A{System.IO.Path.GetFileName filePath} successfully saved."

let saveImage (image: MyImage) file =
    let img = Image.LoadPixelData<L8>(image.Data, image.Width, image.Height)
    img.Save file

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

let gaussianBlur7x7Kernel =
    [| [| 0; 0; 1; 2; 1; 0; 0 |]
       [| 0; 3; 13; 22; 13; 3; 0 |]
       [| 1; 13; 59; 97; 59; 13; 1 |]
       [| 2; 22; 97; 159; 97; 22; 2 |]
       [| 1; 13; 59; 97; 59; 13; 1 |]
       [| 0; 3; 13; 22; 13; 3; 0 |]
       [| 0; 0; 1; 2; 1; 0; 0 |] |]
    |> Array.map (Array.map (fun x -> (float32 x) / 1003.0f))

let sharpenKernel =
    [| [| -1; -1; -1; -1; -1 |]
       [| -1; 2; 2; 2; -1 |]
       [| -1; 2; 8; 2; -1 |]
       [| -1; 2; 2; 2; -1 |]
       [| -1; -1; -1; -1; -1 |] |]
    |> Array.map (Array.map (fun x -> (float32 x) / 8.0f))

let embossKernel =
    [| [| -1f; -1f; -1f; -1f; 0f |]
       [| -1f; -1f; -1f; 0f; 1f |]
       [| -1f; -1f; 0f; 1f; 1f |]
       [| -1f; 0f; 1f; 1f; 1f |]
       [| 0f; 1f; 1f; 1f; 1f |] |]

let applyFilter (filter: float32[][]) (img: byte[,]) =
    let imgHeight = Array2D.length1 img
    let imgWidth = Array2D.length2 img

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

let applyFilterToImage (filter: float32[][]) (img: MyImage) =
    let filterD = (Array.length filter) / 2
    let filter = Array.concat filter

    let processPixel p =
        let pw = p % img.Width
        let ph = p / img.Width

        let dataToHandle =
            [| for i in ph - filterD .. ph + filterD do
                   for j in pw - filterD .. pw + filterD do
                       if i < 0 || i >= img.Height || j < 0 || j >= img.Width then
                           float32 img.Data[p]
                       else
                           float32 img.Data[i * img.Width + j] |]

        Array.fold2 (fun s x y -> s + x * y) 0.0f filter dataToHandle

    MyImage(Array.mapi (fun p _ -> byte (processPixel p)) img.Data, img.Width, img.Height, img.Name)

let rotate90Degrees (side: Side) (image: byte[,]) =
    let height = Array2D.length1 image
    let width = Array2D.length2 image
    let res = Array2D.zeroCreate width height

    for i in 0 .. width - 1 do
        for j in 0 .. height - 1 do
            if side = Right then
                res[i, height - 1 - j] <- image[j, i]
            else
                res[width - 1 - i, j] <- image[j, i]

    res

let rotate90DegreesImage (side: Side) (image: MyImage) =
    let res = Array.zeroCreate image.Data.Length

    for p in 0 .. image.Data.Length - 1 do
        if side = Right then
            res[(p % image.Width) * image.Height + image.Height - 1 - p / image.Width] <- image.Data[p]
        else
            res[image.Height * (image.Width - 1 - p % image.Width) + p / image.Width] <- image.Data[p]

    MyImage(res, image.Height, image.Width, image.Name)
