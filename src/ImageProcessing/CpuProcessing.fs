module CpuProcessing

open MyImage
open Types

let applyFilter (filter: float32[][]) (img: MyImage) =
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

let rotate (side: Side) (image: MyImage) =
    let res = Array.zeroCreate image.Data.Length

    for p in 0 .. image.Data.Length - 1 do
        if side = Right then
            res[(p % image.Width) * image.Height + image.Height - 1 - p / image.Width] <- image.Data[p]
        else
            res[image.Height * (image.Width - 1 - p % image.Width) + p / image.Width] <- image.Data[p]

    MyImage(res, image.Height, image.Width, image.Name)

let mirror (side: MirrorDirection) (image: MyImage) =
    let res = Array.zeroCreate image.Data.Length

    for p in 0 .. image.Data.Length - 1 do
        if side = Vertical then
            res[p - p % image.Width + image.Width - 1 - p % image.Width] <- image.Data[p]
        else
            res[(image.Height - 1 - p / image.Width) * image.Width + p % image.Width] <- image.Data[p]

    MyImage(res, image.Width, image.Height, image.Name)

let fishEye (image: MyImage) =
    let distortion = 0.5
    let getFishCoordinates (x: float) (y: float) (r: float) =
        if 1.0 - distortion * r = 0 then
            x, y
        else
            x / (1.0 - distortion * r), y / (1.0 - distortion * r)
    let h = float image.Height
    let w = float image.Width
    let res = Array.zeroCreate image.Data.Length
    for p in 0 .. image.Data.Length - 1 do
        let xnd = (2.0*float (p / image.Width) - h)/h
        let ynd = (2.0*float (p % image.Width) - w)/w
        let radius = xnd*xnd + ynd*ynd
        let xdu, ydu = getFishCoordinates xnd ynd radius
        let xu = int((xdu + 1.0)*h)/2
        let yu = int((ydu + 1.0)*w)/2
        if 0 <= xu && xu < int h && 0 <= yu && yu < int w then
            res[p] <- image.Data[xu * image.Width + yu]
    MyImage(res, image.Width, image.Height, image.Name)
