/// <summary>
/// Module with implementation of processing array of images
/// </summary>
module ImageArrayProcessing

open MyImage
open Agents
open Types

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

/// <summary>
/// List of all files in directory with correct extensions
/// </summary>
let listAllFiles dir =
    let files = System.IO.Directory.GetFiles dir

    let filtered =
        Array.filter (fun (x: string) -> Array.contains (System.IO.Path.GetExtension x) extensions) files

    List.ofArray filtered

/// <summary>
/// Processing array of images
/// </summary>
/// <param name="inputDir">Path to the folder with images</param>
/// <param name="outputDir">Path to save</param>
/// <param name="conversion">Image transformation</param>
/// <param name="agentMod">Processing with or without agent assistance</param>
let arrayOfImagesProcessing inputDir outputDir conversion agentMod =
    let list = listAllFiles inputDir

    if agentMod = On then
        let logger = msgLogger ()
        let agentSaver = imgSaver outputDir logger
        let procAgent = imgProcessor conversion agentSaver logger

        for file in list do
            procAgent.Post(Img(loadAsImage file))

        procAgent.PostAndReply EOS
    else
        let helper filePath =
            let filtered = conversion (loadAsImage filePath)
            saveImage filtered (System.IO.Path.Combine(outputDir, System.IO.Path.GetFileName filePath))

        List.iter helper list
