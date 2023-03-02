namespace CpuTests

open Expecto
open CpuImageProcessing
open Arguments
open System

module SimpleTests =
    let src = __SOURCE_DIRECTORY__

    [<Tests>]
    let tests =
        testList
            "Some simple tests"
            [ testCase "Image after gauss filter"
              <| fun _ ->
                  let image = loadAs2DArray (src + "/input/test.png")
                  let filtered = applyFilter gaussianBlur7x7Kernel image
                  Expect.notEqual image filtered "Image after filter apply shouldn't be the same with source image"
              testCase "Correctness of rotation"
              <| fun _ ->
                  let image =
                      array2D [ [| 1uy; 2uy; 3uy |]; [| 1uy; 2uy; 3uy |]; [| 1uy; 2uy; 3uy |] ]

                  let turnedImage = image |> rotate90Degrees true

                  let expected =
                      array2D [ [| 1uy; 1uy; 1uy |]; [| 2uy; 2uy; 2uy |]; [| 3uy; 3uy; 3uy |] ]

                  Expect.equal turnedImage expected "rotate function is not correct"
              testCase "Image after 4 turns in one way should be the same"
              <| fun _ ->
                  let image = loadAs2DArray (src + "/input/test4.jpg")

                  let turnedImage =
                      image
                      |> rotate90Degrees true
                      |> rotate90Degrees true
                      |> rotate90Degrees true
                      |> rotate90Degrees true

                  Expect.equal image turnedImage "Image after 4 turns in one way should be the same with source image" ]

module PropertyTests =
    [<Tests>]
    let tests =
        testList
            "Some property tests"
            [ testProperty "The image similarity with double left and right rotation"
              <| fun (arr: byte[,]) ->
                  let turnedLeft = arr |> rotate90Degrees false |> rotate90Degrees false
                  let turnedRight = arr |> rotate90Degrees true |> rotate90Degrees true

                  Expect.equal
                      turnedLeft
                      turnedRight
                      "Image after 2 turns in one way should be the same with image turned 2 times in opposite way"

              testProperty "Modification of image"
              <| fun (length: int) (modification: Modifications) ->
                  let arr =
                      Array2D.init ((abs length) + 2) ((abs length) + 2) (fun _ _ -> Random().Next(1, 10) |> byte)

                  let newImage = ModificationParser modification arr
                  Expect.notEqual arr newImage "The image after modification should not be the same with the original" ]
