module Agents

open Types
open MyImage

let listAllFiles dir =
    let files = System.IO.Directory.GetFiles dir
    List.ofArray files

let outFile (imgName: string) (outDir: string) = System.IO.Path.Combine(outDir, imgName)

let imgSaver outDir =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | Msg.EOS ch ->
                    printfn "Image saver is finished!"
                    ch.Reply()
                | Img img ->
                    printfn $"Save: %A{img.Name}"
                    saveImage img (outFile img.Name outDir)
        })

let imgProcessor filter (imgSaver: MailboxProcessor<_>) =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | Msg.EOS ch ->
                    printfn "Image processor is ready to finish!"
                    imgSaver.PostAndReply Msg.EOS
                    printfn "Image processor is finished!"
                    ch.Reply()
                | Img img ->
                    printfn $"Filter: %A{img.Name}"
                    let filtered = filter img
                    imgSaver.Post(Img filtered)
        })

let superAgent outputDir conversion =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | SuperMessage.EOS ch ->
                    printfn "SuperAgent is finished!"
                    ch.Reply()
                | Path inputPath ->
                    let image = loadAsImage inputPath
                    printfn $"Filter: %A{image.Name}"
                    let filtered = conversion image
                    saveImage filtered (outFile image.Name outputDir)
                    printfn $"Save: %A{image.Name}"
        })

let superImageProcessing inputDir outputDir conversion countOfAgents =
    let filesToProcess = listAllFiles inputDir

    let superAgents =
        Array.init countOfAgents (fun _ -> superAgent outputDir conversion)

    for file in filesToProcess do
        (superAgents |> Array.minBy (fun p -> p.CurrentQueueLength)).Post(Path file)

    for agent in superAgents do
        agent.PostAndReply SuperMessage.EOS
