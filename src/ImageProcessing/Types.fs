module Types

open MyImage

type Side =
    | Right
    | Left

type MirrorDirection =
    | Vertical
    | Horizontal

type Msg =
    | Img of MyImage
    | Path of string
    | EOS of AsyncReplyChannel<unit>
    | Message of string

type AgentStatus =
    | On
    | Off

type Modifications =
    | Gauss5x5
    | Gauss7x7
    | Edges
    | Sharpen
    | Emboss
    | ClockwiseRotation
    | CounterClockwiseRotation
    | MirrorVertical
    | MirrorHorizontal
    | FishEye

type Devices =
    | AnyGpu
    | Nvidia
    | Amd
    | Intel
