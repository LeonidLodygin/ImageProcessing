module CpuImageProcessing

open MyImage
open Types

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

let rotate90DegreesImage (side: Side) (image: MyImage) =
    let res = Array.zeroCreate image.Data.Length

    for p in 0 .. image.Data.Length - 1 do
        if side = Right then
            res[(p % image.Width) * image.Height + image.Height - 1 - p / image.Width] <- image.Data[p]
        else
            res[image.Height * (image.Width - 1 - p % image.Width) + p / image.Width] <- image.Data[p]

    MyImage(res, image.Height, image.Width, image.Name)

let ImageMirrorCPU (side: MirrorDirection) (image: MyImage) =
    let res = Array.zeroCreate image.Data.Length

    for p in 0 .. image.Data.Length - 1 do
        if side = Vertical then
            res[p - p % image.Width + image.Width - 1 - p % image.Width] <- image.Data[p]
        else
            res[(image.Height - 1 - p / image.Width) * image.Width + p % image.Width] <- image.Data[p]

    MyImage(res, image.Width, image.Height, image.Name)
