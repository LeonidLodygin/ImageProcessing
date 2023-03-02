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

    let filtered =
        Array.filter (fun (x: string) -> Array.contains (System.IO.Path.GetExtension x) Extensions) files

    List.ofArray filtered

let ArrayOfImagesProcessing inputDir outputDir conversion =
    let list = listAllFiles inputDir

    let helper filePath =
        let filtered = conversion (loadAs2DArray filePath)
        save2DByteArrayAsImage filtered (System.IO.Path.Combine(outputDir, System.IO.Path.GetFileName filePath))

    List.iter helper list
