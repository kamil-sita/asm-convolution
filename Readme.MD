## About
This repository contains both low level (x64 assembly) and high level (C#) implementation of algorithm that applies [1-dimensional kernel to image](https://en.wikipedia.org/wiki/Kernel_(image_processing))

## Usage
### Arguments
Both versions of the algorithm take following arguments:
* kernelPtr* - pointer to values of the kernel
* imgInPtr* - pointer to input image layer
* imgOutPtr* - pointer to output image layer
* kernelSize - size of kernel array
* imgStartX - defines start (X axis) of rectangular area in which filter should be applied
* imgStartY - defined end (X axis) of rectangular area in which filter should be applied
* imgEndX - defines start (Y axis) of rectangular area in which filter should be applied
* imgEndY - defines end (Y axis) of rectangular area in which filter should be applied
* imgWidth - defines width of image (needed for 1D -> 2D translation)
* imgJmp - indicates distance between adjacent pixels on which filter should be applied. Use 1 if filter should be applied horizontally, and use imgWidth if filter should be applied vertically 

### Assumptions
* image size needs to be increased  to account for kernel size - for example if your filter is 5x5, you need to create artificial border of size 2 around the image.
* filter size **must** be odd

## Performance
Assembly version is about 30% faster than C# version. You can use more threads to further improve execution speed (see example).

## About assembly version
Assembly version has some improvements (and drawbacks) compared to C# version. First improvement is usage of addition and multiplication SIMD instructions (on packed double precision values). Because of that, the loop is partially changed - two iterations are done at once on two adjacent values. On last iteration only one value is checked and immediately results are calculated.

Assembly version requires 64 bit processor and support for SSE2 instructions.
