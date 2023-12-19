---
title: How to code
category: Guides
categoryindex: 1
index: 100
---

# How to code

In this tutorial, we will look at how to work with the ImageProcessing library using code rather than console commands.

## Installing ImageProcessing

```sh
> dotnet add package LeonidLodygin.ImageProcessing --version 1.0.0
```
<div class="alert alert-primary" role="alert">
    <p>
        NOTE: The library uses .NET 7.0. Make sure your application complies with this requirement.
    </p>
</div>

Load your image using the `loadAsImage` function from the `MyImage` module.

```sh
> let image = loadAsImage "path to the image"
```

For CPU and GPU the list of transforms is identical, decide what you want to process your image on and select the appropriate function to process from the `CpuProcessing` module or the `GpuProcessing` module respectively.

### In the case of CPU processing:

Apply the fisheye filter to the uploaded image.

```sh
> let newImage = fishEye image
```

Don't forget to save the processed image using the saveImage function from the `MyImage` module!

```sh
> let newImage = saveImage "path"
```

### In the case of GPU processing:

In the case of GPU processing, you have to go through a few extra steps to achieve your goal:

Prepare OpenCl context and queue(from `Brahma.FSharp` module):

```sh
> let clContext = ClContext(ClDevice.GetFirstAppropriateDevice(device))
> let queue = clContext.QueueProvider.CreateQueue()
```

Compile the kernel to apply the filter using the `fishEyeKernel` function from the `GpuKernels` module: 

```sh
> let fishKernel = fishEyeKernel clContext
```

Process the image using the `fishEye` function from the `GpuProcessing` module:

```sh
let newImage = fishEye fishKernel clContext 64 queue image
```

Don't forget to save the new image:

```sh
> let newImage = saveImage "path"
```

<div class="alert alert-primary" role="alert">
    <p>
        For more info about GPU processing please check <a href="https://yaccconstructor.github.io/Brahma.FSharp/">Brahma
    </p>
</div>
