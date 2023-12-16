/// <summary>
/// Module with implementation of agents for image processing
/// </summary>
module ImageProcessing.Agents

open Types
open MyImage

/// <summary>
/// List of all files in directory
/// </summary>
let listAllFiles dir =
    let files = System.IO.Directory.GetFiles dir
    List.ofArray files

/// <summary>
/// Creation of path to save the image
/// </summary>
let outFile (imgName: string) (outDir: string) = System.IO.Path.Combine(outDir, imgName)

/// <summary>
/// Agent for saving images
/// </summary>
/// <param name="outDir">Path to save</param>
/// <param name="logger">Logging Agent</param>
let imgSaver outDir (logger: MailboxProcessor<_>) =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | EOS ch ->
                    logger.Post(Message "Image saver is finished!")
                    ch.Reply()
                | Img img ->
                    logger.Post(Message $"Save: %A{img.Name}")
                    saveImage img (outFile img.Name outDir)
                | _ -> failwith "imgSaver received the wrong message"
        })

/// <summary>
/// Agent for image processing
/// </summary>
/// <param name="filter">Filter for application</param>
/// <param name="imgSaver">Saving Agent</param>
/// <param name="logger">Logging Agent</param>
let imgProcessor filter (imgSaver: MailboxProcessor<_>) (logger: MailboxProcessor<_>) =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | EOS ch ->
                    logger.Post(Message "Image processor is ready to finish!")
                    imgSaver.PostAndReply Msg.EOS
                    logger.Post(Message "Image processor is finished!")
                    ch.Reply()
                | Img img ->
                    logger.Post(Message $"Filter: %A{img.Name}")
                    let filtered = filter img
                    imgSaver.Post(Img filtered)
                | _ -> failwith "imgProcessor received the wrong message"
        })

/// <summary>
/// Agent for logging
/// </summary>
let msgLogger () =
    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | EOS ch ->
                    printfn "msgLogger is finished!"
                    ch.Reply()
                | Message s -> printfn $"%s{s}"
                | _ -> failwith "msgLogger received the wrong message"
        })

/// <summary>
/// Agent with the ability to process and save the image
/// </summary>
/// <param name="outputDir">Path to save</param>
/// <param name="conversion">Image transformation</param>
/// <param name="logger">Logging Agent</param>
let superAgent outputDir conversion (logger: MailboxProcessor<_>) =

    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()

                match msg with
                | EOS ch ->
                    logger.Post(Message "SuperAgent is finished!")
                    ch.Reply()
                | Path inputPath ->
                    let image = loadAsImage inputPath
                    logger.Post(Message $"Filter: %A{image.Name}")
                    let filtered = conversion image
                    saveImage filtered (outFile image.Name outputDir)
                    logger.Post(Message $"Save: %A{image.Name}")
                | _ -> failwith "superAgent received the wrong message"
        })

/// <summary>
/// Image processing using superAgents
/// </summary>
/// <param name="inputDir">Path to image or images</param>
/// <param name="outputDir">Path to save</param>
/// <param name="conversion">Image transformation</param>
/// <param name="countOfAgents">Count of superAgents to processing</param>
let superImageProcessing inputDir outputDir conversion countOfAgents =
    let filesToProcess = listAllFiles inputDir
    let logger = msgLogger ()

    let superAgents =
        Array.init countOfAgents (fun _ -> superAgent outputDir conversion logger)

    for file in filesToProcess do
        (superAgents |> Array.minBy (fun p -> p.CurrentQueueLength)).Post(Path file)

    for agent in superAgents do
        agent.PostAndReply EOS

    logger.PostAndReply EOS
