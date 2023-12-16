namespace CpuTests

open Expecto
open ImageProcessing.Arguments
open ImageProcessing.Kernels
open ImageProcessing.MyImage
open ImageProcessing.Types
open System

module SimpleTests =
    let src = __SOURCE_DIRECTORY__

    [<Tests>]
    let tests =
        testList
            "Some simple tests with CPU"
            [ testCase "MyImage after gauss filter with CPU"
              <| fun _ ->
                  let image = loadAsImage (src + "/input/test.png")
                  let filtered = ImageProcessing.CpuProcessing.applyFilter gaussianBlur7x7Kernel image

                  Expect.notEqual
                      image.Data
                      filtered.Data
                      "Image after filter apply shouldn't be the same with source image"
              testCase "Correctness of rotation MyImage with CPU"
              <| fun _ ->
                  let image = MyImage([| 1uy; 2uy; 3uy; 1uy; 2uy; 3uy; 1uy; 2uy; 3uy |], 3, 3, "test")

                  let turnedImage = image |> ImageProcessing.CpuProcessing.rotate Right

                  let expected = [| 1uy; 1uy; 1uy; 2uy; 2uy; 2uy; 3uy; 3uy; 3uy |]

                  Expect.equal turnedImage.Data expected "rotate function is not correct"
              testCase "MyImage after 4 turns in one way should be the same with CPU"
              <| fun _ ->
                  let image = loadAsImage (src + "/input/test4.jpg")

                  let turnedImage =
                      image
                      |> ImageProcessing.CpuProcessing.rotate Right
                      |> ImageProcessing.CpuProcessing.rotate Right
                      |> ImageProcessing.CpuProcessing.rotate Right
                      |> ImageProcessing.CpuProcessing.rotate Right

                  Expect.equal
                      image.Data
                      turnedImage.Data
                      "Image after 4 turns in one way should be the same with source image" ]

module PropertyTests =

    [<Tests>]
    let tests =
        testList
            "Some property tests with CPU"
            [ testProperty "MyImage similarity with double left and right rotation with CPU"
              <| fun (arr: byte[,]) ->
                  let image =
                      MyImage(GpuTests.SimpleTests.flat2dArray arr, Array2D.length2 arr, Array2D.length1 arr, "test")

                  let turnedLeft =
                      image
                      |> ImageProcessing.CpuProcessing.rotate Left
                      |> ImageProcessing.CpuProcessing.rotate Left

                  let turnedRight =
                      image
                      |> ImageProcessing.CpuProcessing.rotate Right
                      |> ImageProcessing.CpuProcessing.rotate Right

                  Expect.equal
                      turnedLeft.Data
                      turnedRight.Data
                      "Image after 2 turns in one way should be the same with image turned 2 times in opposite way"

              testProperty "Modification of image with CPU"
              <| fun (length: int) (modification: Modifications) ->
                  let arr =
                      Array2D.init ((abs length) + 2) ((abs length) + 2) (fun _ _ -> Random().Next(1, 10) |> byte)

                  let image =
                      MyImage(GpuTests.SimpleTests.flat2dArray arr, Array2D.length2 arr, Array2D.length1 arr, "test")

                  let newImage = modificationParser modification image

                  Expect.notEqual
                      image.Data
                      newImage.Data
                      "The image after modification should not be the same with the original" ]
