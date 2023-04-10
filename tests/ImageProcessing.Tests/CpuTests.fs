namespace CpuTests

open Expecto
open Arguments
open Kernels
open MyImage
open Types
open System

module SimpleTests =
    let src = __SOURCE_DIRECTORY__

    [<Tests>]
    let tests =
        testList
            "Some simple tests"
            [ testCase "MyImage after gauss filter"
              <| fun _ ->
                  let image = loadAsImage (src + "/input/test.png")
                  let filtered = CpuProcessing.applyFilter gaussianBlur7x7Kernel image

                  Expect.notEqual
                      image.Data
                      filtered.Data
                      "Image after filter apply shouldn't be the same with source image"
              testCase "Correctness of rotation MyImage"
              <| fun _ ->
                  let image = MyImage([| 1uy; 2uy; 3uy; 1uy; 2uy; 3uy; 1uy; 2uy; 3uy |], 3, 3, "test")

                  let turnedImage = image |> CpuProcessing.rotate Right

                  let expected = [| 1uy; 1uy; 1uy; 2uy; 2uy; 2uy; 3uy; 3uy; 3uy |]

                  Expect.equal turnedImage.Data expected "rotate function is not correct"
              testCase "MyImage after 4 turns in one way should be the same"
              <| fun _ ->
                  let image = loadAsImage (src + "/input/test4.jpg")

                  let turnedImage =
                      image
                      |> CpuProcessing.rotate Right
                      |> CpuProcessing.rotate Right
                      |> CpuProcessing.rotate Right
                      |> CpuProcessing.rotate Right

                  Expect.equal
                      image.Data
                      turnedImage.Data
                      "Image after 4 turns in one way should be the same with source image" ]

module PropertyTests =
    let flat2dArray arr =
        seq {
            for x in 0 .. (Array2D.length1 arr) - 1 do
                for y in 0 .. (Array2D.length2 arr) - 1 do
                    yield arr[x, y]
        }
        |> Array.ofSeq

    [<Tests>]
    let tests =
        testList
            "Some property tests"
            [ testProperty "MyImage similarity with double left and right rotation"
              <| fun (arr: byte[,]) ->
                  let image =
                      MyImage(flat2dArray arr, Array2D.length2 arr, Array2D.length1 arr, "test")

                  let turnedLeft = image |> CpuProcessing.rotate Left |> CpuProcessing.rotate Left
                  let turnedRight = image |> CpuProcessing.rotate Right |> CpuProcessing.rotate Right

                  Expect.equal
                      turnedLeft
                      turnedRight
                      "Image after 2 turns in one way should be the same with image turned 2 times in opposite way"

              testProperty "Modification of image"
              <| fun (length: int) (modification: Modifications) ->
                  let arr =
                      Array2D.init ((abs length) + 2) ((abs length) + 2) (fun _ _ -> Random().Next(1, 10) |> byte)

                  let image =
                      MyImage(flat2dArray arr, Array2D.length2 arr, Array2D.length1 arr, "test")

                  let newImage = modificationParser modification image

                  Expect.notEqual
                      image.Data
                      newImage.Data
                      "The image after modification should not be the same with the original" ]
