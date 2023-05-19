namespace GpuCpuComparison

open Expecto
open Arguments
open Types

module PropertyTests =
    [<Tests>]
    let tests =
        testList
            "Some property tests with GPU and CPU"
            [ testProperty "Modification of image with GPU and CPU"
              <| fun (length: int) (modification: Modifications) ->
                  match modification with
                  | Gauss5x5 -> skiptest |> ignore
                  | Gauss7x7 -> skiptest |> ignore
                  | Edges -> skiptest |> ignore
                  | Sharpen -> skiptest |> ignore
                  | Emboss -> skiptest |> ignore
                  | FishEye -> skiptest |> ignore
                  | _ ->
                      let image = GpuTests.SimpleTests.imageBuilder length

                      let newGPUImage =
                          modificationGpuParser
                              modification
                              GpuTests.SimpleTests.kernelsCortege
                              GpuTests.SimpleTests.context
                              64
                              GpuTests.SimpleTests.queue
                              image

                      let newCPUImage = modificationParser modification image

                      Expect.equal
                          newCPUImage.Data
                          newGPUImage.Data
                          $"The image after modification on GPU should be the same with image after modification on CPU. Image data is %A{image.Data}" ]
