module CpuImageProcessing

open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

let loadAs2DArray (filePath: string) =
    let img = Image.Load<L8> filePath
    let res = Array2D.zeroCreate img.Height img.Width

    for i in 0 .. img.Width - 1 do
        for j in 0 .. img.Height - 1 do
            res[j, i] <- img.Item(i, j).PackedValue

    printfn $"%A{System.IO.Path.GetFileName filePath} successfully loaded."
    res

let save2DByteArrayAsImage (imageData: byte[,]) filePath =
    let height = Array2D.length1 imageData
    let width = Array2D.length2 imageData

    let flat2dArray array2D =
        seq {
            for x in [ 0 .. (Array2D.length1 array2D) - 1 ] do
                for y in [ 0 .. (Array2D.length2 array2D) - 1 ] do
                    yield array2D[x, y]
        }
        |> Array.ofSeq

    let img = Image.LoadPixelData<L8>(flat2dArray imageData, width, height)
    img.Save filePath
    printfn $"%A{System.IO.Path.GetFileName filePath} successfully saved."

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

/// Rotating the picture 90 degrees. True - clockwise, false - counterclockwise.
let rotate90Degrees (side: bool) (image: byte[,]) =
    let height = Array2D.length1 image
    let width = Array2D.length2 image
    let res = Array2D.zeroCreate width height

    if side then
        for i in 0 .. width - 1 do
            for j in 0 .. height - 1 do
                res[i, height - 1 - j] <- image[j, i]
    else
        for i in 0 .. width - 1 do
            for j in 0 .. height - 1 do
                res[width - 1 - i, j] <- image[j, i]

    res
