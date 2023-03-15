module ImageArrayProcessing

open CpuImageProcessing
open Agents

let extensions =
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
        Array.filter (fun (x: string) -> Array.contains (System.IO.Path.GetExtension x) extensions) files

    List.ofArray filtered

let arrayOfImagesProcessing inputDir outputDir conversion switcher =
    let list = listAllFiles inputDir
    if switcher then
        let agentSaver = imgSaver outputDir
        let procAgent = imgProcessor conversion agentSaver
        for file in list do
            procAgent.Post(Img (loadAsImage file))
        procAgent.PostAndReply EOS
    else
        let helper filePath =
            let filtered = conversion (loadAsImage filePath)
            saveImage filtered (System.IO.Path.Combine(outputDir, System.IO.Path.GetFileName filePath))

        List.iter helper list
