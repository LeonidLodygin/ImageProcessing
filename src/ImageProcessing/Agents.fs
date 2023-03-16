module Agents

open CpuImageProcessing

let listAllFiles dir =
    let files = System.IO.Directory.GetFiles dir
    List.ofArray files

let outFile (imgName: string) (outDir: string) = System.IO.Path.Combine(outDir, imgName)

type Msg =
    | Img of MyImage
    | Path of string
    | EOS of AsyncReplyChannel<unit>

type AgentStatus =
    | On
    | Off

let imgSaver outDir =

    MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()

                match msg with
                | Path _ -> failwith $"This agent is not able to read the image"
                | EOS ch ->
                    printfn "Image saver is finished!"
                    ch.Reply()
                | Img img ->
                    printfn $"Save: %A{img.Name}"
                    saveImage img (outFile img.Name outDir)
                    return! loop ()
            }

        loop ())

let imgProcessor filter (imgSaver: MailboxProcessor<_>) =

    MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()

                match msg with
                | Path _ -> failwith $"This agent is not able to read the image"
                | EOS ch ->
                    printfn "Image processor is ready to finish!"
                    imgSaver.PostAndReply EOS
                    printfn "Image processor is finished!"
                    ch.Reply()
                | Img img ->
                    printfn $"Filter: %A{img.Name}"
                    let filtered = filter img
                    imgSaver.Post(Img filtered)
                    return! loop ()
            }

        loop ())

let superAgent outputDir conversion =

    MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()

                match msg with
                | Img _ -> failwith $"This agent is not able to accept the image"
                | EOS ch ->
                    printfn "SuperAgent is finished!"
                    ch.Reply()
                | Path inputPath ->
                    let image = loadAsImage inputPath
                    printfn $"Filter: %A{image.Name}"
                    let filtered = conversion image
                    saveImage filtered (outFile image.Name outputDir)
                    printfn $"Save: %A{image.Name}"
                    return! loop ()
            }

        loop ())

let superImageProcessing inputDir outputDir conversion countAgent =
    let filesToProcess = listAllFiles inputDir
    let superAgents = Array.create countAgent (superAgent outputDir conversion)

    for file in filesToProcess do
        (superAgents |> Array.minBy (fun p -> p.CurrentQueueLength)).Post(Path file)

    for agent in superAgents do
        agent.PostAndReply EOS
