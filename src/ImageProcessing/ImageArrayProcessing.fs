module ImageArrayProcessing

open CpuImageProcessing

let Extensions =
    [| ".png"
       ".jpeg"
       ".jpg"
       ".gif"
       ".jfif"
       ".webp"
       ".pbm"
       ".bmp"
       ".tga"
       ".tiff" |]

let listAllFiles dir =
    let files = System.IO.Directory.GetFiles dir
    printfn $"%A{files}"
    // Filter the obtained files by extension
    let filtered =
        Array.filter (fun (x: string) -> Array.contains (x.Substring(x.LastIndexOf('.'))) Extensions) files

    printfn $"%A{filtered}"
    List.ofArray filtered

let ArrayOfImagesProcessing inputDir outputDir conversion =
    let list = listAllFiles inputDir

    let rec helper lst =
        match lst with
        | [] -> ()
        | hd :: tl ->
            let filtered = conversion (loadAs2DArray hd)
            save2DByteArrayAsImage filtered (System.IO.Path.Combine(outputDir, System.IO.Path.GetFileName hd))
            helper tl

    helper list
