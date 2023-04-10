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
    | EOS of AsyncReplyChannel<unit>

type SuperMessage =
    | Path of string
    | EOS of AsyncReplyChannel<unit>

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

type Devices =
    | AnyGpu
    | Nvidia
    | Amd
    | Intel
