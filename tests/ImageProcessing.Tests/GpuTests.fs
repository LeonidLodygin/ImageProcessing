namespace GpuTests

open Expecto
open Arguments
open Kernels
open MyImage
open Types
open System
open Brahma.FSharp

module SimpleTests =
    let src = __SOURCE_DIRECTORY__
    let context = ClContext(ClDevice.GetFirstAppropriateDevice())
    let queue = context.QueueProvider.CreateQueue()

    let flat2dArray arr =
        seq {
            for x in 0 .. (Array2D.length1 arr) - 1 do
                for y in 0 .. (Array2D.length2 arr) - 1 do
                    yield arr[x, y]
        }
        |> Array.ofSeq

    let imageBuilder length =
        let arr =
            Array2D.init ((abs length) + 2) ((abs length) + 2) (fun _ _ -> Random().Next(1, 10) |> byte)

        MyImage(flat2dArray arr, Array2D.length2 arr, Array2D.length1 arr, "test")

    [<Tests>]
    let tests =
        testList
            "Some simple tests with GPU"
            [ testCase "MyImage after gauss filter with GPU"
              <| fun _ ->
                  let image = loadAsImage (src + "/input/test.png")

                  let filtered =
                      GpuProcessing.applyFilter gaussianBlur7x7Kernel context 64 queue image

                  Expect.notEqual
                      image.Data
                      filtered.Data
                      "Image after filter apply shouldn't be the same with source image"
              testCase "Correctness of rotation MyImage with GPU"
              <| fun _ ->
                  let image = MyImage([| 1uy; 2uy; 3uy; 1uy; 2uy; 3uy; 1uy; 2uy; 3uy |], 3, 3, "test")

                  let turnedImage = image |> GpuProcessing.rotate Right context 64 queue

                  let expected = [| 1uy; 1uy; 1uy; 2uy; 2uy; 2uy; 3uy; 3uy; 3uy |]

                  Expect.equal turnedImage.Data expected "rotate function is not correct"
              testCase "MyImage after 4 turns in one way should be the same with GPU"
              <| fun _ ->
                  let image = loadAsImage (src + "/input/test4.jpg")

                  let turnedImage =
                      image
                      |> GpuProcessing.rotate Right context 64 queue
                      |> GpuProcessing.rotate Right context 64 queue
                      |> GpuProcessing.rotate Right context 64 queue
                      |> GpuProcessing.rotate Right context 64 queue

                  Expect.equal
                      image.Data
                      turnedImage.Data
                      "Image after 4 turns in one way should be the same with source image" ]

module PropertyTests =

    [<Tests>]
    let tests =
        testList
            "Some property tests with GPU"
            [ testProperty "MyImage similarity with double left and right rotation with GPU"
              <| fun (length: int) ->
                  let image = SimpleTests.imageBuilder length

                  let turnedLeft =
                      image
                      |> GpuProcessing.rotate Left SimpleTests.context 64 SimpleTests.queue
                      |> GpuProcessing.rotate Left SimpleTests.context 64 SimpleTests.queue

                  let turnedRight =
                      image
                      |> GpuProcessing.rotate Right SimpleTests.context 64 SimpleTests.queue
                      |> GpuProcessing.rotate Right SimpleTests.context 64 SimpleTests.queue

                  Expect.equal
                      turnedLeft.Data
                      turnedRight.Data
                      "Image after 2 turns in one way should be the same with image turned 2 times in opposite way"

              testProperty "Modification of image with GPU"
              <| fun (length: int) (modification: Modifications) ->
                  match modification with
                  | MirrorHorizontal -> skiptest |> ignore
                  | MirrorVertical -> skiptest |> ignore
                  | ClockwiseRotation -> skiptest |> ignore
                  | CounterClockwiseRotation -> skiptest |> ignore
                  | _ ->
                      let image = SimpleTests.imageBuilder length

                      let newImage =
                          modificationGpuParser modification SimpleTests.context 64 SimpleTests.queue image

                      Expect.notEqual
                          image.Data
                          newImage.Data
                          "The image after modification should not be the same with the original" ]
