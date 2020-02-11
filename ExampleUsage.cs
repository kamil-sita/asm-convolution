/**
This example shows multithreaded applying of given filter. 
ImageLayers represents a simple structure with R, G, B layers as byte arrays, with already extended borders.


*/

namespace BlurAppExample
{
    class Blur 
    {

        //główna funkcja rozmywająca (aplikująca)
        public static void blur(double[] kernel, ImageLayers imgin, int threads, bool isAsm)
        {
            int imgLength = imgin.layerR.Length;

            //temporary layers for filter
            byte[] newR0 = new byte[imgLength];
            byte[] newG0 = new byte[imgLength];
            byte[] newB0 = new byte[imgLength];

            int kernelSize = kernel.Length;
            int halfKernel = (kernelSize) / 2;

            //thread creation
            Thread[] threads0 = new Thread[threads];

            int halfwayFinished = 0;

			//assigning work to threads
            for (int i = 0; i < threads; i++)
            {
                //horizontal filtering
                int lowerBound = getBound(i, threads, imgin.getCorrectedHeight());
                int upperBound = getBound(i + 1, threads, imgin.getCorrectedHeight());

                //vertical filtering
                int lowerBound2 = getBound(i, threads, imgin.height);
                int upperBound2 = getBound(i + 1, threads, imgin.height);

                threads0[i] = new Thread(() =>
                {
                    blurFragment(kernel, imgin.layerR, newR0, kernelSize, halfKernel, lowerBound, imgin.width + halfKernel, upperBound, imgin.width + 2 * halfKernel, true, isAsm);
                    blurFragment(kernel, imgin.layerG, newG0, kernelSize, halfKernel, lowerBound, imgin.width + halfKernel, upperBound, imgin.width + 2 * halfKernel, true, isAsm);
                    blurFragment(kernel, imgin.layerB, newB0, kernelSize, halfKernel, lowerBound, imgin.width + halfKernel, upperBound, imgin.width + 2 * halfKernel, true, isAsm);

                    Interlocked.Increment(ref halfwayFinished);

					//synchronization - waiting for all threads to finish their work
                    while (halfwayFinished != threads)
                    {
                       Thread.Sleep(10);
                    }

                    blurFragment(kernel, newR0, imgin.layerR, kernelSize, halfKernel, lowerBound2 + halfKernel, imgin.width + halfKernel, upperBound2 + halfKernel, imgin.width + 2 * halfKernel, false, isAsm);
                    blurFragment(kernel, newG0, imgin.layerG, kernelSize, halfKernel, lowerBound2 + halfKernel, imgin.width + halfKernel, upperBound2 + halfKernel, imgin.width + 2 * halfKernel, false, isAsm);
                    blurFragment(kernel, newB0, imgin.layerB, kernelSize, halfKernel, lowerBound2 + halfKernel, imgin.width + halfKernel, upperBound2 + halfKernel, imgin.width + 2 * halfKernel, false, isAsm);

                });

                threads0[i].Start();
            }

			//waiting for all threads to finish
            for (int i = 0; i < threads; i++)
            {
                threads0[i].Join();
            }
        }

        private static int getBound(int threadId, int threads, int max)
        {
            if (threadId == threads) {
                return max;
            }
            int jmp = (int)((1.0 * max) / threads);

            return threadId * jmp;
        }

        private static void blurFragment(double[] kernel, byte[] imgIn, byte[] imgOut, int kernelSize, int startX, int startY, int endX, int endY, int imgWidth, bool isHorizontal, bool isAsm)
        {
            int jmp = 1;
            if (!isHorizontal)
            {
                jmp = imgWidth;
            }
            filterCall(isAsm, kernel, imgIn, imgOut, kernelSize, startX, startY, endX, endY, imgWidth, jmp);
        }
        
        public static void filterCall(bool isAsm, double[] kernel, byte[] imgIn, byte[] imgOut, int kernelSize, int imgStartX, int imgStartY, int imgEndX, int imgEndY, int imgWidth, int imgJmp)
        {
            unsafe
            {
                fixed (double* kernelPtr = &kernel[0])
                {
                    fixed (byte* imgInPtr = &imgIn[0])
                    {
                        fixed (byte* imgOutPtr = &imgOut[0])
                        {
                            if (isAsm)
                            {
                                filterasm(kernelPtr, imgInPtr, imgOutPtr, kernelSize, imgStartX, imgStartY, imgEndX, imgEndY, imgWidth, imgJmp);
                            }
                            else
                            {
                                filter(kernelPtr, imgInPtr, imgOutPtr, kernelSize, imgStartX, imgStartY, imgEndX, imgEndY, imgWidth, imgJmp);
                            }
                        }
                    }
                }
            }
        }
		
		


        [DllImport("DLL_CS.dll")]
        private unsafe static extern void filtercs(double* kernel, byte* imgIn, byte* imgOut, int kernelSize, int imgStartX, int imgStartY, int imgEndX, int imgEndY, int imgWidth, int imgJmp);

        [DllImport("DLL_ASM.dll")]
        public unsafe static extern void filterasm(double* kernel, byte* imgIn, byte* imgOut, int kernelSize, int imgStartX, int imgStartY, int imgEndX, int imgEndY, int imgWidth, int imgJmp);
    }
}
