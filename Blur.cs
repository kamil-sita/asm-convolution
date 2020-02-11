namespace BlurCs
{
    public unsafe class Blur
    {
        [DllExport(ExportName = "filter", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        public static void filtercs(double* kernelPtr, byte* imgInPtr, byte* imgOutPtr, int kernelSize, int imgStartX, int imgStartY, int imgEndX, int imgEndY, int imgWidth, int imgJmp)
        {

            int kernelHalf = kernelSize / 2;

            for (int y = imgStartY; y < imgEndY; y++)
            {

                for (int x = imgStartX; x < imgEndX; x++)
                {

                    double sum = 0;
                    int ptr = y * imgWidth + x; 

                    for (int k = -kernelHalf; k <= kernelHalf; k++)
                    {
                        int krnptr = k + kernelHalf; 
                        int krnlimgptr = ptr + k * imgJmp; 

                        sum += (imgInPtr[krnlimgptr] * kernelPtr[krnptr]);
                    }

                    imgOutPtr[ptr] = (byte)sum;
                }

            }

        }
    }
}