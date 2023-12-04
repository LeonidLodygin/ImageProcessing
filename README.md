# LeonidLodygin.ImageProcessing

Simple image processing on GPGPU in F# using [Brahma.FSharp](https://github.com/YaccConstructor/Brahma.FSharp).

## Features
* Apply built-in or custom filters to an image
* Rotating, reflecting the image
* Applying fish-eye to an image
* Image processing using CPU or specific GPU
* Ability to process images in multiple threads using agents (mailbox processor based).
* Process one image or a whole set of images at a time

## Installation
* TODO

## Quick start
```sh
> dotnet run -i *input path* -o *output path* -mod FishEye -gpu AnyGpu
```
## Result
| Original                                                                                       | Fisheye                                                                                        |
|:-----------------------------------------------------------------------------------------------|:-----------------------------------------------------------------------------------------------|
| ![image](https://raw.githubusercontent.com/LeonidLodygin/ImageProcessing/images/example.jpg)   | ![image](https://raw.githubusercontent.com/LeonidLodygin/ImageProcessing/images/processed.jpg) |

## Contributors

- Leonid Lodygin (Github: [@LeonidLodygin](https://github.com/LeonidLodygin))
- Semyon Grigorev (Github: [@gsvgit](https://github.com/gsvgit))

## License

This project licensed under MIT License. License text can be found in the
[license file](https://github.com/LeonidLodygin/ImageProcessing/blob/main/LICENSE.md).
