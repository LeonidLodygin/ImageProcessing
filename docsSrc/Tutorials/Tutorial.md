---
title: Get Started
category: Tutorials
categoryindex: 0
index: 100
---

# Get Started

In this tutorial we will look at how to get started with the ImageProcessing library and process your first images.

## Installing ImageProcessing

```sh
> dotnet add package LeonidLodygin.ImageProcessing --version 1.0.0
```

## Processing of images

### Prepare your images

Decide on the image you want to process. You can also process several images at once, in which case specify the path to the directory with your images.

In the command line parameter "-i" use the path to the image, for "-o" use the path where you want to save the image.
<div class="alert alert-primary" role="alert">
    <p>
        NOTE: Do not use the same directory as your images for saving, if you do, you will lose the original images!
    </p>
</div>

### Choose the desired modifications

Decide on the modifications you want to apply to the image. Here is a complete list of available modifications:

- Gauss5x5 
- Gauss7x7
- Edges
- Sharpen
- Emboss
- ClockwiseRotation
- CounterClockwiseRotation
- MirrorVertical
- MirrorHorizontal
- FishEye

Use the selected modification or modification list for the "-mod" parameter.

### CPU or GPU processing?

By default, all processing will be done at the expense of the CPU. If you want to process images using GPGPU, use the "-gpu" parameter (if the device has a video card):

- AnyGpu
- Nvidia
- Amd
- Intel

### How many logical cores does your system have?

In the case of processing a large number of images, it would be logical to utilize the parallel processing power of your device. To do this, use the "-ag" or "-sag" parameter. The "-ag" parameter will split image processing and saving tasks into two separate computational threads. The "-sag" parameter will allocate the number of threads you need to process and save images independently. For this parameter you should specify the number of threads you need.

### Let's start processing!

The end result may look like the following:
```sh
> dotnet run -i *input path* -o *output path* -mod FishEye -gpu AnyGpu
```