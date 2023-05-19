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
                | Msg.SaverEOS ch ->
                    logger.PostAndReply SaverEOS
                    ch.Reply()
                | Img img ->
                    logger.Post(Message $"Save: %A{img.Name}")
                    saveImage img (outFile img.Name outDir)
                | _ -> failwith "imgSaver received the wrong message"
        })

let imgProcessor filter (imgSaver: MailboxProcessor<_>) (logger: MailboxProcessor<_>) =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | ProcessorEOS ch ->
                    logger.Post(Message "Image processor is ready to finish!")
                    imgSaver.PostAndReply SaverEOS
                    logger.PostAndReply ProcessorEOS
                    ch.Reply()
                | Img img ->
                    logger.Post(Message $"Filter: %A{img.Name}")
                    let filtered = filter img
                    imgSaver.Post(Img filtered)
                | _ -> failwith "imgProcessor received the wrong message"
        })

let msgLogger () =
    MailboxProcessor<Msg>.Start
        (fun inbox ->
            async {
                while true do
                    let! msg = inbox.Receive()

                    match msg with
                    | Message x -> printfn $"%s{x}"
                    | ProcessorEOS ch ->
                        printfn "Image processor is finished!"
                        ch.Reply()
                    | SaverEOS ch ->
                        printfn "Image saver is finished!"
                        ch.Reply()
                    | SuperEOS ch ->
                        printfn "SuperAgent is finished!"
                        ch.Reply()
                    | _ -> failwith "msgLogger received the wrong message"
            })

let superAgent outputDir conversion (logger: MailboxProcessor<_>) =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | SuperEOS ch ->
                    logger.PostAndReply SuperEOS
                    ch.Reply()
                | Path inputPath ->
                    let image = loadAsImage inputPath
                    logger.Post(Message $"Filter: %A{image.Name}")
                    let filtered = conversion image
                    saveImage filtered (outFile image.Name outputDir)
                    logger.Post(Message $"Save: %A{image.Name}")
                | _ -> failwith "superAgent received the wrong message"
        })

let superImageProcessing inputDir outputDir conversion countOfAgents =
    let filesToProcess = listAllFiles inputDir
    let logger = msgLogger ()

    let superAgents =
        Array.init countOfAgents (fun _ -> superAgent outputDir conversion logger)

    for file in filesToProcess do
        (superAgents |> Array.minBy (fun p -> p.CurrentQueueLength)).Post(Path file)

    for agent in superAgents do
        agent.PostAndReply SuperEOS
