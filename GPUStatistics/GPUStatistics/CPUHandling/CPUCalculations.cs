using MathNet.Numerics.Statistics;
using System.Diagnostics;

namespace GPUStatistics.CPUHandling
{
    public class CPUCalculations : ICalculations
    {
        public (float, double) CalculateAverage(float[] array)
        {
            Stopwatch cpuStopwatch = new Stopwatch();
            cpuStopwatch.Start();
            float cpuAverage = array.Average();
            cpuStopwatch.Stop();
            double averageCpuComputationTime = cpuStopwatch.Elapsed.TotalMilliseconds;
            return (cpuAverage, averageCpuComputationTime);
        }

        public (float, double) CalculateMax(float[] array)
        {
            Stopwatch cpuStopwatch = new Stopwatch();
            cpuStopwatch.Start();
            float cpuMax = array.Max();
            cpuStopwatch.Stop();
            double maxCpuComputationTime = cpuStopwatch.Elapsed.TotalMilliseconds;
            return (cpuMax, maxCpuComputationTime);
        }

        public (float, double) CalculateMedian(float[] array)
        {
            Stopwatch cpuStopwatch = new Stopwatch();
            cpuStopwatch.Start();
            float cpuMedian = array.Median();
            cpuStopwatch.Stop();
            double medianCpuComputationTime = cpuStopwatch.Elapsed.TotalMilliseconds;
            return (cpuMedian, medianCpuComputationTime);
        }

        public (float, double) CalculateMin(float[] array)
        {
            Stopwatch cpuStopwatch = new Stopwatch();
            cpuStopwatch.Start();
            float cpuMin = array.Min();
            cpuStopwatch.Stop();
            double minCpuComputationTime = cpuStopwatch.Elapsed.TotalMilliseconds;
            return (cpuMin, minCpuComputationTime);
        }

        public (float, double) CalculateSum(float[] array)
        {
            Stopwatch cpuStopwatch = new Stopwatch();
            cpuStopwatch.Start();
            float cpuSum = array.Sum();
            cpuStopwatch.Stop();
            double sumCpuComputationTime = cpuStopwatch.Elapsed.TotalMilliseconds;
            return (cpuSum, sumCpuComputationTime);
        }
    }
}
