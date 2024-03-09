using System;
using System.Diagnostics;
using System.Linq;
using Cloo;

class Program
{
    static void Main(string[] args)
    {
        const int arraySize = 500000000; // A tömb mérete
        float[] array = new float[arraySize];
        float sum = 0;

        // Tömb inicializálása véletlenszámokkal
        Random rand = new Random();
        for (int i = 0; i < arraySize; i++)
        {
            array[i] = (float)rand.NextDouble();
            sum += array[i];
        }

        // OpenCL inicializálása
        ComputePlatform platform = ComputePlatform.Platforms[0];
        ComputeContextPropertyList properties = new ComputeContextPropertyList(platform);
        ComputeContext context = new ComputeContext(ComputeDeviceTypes.Gpu, properties, null, IntPtr.Zero);

        // Kernel betöltése és létrehozása
        string kernelSource = @"
            __kernel void Sum(__global float* array, const int length) {
                int global_id = get_global_id(0);
                float sum = 0;
                if (global_id < length) {
                    sum += array[global_id];
                }
                array[global_id] = sum;
            }
        ";

        ComputeProgram program = new ComputeProgram(context, kernelSource);
        program.Build(null, null, null, IntPtr.Zero);

        ComputeKernel kernel = program.CreateKernel("Sum");

        // Tömb létrehozása és feltöltése a számításhoz
        ComputeBuffer<float> arrayBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, array);
        kernel.SetMemoryArgument(0, arrayBuffer);

        // Kernel paraméterek beállítása
        int arrayLength = array.Length;
        kernel.SetValueArgument(1, arrayLength);

        // Mérjük az időt a GPU-n történő számítás elvégzéséhez
        Stopwatch gpuStopwatch = new Stopwatch();
        gpuStopwatch.Start();

        // Tömb összegzése a kernel segítségével a GPU-n
        ComputeCommandQueue gpuQueue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
        gpuQueue.Execute(kernel, null, new long[] { arraySize }, null, null);

        // Számítási idő a GPU-n leállítása
        gpuStopwatch.Stop();
        TimeSpan gpuComputationTime = gpuStopwatch.Elapsed;

        // Tömb eredményének visszaolvasása a GPU-ról
        gpuQueue.ReadFromBuffer(arrayBuffer, ref array, true, null);

        // Eredmény kiíratása a GPU-ról
        float sumFromGPU = array.Sum();

        // Mérjük az időt a CPU-n történő számítás elvégzéséhez
        Stopwatch cpuStopwatch = new Stopwatch();
        cpuStopwatch.Start();

        // Tömb összegzése a CPU-n
        float sumFromCPU = 0;
        foreach (float value in array)
        {
            sumFromCPU += value;
        }

        // Számítási idő a CPU-n leállítása
        cpuStopwatch.Stop();
        TimeSpan cpuComputationTime = cpuStopwatch.Elapsed;

        Console.WriteLine($"Összeg (CPU): {sum}");
        Console.WriteLine($"Összeg (GPU): {sumFromGPU}");
        Console.WriteLine($"Számítási idő (CPU): {cpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Számítási idő (GPU): {gpuComputationTime.TotalMilliseconds} ms");

        // Takarítás
        arrayBuffer.Dispose();
        kernel.Dispose();
        program.Dispose();
        context.Dispose();
    }
}
