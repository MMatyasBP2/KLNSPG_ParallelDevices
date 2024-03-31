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

        #region Kernel handler

        private int CalculateNumberOfGroups(long arraySize) => (int)Math.Ceiling((double)arraySize / WorkGroupSize);

        private ComputeKernel CreateKernel(string kernelName, float[] array, long arraySize, int numberOfGroups)
        {
            ComputeKernel kernel;

            try
            {
                kernel = Program.CreateKernel(kernelName);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid kernel name!");
            }

            return kernel;
        }

        private ComputeBuffer<float> CreateArrayBuffer(float[] array) => new ComputeBuffer<float>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, array);

        private ComputeBuffer<float> CreateResultBuffer(int numberOfGroups) => new ComputeBuffer<float>(Context, ComputeMemoryFlags.WriteOnly, numberOfGroups);

        private void ExecuteKernel(ComputeKernel kernel, ComputeBuffer<float> inputBuffer, ComputeBuffer<float> outputBuffer, ComputeCommandQueue queue, long arraySize, int numberOfGroups, Stopwatch gpuStopwatch)
        {
            gpuStopwatch.Start();

            long[] globalWorkSize = new long[] { numberOfGroups * WorkGroupSize };
            long[] localWorkSize = new long[] { WorkGroupSize };

            queue.Execute(kernel, null, globalWorkSize, localWorkSize, null);

            gpuStopwatch.Stop();
            SetGpuComputationTime(gpuStopwatch);
        }

        private float CalculateSumFromResultBuffer(ComputeBuffer<float> resultBuffer, ComputeCommandQueue queue, int numberOfGroups)
        {
            float[] result = new float[numberOfGroups];
            queue.ReadFromBuffer(resultBuffer, ref result, true, null);

            float gpuSum = 0;
            foreach (float partialSum in result)
                gpuSum += partialSum;

            return gpuSum;
        }

        private float CalculateAverageFromResultBuffer(ComputeBuffer<float> resultBuffer, ComputeCommandQueue queue, long arraySize)
        {
            float[] sumResults = new float[CalculateNumberOfGroups(arraySize)];
            queue.ReadFromBuffer(resultBuffer, ref sumResults, true, null);

            float totalSum = 0;
            foreach (float groupSum in sumResults)
                totalSum += groupSum;

            return totalSum / arraySize;
        }

        private float CalculateMinimumFromResultBuffer(ComputeBuffer<float> resultBuffer, ComputeCommandQueue queue, long arraySize)
        {
            float[] minResult = new float[CalculateNumberOfGroups(arraySize)];
            queue.ReadFromBuffer(resultBuffer, ref minResult, true, null);
            return minResult.Min();
        }

        private float CalculateMaximumFromResultBuffer(ComputeBuffer<float> resultBuffer, ComputeCommandQueue queue, long arraySize)
        {
            float[] maxResult = new float[CalculateNumberOfGroups(arraySize)];
            queue.ReadFromBuffer(resultBuffer, ref maxResult, true, null);
            return maxResult.Max();
        }

        private double GetGpuComputationTime(Stopwatch gpuStopwatch) => gpuStopwatch.Elapsed.TotalMilliseconds;

        private void SetGpuComputationTime(Stopwatch gpuStopwatch)
        {
            gpuStopwatch.Stop();
            gpuStopwatch.Reset();
            gpuStopwatch.Start();
        }

        private void CleanupResources(ComputeKernel kernel, ComputeBuffer<float> arrayBuffer, ComputeBuffer<float> resultBuffer)
        {
            kernel.Dispose();
            arrayBuffer.Dispose();
            resultBuffer.Dispose();
        }

        private void SetMemoryArgument(ComputeKernel actualKernel, ComputeBuffer<float> arrayBuffer, ComputeBuffer<float> resultBuffer, long arraySize)
        {
            actualKernel.SetMemoryArgument(0, arrayBuffer);
            actualKernel.SetMemoryArgument(1, resultBuffer);
            actualKernel.SetValueArgument(2, arraySize);
        }

        #endregion

        #region Calculations
        public (float, double) CalculateSum(float[] array)
        {
            try
            {
                long arraySize = array.Length;
                int numberOfGroups = CalculateNumberOfGroups(arraySize);

                ComputeKernel sumKernel = CreateKernel("Sum", array, arraySize, numberOfGroups);
                ComputeBuffer<float> arrayBuffer = CreateArrayBuffer(array);
                ComputeBuffer<float> resultBuffer = CreateResultBuffer(numberOfGroups);
                SetMemoryArgument(sumKernel, arrayBuffer, resultBuffer, arraySize);

                ComputeCommandQueue queue = new ComputeCommandQueue(Context, Context.Devices[0], ComputeCommandQueueFlags.None);
                Stopwatch sw = new Stopwatch();

                ExecuteKernel(sumKernel, arrayBuffer, resultBuffer, queue, arraySize, numberOfGroups, sw);

                float gpuSum = CalculateSumFromResultBuffer(resultBuffer, queue, numberOfGroups);
                double sumGpuComputationTime = GetGpuComputationTime(sw);

                CleanupResources(sumKernel, arrayBuffer, resultBuffer);

                return (gpuSum, sumGpuComputationTime);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public (float, double) CalculateAverage(float[] array)
        {
            try
            {
                long arraySize = array.Length;
                int numberOfGroups = CalculateNumberOfGroups(arraySize);

                ComputeKernel averageKernel = CreateKernel("CalculateSumAndAverage", array, arraySize, numberOfGroups);
                ComputeBuffer<float> arrayBuffer = CreateArrayBuffer(array);
                ComputeBuffer<float> resultBuffer = CreateResultBuffer(numberOfGroups);
                SetMemoryArgument(averageKernel, arrayBuffer, resultBuffer, arraySize);

                ComputeCommandQueue queue = new ComputeCommandQueue(Context, Context.Devices[0], ComputeCommandQueueFlags.None);
                Stopwatch sw = new Stopwatch();

                ExecuteKernel(averageKernel, arrayBuffer, resultBuffer, queue, arraySize, numberOfGroups, sw);

                float average = CalculateAverageFromResultBuffer(resultBuffer, queue, arraySize);
                double averageGpuComputationTime = GetGpuComputationTime(sw);

                CleanupResources(averageKernel, arrayBuffer, resultBuffer);

                return (average, averageGpuComputationTime);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public (float, double) CalculateMin(float[] array)
        {
            try
            {
                long arraySize = array.Length;
                int numberOfGroups = CalculateNumberOfGroups(arraySize);

                ComputeKernel minKernel = CreateKernel("CalculateMin", array, arraySize, numberOfGroups);
                ComputeBuffer<float> arrayBuffer = CreateArrayBuffer(array);
                ComputeBuffer<float> resultBuffer = CreateResultBuffer(numberOfGroups);
                SetMemoryArgument(minKernel, arrayBuffer, resultBuffer, arraySize);

                ComputeCommandQueue queue = new ComputeCommandQueue(Context, Context.Devices[0], ComputeCommandQueueFlags.None);
                Stopwatch sw = new Stopwatch();

                ExecuteKernel(minKernel, arrayBuffer, resultBuffer, queue, arraySize, numberOfGroups, sw);

                float min = CalculateMinimumFromResultBuffer(resultBuffer, queue, arraySize);
                double minimumGpuComputationTime = GetGpuComputationTime(sw);
                CleanupResources(minKernel, arrayBuffer, resultBuffer);

                return (min, minimumGpuComputationTime);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public (float, double) CalculateMax(float[] array)
        {
            try
            {
                long arraySize = array.Length;
                int numberOfGroups = CalculateNumberOfGroups(arraySize);

                ComputeKernel maxKernel = CreateKernel("CalculateMax", array, arraySize, numberOfGroups);
                ComputeBuffer<float> arrayBuffer = CreateArrayBuffer(array);
                ComputeBuffer<float> resultBuffer = CreateResultBuffer(numberOfGroups);
                SetMemoryArgument(maxKernel, arrayBuffer, resultBuffer, arraySize);

                ComputeCommandQueue queue = new ComputeCommandQueue(Context, Context.Devices[0], ComputeCommandQueueFlags.None);
                Stopwatch sw = new Stopwatch();

                ExecuteKernel(maxKernel, arrayBuffer, resultBuffer, queue, arraySize, numberOfGroups, sw);

                float max = CalculateMaximumFromResultBuffer(resultBuffer, queue, arraySize);
                double maximumGpuComputationTime = GetGpuComputationTime(sw);
                CleanupResources(maxKernel, arrayBuffer, resultBuffer);

                return (max, maximumGpuComputationTime);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public (float, double) CalculateMedian(float[] array)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
