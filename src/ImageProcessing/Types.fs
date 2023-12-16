/// <summary>
/// Module with necessary algebraic types
/// </summary>
module ImageProcessing.Types

open MyImage

/// <summary>
/// Type for determining the rotation side of the image
/// </summary>
type Side =
    | Right
    | Left

/// <summary>
/// Type for determining the direction of image reflection
/// </summary>
type MirrorDirection =
    | Vertical
    | Horizontal

/// <summary>
/// Type to define a message to be forwarded between agents
/// </summary>
type Msg =
    | Img of MyImage
    | Path of string
    | EOS of AsyncReplyChannel<unit>
    | Message of string

/// <summary>
/// Type for determining the status of an agent
/// </summary>
type AgentStatus =
    | On
    | Off

/// <summary>
/// Type for determining the applied image transformation
/// </summary>
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

/// <summary>
/// Type for defining the executor of transformations
/// </summary>
type Devices =
    | AnyGpu
    | Nvidia
    | Amd
    | Intel
