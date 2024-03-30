using Cloo;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace GPUStatistics.GPUHandling
{
    public class GPUHandler : ICalculations
    {
        public ComputeContext Context { get; set; }
        public ComputeProgram Program { get; set; }
        public int WorkGroupSize { get; set; }
        public int NumberOfGroups { get; set; }

        public GPUHandler(int WorkGroupSize)
        {
            try
            {
                this.WorkGroupSize = WorkGroupSize;

                ComputePlatform platform = ComputePlatform.Platforms[0];
                Context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(platform), null, nint.Zero);

                string kernelSource = File.ReadAllText("./Kernelfile/calculations.cl");
                Program = new ComputeProgram(Context, kernelSource);

                Program.Build(null, null, null, nint.Zero);
            }
            catch (Exception ex)
            {
                if (Program != null && Context != null && Context.Devices.Count > 0)
                {
                    string buildLog = Program.GetBuildLog(Context.Devices[0]);
                    string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                    Directory.CreateDirectory(logDirectory);

                    string logFileName = Path.Combine(logDirectory, $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    File.WriteAllText(logFileName, buildLog);
                    throw new Exception($"Build log saved to: {logFileName}");
                }
                else
                {
                    throw new Exception($"Initialization failed, unable to retrieve build log. Exception: {ex.Message}");
                }
            }
        }

        #region Calculations
        public (float, double) CalculateSum(float[] array)
        {
            long arraySize = (long)MainWindow.main.SizeSlider.Value;
            int numberOfGroups = ((int)arraySize + WorkGroupSize - 1) / WorkGroupSize;
            ComputeKernel sumKernel = Program.CreateKernel("Sum");
            ComputeBuffer<float> arrayBuffer = new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, array);
            ComputeBuffer<float> resultBuffer = new ComputeBuffer<float>(Context, ComputeMemoryFlags.WriteOnly, numberOfGroups);
            sumKernel.SetMemoryArgument(0, arrayBuffer);
            sumKernel.SetMemoryArgument(1, resultBuffer);
            sumKernel.SetValueArgument(2, arraySize);

            ComputeCommandQueue queue = new ComputeCommandQueue(Context, Context.Devices[0], ComputeCommandQueueFlags.None);
            Stopwatch gpuStopwatch = new Stopwatch();
            gpuStopwatch.Start();

            queue.Execute(sumKernel, null, new long[] { arraySize }, new long[] { WorkGroupSize }, null);

            float[] result = new float[numberOfGroups];
            queue.ReadFromBuffer(resultBuffer, ref result, true, null);

            gpuStopwatch.Stop();
            double sumGpuComputationTime = gpuStopwatch.Elapsed.TotalMilliseconds;

            float gpuSum = 0;
            foreach (float partialSum in result)
                gpuSum += partialSum;

            return (gpuSum, sumGpuComputationTime);
        }

        public (float, double) CalculateAverage(float[] array)
        {
            throw new NotImplementedException();
        }

        public (float, double) CalculateMin(float[] array)
        {
            throw new NotImplementedException();
        }

        public (float, double) CalculateMax(float[] array)
        {
            throw new NotImplementedException();
        }

        public (float, double) CalculateMedian(float[] array)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
