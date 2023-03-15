module Agents

open CpuImageProcessing

let listAllFiles dir =
    let files = System.IO.Directory.GetFiles dir
    List.ofArray files

type msg =
    | Img of MyImage
    | EOS of AsyncReplyChannel<unit>

let imgSaver outDir =
    let outFile (imgName: string) = System.IO.Path.Combine(outDir, imgName)

    MailboxProcessor.Start(fun inbox ->
        let rec loop () = async {
            let! msg = inbox.Receive()

            match msg with
            | EOS ch ->
                printfn "Image saver is finished!"
                ch.Reply()
            | Img img ->
                printfn $"Save: %A{img.Name}"
                saveImage img (outFile img.Name)
                return! loop ()
        }

        loop ()
    )

let imgProcessor filterApplicator (imgSaver: MailboxProcessor<_>) =

    let filter = filterApplicator

    MailboxProcessor.Start(fun inbox ->
        let rec loop () = async {
            let! msg = inbox.Receive()

            match msg with
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

        loop ()
    )
