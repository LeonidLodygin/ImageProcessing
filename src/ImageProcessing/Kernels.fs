﻿module Kernels

let gaussianBlurKernel =
    [| [| 1; 4; 6; 4; 1 |]
       [| 4; 16; 24; 16; 4 |]
       [| 6; 24; 36; 24; 6 |]
       [| 4; 16; 24; 16; 4 |]
       [| 1; 4; 6; 4; 1 |] |]
    |> Array.map (Array.map (fun x -> (float32 x) / 100.0f))

let edgesKernel =
    [| [| 0; 0; -1; 0; 0 |]
       [| 0; 0; -1; 0; 0 |]
       [| 0; 0; 2; 0; 0 |]
       [| 0; 0; 0; 0; 0 |]
       [| 0; 0; 0; 0; 0 |] |]
    |> Array.map (Array.map float32)

let gaussianBlur7x7Kernel =
    [| [| 0; 0; 1; 2; 1; 0; 0 |]
       [| 0; 3; 13; 22; 13; 3; 0 |]
       [| 1; 13; 59; 97; 59; 13; 1 |]
       [| 2; 22; 97; 159; 97; 22; 2 |]
       [| 1; 13; 59; 97; 59; 13; 1 |]
       [| 0; 3; 13; 22; 13; 3; 0 |]
       [| 0; 0; 1; 2; 1; 0; 0 |] |]
    |> Array.map (Array.map (fun x -> (float32 x) / 1003.0f))

let sharpenKernel =
    [| [| -1; -1; -1; -1; -1 |]
       [| -1; 2; 2; 2; -1 |]
       [| -1; 2; 8; 2; -1 |]
       [| -1; 2; 2; 2; -1 |]
       [| -1; -1; -1; -1; -1 |] |]
    |> Array.map (Array.map (fun x -> (float32 x) / 8.0f))

let embossKernel =
    [| [| -1f; -1f; -1f; -1f; 0f |]
       [| -1f; -1f; -1f; 0f; 1f |]
       [| -1f; -1f; 0f; 1f; 1f |]
       [| -1f; 0f; 1f; 1f; 1f |]
       [| 0f; 1f; 1f; 1f; 1f |] |]