using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cloo;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

class Program
{
    static void Main(string[] args)
    {
        const int arraySize = 200000000; // A tömb mérete
        float[] array = new float[arraySize];
        float sum = 0;

        // Tömb inicializálása normális eloszlású véletlen számokkal
        Normal normalDistribution = new Normal();
        for (int i = 0; i < arraySize; i++)
        {
            array[i] = (float)normalDistribution.Sample();
            sum += array[i];
        }

        /// CPU part
        Stopwatch cpuStopwatch = new Stopwatch();

        cpuStopwatch.Start();
        float cpuSum = array.Sum();
        cpuStopwatch.Stop();
        TimeSpan sumCpuComputationTime = cpuStopwatch.Elapsed;

        cpuStopwatch.Restart();
        float cpuMin = array.Min();
        cpuStopwatch.Stop();
        TimeSpan minCpuComputationTime = cpuStopwatch.Elapsed;

        cpuStopwatch.Restart();
        float cpuMax = array.Max();
        cpuStopwatch.Stop();
        TimeSpan maxCpuComputationTime = cpuStopwatch.Elapsed;

        cpuStopwatch.Restart();
        float cpuAverage = array.Average();
        cpuStopwatch.Stop();
        TimeSpan averageCpuComputationTime = cpuStopwatch.Elapsed;

        cpuStopwatch.Restart();
        float cpuMedian = array.Median();
        cpuStopwatch.Stop();
        TimeSpan medianCpuComputationTime = cpuStopwatch.Elapsed;

        /// GPU part
        // OpenCL inicializálása
        ComputePlatform platform = ComputePlatform.Platforms[0];
        ComputeContextPropertyList properties = new ComputeContextPropertyList(platform);
        ComputeContext context = new ComputeContext(ComputeDeviceTypes.Gpu, properties, null, IntPtr.Zero);

        // Kernel betöltése és létrehozása
        string kernelSource = File.ReadAllText("calculations.cl");
        ComputeProgram program = new ComputeProgram(context, kernelSource);
        program.Build(null, null, null, IntPtr.Zero);

        // Sum kernel
        ComputeKernel sumKernel = program.CreateKernel("Sum");
        ComputeBuffer<float> sumArrayBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, array);
        sumKernel.SetMemoryArgument(0, sumArrayBuffer);
        sumKernel.SetValueArgument(1, arraySize);

        // Min kernel
        ComputeKernel minKernel = program.CreateKernel("Min");
        ComputeBuffer<float> minArrayBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, array);
        minKernel.SetMemoryArgument(0, minArrayBuffer);
        minKernel.SetValueArgument(1, arraySize);

        // Max kernel
        ComputeKernel maxKernel = program.CreateKernel("Max");
        ComputeBuffer<float> maxArrayBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, array);
        maxKernel.SetMemoryArgument(0, maxArrayBuffer);
        maxKernel.SetValueArgument(1, arraySize);

        // Average kernel
        ComputeKernel averageKernel = program.CreateKernel("Average");
        ComputeBuffer<float> averageArrayBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, array);
        averageKernel.SetMemoryArgument(0, averageArrayBuffer);
        averageKernel.SetValueArgument(1, arraySize);

        // Median kernel
        ComputeKernel medianKernel = program.CreateKernel("Median");
        ComputeBuffer<float> medianArrayBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, array);
        medianKernel.SetMemoryArgument(0, medianArrayBuffer);
        medianKernel.SetValueArgument(1, arraySize);

        // Mérjük az időt a GPU-n történő számítás elvégzéséhez
        Stopwatch gpuStopwatch = new Stopwatch();

        // Sum kernel számítási idő mérése
        gpuStopwatch.Start();
        ComputeCommandQueue gpuQueue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
        gpuQueue.Execute(sumKernel, null, new long[] { arraySize }, null, null);
        gpuQueue.ReadFromBuffer(sumArrayBuffer, ref array, true, null);
        gpuStopwatch.Stop();
        TimeSpan sumGpuComputationTime = gpuStopwatch.Elapsed;

        // Min kernel számítási idő mérése
        gpuStopwatch.Restart();
        gpuQueue.Execute(minKernel, null, new long[] { arraySize }, null, null);
        gpuQueue.ReadFromBuffer(minArrayBuffer, ref array, true, null);
        gpuStopwatch.Stop();
        TimeSpan minGpuComputationTime = gpuStopwatch.Elapsed;

        // Max kernel számítási idő mérése
        gpuStopwatch.Restart();
        gpuQueue.Execute(maxKernel, null, new long[] { arraySize }, null, null);
        gpuQueue.ReadFromBuffer(maxArrayBuffer, ref array, true, null);
        gpuStopwatch.Stop();
        TimeSpan maxGpuComputationTime = gpuStopwatch.Elapsed;

        // Average kernel számítási idő mérése
        gpuStopwatch.Restart();
        gpuQueue.Execute(averageKernel, null, new long[] { arraySize }, null, null);
        gpuQueue.ReadFromBuffer(averageArrayBuffer, ref array, true, null);
        gpuStopwatch.Stop();
        TimeSpan averageGpuComputationTime = gpuStopwatch.Elapsed;

        /*// Median kernel számítási idő mérése
        gpuStopwatch.Restart();
        gpuQueue.Execute(medianKernel, null, new long[] { arraySize }, null, null);
        gpuQueue.ReadFromBuffer(medianArrayBuffer, ref array, true, null);
        gpuStopwatch.Stop();
        TimeSpan medianGpuComputationTime = gpuStopwatch.Elapsed;*/

        // Eredmény kiíratása
        Console.WriteLine($"Minták száma: {arraySize}");
        Console.WriteLine("CPU számítások:");
        Console.WriteLine($"Összeg (CPU): {cpuSum}");
        Console.WriteLine($"Számítási idő (CPU) - Összeg: {sumCpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Minimum (CPU): {cpuMin}");
        Console.WriteLine($"Számítási idő (CPU) - Minimum: {minCpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Maximum (CPU): {cpuMax}");
        Console.WriteLine($"Számítási idő (CPU) - Maximum: {maxCpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Átlag (CPU): {cpuAverage}");
        Console.WriteLine($"Számítási idő (CPU) - Átlag: {averageCpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Medián (CPU): {cpuMedian}");
        Console.WriteLine($"Számítási idő (CPU) - Medián: {medianCpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine("--------------------------------------");
        Console.WriteLine("GPU számítások:");
        Console.WriteLine($"Összeg (GPU): {array.Sum()}");
        Console.WriteLine($"Számítási idő (GPU) - Összeg: {sumGpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Minimum (GPU): {array.Min()}");
        Console.WriteLine($"Számítási idő (GPU) - Minimum: {minGpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Maximum (GPU): {array.Max()}");
        Console.WriteLine($"Számítási idő (GPU) - Maximum: {maxGpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Átlag (GPU): {array.Average()}");
        Console.WriteLine($"Számítási idő (GPU) - Átlag: {averageGpuComputationTime.TotalMilliseconds} ms");
        /*Console.WriteLine($"Medián (GPU): {array.Median()}");
        Console.WriteLine($"Számítási idő (GPU) - Medián: {medianGpuComputationTime.TotalMilliseconds} ms");*/

        // Takarítás
        sumArrayBuffer.Dispose();
        minArrayBuffer.Dispose();
        maxArrayBuffer.Dispose();
        averageArrayBuffer.Dispose();
        sumKernel.Dispose();
        minKernel.Dispose();
        maxKernel.Dispose();
        averageKernel.Dispose();
        program.Dispose();
        context.Dispose();
    }
}
