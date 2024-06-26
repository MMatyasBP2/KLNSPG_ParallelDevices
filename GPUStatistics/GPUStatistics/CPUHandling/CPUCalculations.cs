﻿using System.Diagnostics;

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
            Trace.WriteLine("Process 30");
            return (cpuAverage, averageCpuComputationTime);
        }

        public (float, double) CalculateMax(float[] array)
        {
            Stopwatch cpuStopwatch = new Stopwatch();
            cpuStopwatch.Start();
            float cpuMax = array.Max();
            cpuStopwatch.Stop();
            double maxCpuComputationTime = cpuStopwatch.Elapsed.TotalMilliseconds;
            Trace.WriteLine("Process 60");
            return (cpuMax, maxCpuComputationTime);
        }

        public (float, double) CalculateMedian(float[] array)
        {
            Stopwatch cpuStopwatch = new Stopwatch();
            cpuStopwatch.Start();
            float cpuMedian = Median(array);
            cpuStopwatch.Stop();
            double medianCpuComputationTime = cpuStopwatch.Elapsed.TotalMilliseconds;
            Trace.WriteLine("Process 80");
            return (cpuMedian, medianCpuComputationTime);
        }

        public (float, double) CalculateMin(float[] array)
        {
            Stopwatch cpuStopwatch = new Stopwatch();
            cpuStopwatch.Start();
            float cpuMin = array.Min();
            cpuStopwatch.Stop();
            double minCpuComputationTime = cpuStopwatch.Elapsed.TotalMilliseconds;
            Trace.WriteLine("Process 40");
            return (cpuMin, minCpuComputationTime);
        }

        public (float, double) CalculateSum(float[] array)
        {
            Stopwatch cpuStopwatch = new Stopwatch();
            cpuStopwatch.Start();
            float cpuSum = array.Sum();
            cpuStopwatch.Stop();
            double sumCpuComputationTime = cpuStopwatch.Elapsed.TotalMilliseconds;
            Trace.WriteLine("Process 10");
            return (cpuSum, sumCpuComputationTime);
        }

        private float Median(float[] numbers)
        {
            Array.Sort(numbers);

            float median;
            if (numbers.Length % 2 == 0)
            {
                int middleIndex = numbers.Length / 2;
                median = (numbers[middleIndex - 1] + numbers[middleIndex]) / 2.0f;
            }
            else
                median = numbers[numbers.Length / 2];

            return median;
        }
    }
}
