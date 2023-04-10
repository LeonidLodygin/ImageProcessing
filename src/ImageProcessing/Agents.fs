module Agents

open Types
open MyImage

let listAllFiles dir =
    let files = System.IO.Directory.GetFiles dir
    List.ofArray files

let outFile (imgName: string) (outDir: string) = System.IO.Path.Combine(outDir, imgName)

let imgSaver outDir (logger: MailboxProcessor<_>) =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | Msg.EOS ch ->
                    logger.Post("Image saver is finished!")
                    ch.Reply()
                | Img img ->
                    logger.Post($"Save: %A{img.Name}")
                    saveImage img (outFile img.Name outDir)
        })

let imgProcessor filter (imgSaver: MailboxProcessor<_>) (logger: MailboxProcessor<_>) =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | Msg.EOS ch ->
                    logger.Post("Image processor is ready to finish!")
                    imgSaver.PostAndReply Msg.EOS
                    logger.Post("Image processor is finished!")
                    ch.Reply()
                | Img img ->
                    logger.Post($"Filter: %A{img.Name}")
                    let filtered = filter img
                    imgSaver.Post(Img filtered)
        })

let msgLogger() =
    MailboxProcessor<string>.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()
                printfn $"%s{msg}"
        })

let superAgent outputDir conversion (logger: MailboxProcessor<_>) =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | SuperMessage.EOS ch ->
                    logger.Post("SuperAgent is finished!")
                    ch.Reply()
                | Path inputPath ->
                    let image = loadAsImage inputPath
                    logger.Post($"Filter: %A{image.Name}")
                    let filtered = conversion image
                    saveImage filtered (outFile image.Name outputDir)
                    logger.Post($"Save: %A{image.Name}")
        })

let superImageProcessing inputDir outputDir conversion countOfAgents =
    let filesToProcess = listAllFiles inputDir
    let logger = msgLogger()
    let superAgents =
        Array.init countOfAgents (fun _ -> superAgent outputDir conversion logger)

    for file in filesToProcess do
        (superAgents |> Array.minBy (fun p -> p.CurrentQueueLength)).Post(Path file)

    for agent in superAgents do
        agent.PostAndReply SuperMessage.EOS
