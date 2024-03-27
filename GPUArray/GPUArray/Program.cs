using System.Diagnostics;
using Cloo;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

class Program
{
    static void Main(string[] args)
    {
        const int arraySize = 200000000;
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
        float cpuAverage = array.Average();
        cpuStopwatch.Stop();
        TimeSpan averageCpuComputationTime = cpuStopwatch.Elapsed;

        cpuStopwatch.Restart();
        float cpuMin = array.Min();
        cpuStopwatch.Stop();
        TimeSpan minCpuComputationTime = cpuStopwatch.Elapsed;

        cpuStopwatch.Restart();
        float cpuMax = array.Max();
        cpuStopwatch.Stop();
        TimeSpan maxCpuComputationTime = cpuStopwatch.Elapsed;

        cpuStopwatch.Restart();
        float cpuMedian = array.Median();
        cpuStopwatch.Stop();
        TimeSpan medianCpuComputationTime = cpuStopwatch.Elapsed;

        // GPU rész
        ComputePlatform platform = ComputePlatform.Platforms[0];
        ComputeContext context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(platform), null, IntPtr.Zero);

        string kernelSource = File.ReadAllText("calculations.cl");
        ComputeProgram program = new ComputeProgram(context, kernelSource);

        try
        {
            program.Build(null, null, null, IntPtr.Zero);
        }
        catch (Exception)
        {
            string buildLog = program.GetBuildLog(context.Devices[0]);
            Console.WriteLine(buildLog);
        }

        int workGroupSize = 256;
        int numberOfGroups = (arraySize + workGroupSize - 1) / workGroupSize;

        ComputeKernel sumKernel = program.CreateKernel("Sum");
        ComputeBuffer<float> arrayBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, array);
        ComputeBuffer<float> resultBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly, numberOfGroups);
        sumKernel.SetMemoryArgument(0, arrayBuffer);
        sumKernel.SetMemoryArgument(1, resultBuffer);
        sumKernel.SetValueArgument(2, arraySize);

        ComputeCommandQueue queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

        Stopwatch gpuStopwatch = new Stopwatch();
        gpuStopwatch.Start();

        queue.Execute(sumKernel, null, new long[] { arraySize }, new long[] { workGroupSize }, null);

        float[] result = new float[numberOfGroups];
        queue.ReadFromBuffer(resultBuffer, ref result, true, null);

        gpuStopwatch.Stop();
        TimeSpan sumGpuComputationTime = gpuStopwatch.Elapsed;

        float gpuSum = 0;
        foreach (float partialSum in result)
        {
            gpuSum += partialSum;
        }

        ComputeKernel averageKernel = program.CreateKernel("CalculateSumAndAverage");
        ComputeBuffer<float> averageResultBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly, numberOfGroups);
        averageKernel.SetMemoryArgument(0, arrayBuffer);
        averageKernel.SetMemoryArgument(1, averageResultBuffer);
        averageKernel.SetValueArgument(2, arraySize);

        gpuStopwatch.Restart();
        queue.Execute(averageKernel, null, new long[] { arraySize }, new long[] { workGroupSize }, null);

        float[] sumResults = new float[numberOfGroups];
        queue.ReadFromBuffer(averageResultBuffer, ref sumResults, true, null);
        gpuStopwatch.Stop();

        float totalSum = 0;
        foreach (float groupSum in sumResults)
        {
            totalSum += groupSum;
        }
        float average = totalSum / arraySize;
        TimeSpan avgGpuComputationTime = gpuStopwatch.Elapsed;

        ComputeKernel minKernel = program.CreateKernel("CalculateMin");
        ComputeBuffer<float> minResultBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly, numberOfGroups);
        minKernel.SetMemoryArgument(0, arrayBuffer);
        minKernel.SetMemoryArgument(1, minResultBuffer);
        minKernel.SetValueArgument(2, arraySize);

        ComputeKernel maxKernel = program.CreateKernel("CalculateMax");
        ComputeBuffer<float> maxResultBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly, numberOfGroups);
        maxKernel.SetMemoryArgument(0, arrayBuffer);
        maxKernel.SetMemoryArgument(1, maxResultBuffer);
        maxKernel.SetValueArgument(2, arraySize);

        gpuStopwatch.Restart();
        queue.Execute(minKernel, null, new long[] { arraySize }, new long[] { workGroupSize }, null);
        float[] minResults = new float[numberOfGroups];
        queue.ReadFromBuffer(minResultBuffer, ref minResults, true, null);
        gpuStopwatch.Stop();
        TimeSpan minGpuComputationTime = gpuStopwatch.Elapsed;

        gpuStopwatch.Restart();
        queue.Execute(maxKernel, null, new long[] { arraySize }, new long[] { workGroupSize }, null);
        float[] maxResults = new float[numberOfGroups];
        queue.ReadFromBuffer(maxResultBuffer, ref maxResults, true, null);
        gpuStopwatch.Stop();
        TimeSpan maxGpuComputationTime = gpuStopwatch.Elapsed;

        float gpuMin = minResults.Min();
        float gpuMax = maxResults.Max();

        ComputeKernel sortKernel = program.CreateKernel("SortArray");
        sortKernel.SetMemoryArgument(0, arrayBuffer);
        sortKernel.SetValueArgument(1, arraySize);
        queue.Execute(sortKernel, null, new long[] { arraySize }, null, null);
        queue.ReadFromBuffer(arrayBuffer, ref array, true, null);

        ComputeKernel medianKernel = program.CreateKernel("CalcMedian");
        ComputeBuffer<float> medianResultBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly, 1);
        medianKernel.SetMemoryArgument(0, arrayBuffer);
        medianKernel.SetMemoryArgument(1, medianResultBuffer);
        medianKernel.SetValueArgument(2, arraySize);

        gpuStopwatch.Restart();
        queue.Execute(medianKernel, null, new long[] { arraySize }, null, null);
        float[] medianResult = new float[1];
        queue.ReadFromBuffer(medianResultBuffer, ref medianResult, true, null);
        gpuStopwatch.Stop();
        TimeSpan medianGpuComputationTime = gpuStopwatch.Elapsed;

        float gpuMedian = medianResult[0];

        // Eredmények kiírása
        Console.WriteLine($"Minták száma: {arraySize}");
        Console.WriteLine("CPU számítások:");
        Console.WriteLine($"Összeg (CPU): {cpuSum}");
        Console.WriteLine($"Számítási idő (CPU) - Összeg: {sumCpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Átlag (CPU): {cpuAverage}");
        Console.WriteLine($"Számítási idő (CPU) - Átlag: {averageCpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Minimum (CPU): {cpuMin}");
        Console.WriteLine($"Számítási idő (CPU) - Minimum: {minCpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Maximum (CPU): {cpuMax}");
        Console.WriteLine($"Számítási idő (CPU) - Maximum: {maxCpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Medián (CPU): {cpuMedian}");
        Console.WriteLine($"Számítási idő (CPU) - Medián: {medianCpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine("--------------------------------------");
        Console.WriteLine("GPU számítások:");
        Console.WriteLine($"Összeg (GPU): {gpuSum}");
        Console.WriteLine($"Számítási idő (GPU) - Összeg: {sumGpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Átlag (GPU): {average}");
        Console.WriteLine($"Számítási idő (GPU) - Átlag: {avgGpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Minimum (GPU): {gpuMin}");
        Console.WriteLine($"Számítási idő (GPU) - Minimum: {minGpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Maximum (GPU): {gpuMax}");
        Console.WriteLine($"Számítási idő (GPU) - Maximum: {maxGpuComputationTime.TotalMilliseconds} ms");
        Console.WriteLine($"Medián (GPU): {gpuMedian}");
        Console.WriteLine($"Számítási idő (GPU) - Maximum: {medianGpuComputationTime.TotalMilliseconds} ms");

        // Takarítás
        arrayBuffer.Dispose();
        resultBuffer.Dispose();
        sumKernel.Dispose();
        averageKernel.Dispose();
        minKernel.Dispose();
        maxKernel.Dispose();
        medianKernel.Dispose();
        program.Dispose();
        context.Dispose();
    }
}