using MathNet.Numerics.Distributions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Cloo;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;

namespace StatisticalApp.Managing
{
    public class StatisticsController
    {
        public int SampleCount { get; set; }
        public double Min { get; set; }
        public Normal Normal = Normal.WithMeanStdDev(0, 1);
        public CancellationTokenSource cts;
        private readonly object FileLocker = new object();
        public List<double> Samples = new List<double>();

        private ComputeContext context;
        private ComputeProgram program;
        private ComputeKernel kernel;
        private ComputeCommandQueue queue;

        public StatisticsController()
        {
            JObject jConfig = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("appconfig.json"));
            SampleCount = Convert.ToInt32(jConfig["MeasurementSettings"]["SampleCount"]);
        }

        public void InitOpenCL()
        {
            ComputePlatform platform = ComputePlatform.Platforms[0];
            context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(platform), null, IntPtr.Zero);

            string clSource = File.ReadAllText("calculations.cl");
            program = new ComputeProgram(context, clSource);

            try
            {
                program.Build(null, null, null, IntPtr.Zero);
            }
            catch
            {
                Console.WriteLine("Error building program: " + program.GetBuildLog(context.Devices[0]));
                throw;
            }

            kernel = program.CreateKernel("calc_min_reduced");
            queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
        }

        #region Sampling
        public List<double> AddSamplesToList() => Enumerable.Range(0, SampleCount).Select(_ => Normal.Sample()).ToList();

        private void FillResultDictionary(int iterate, IDictionary<string, string> Results)
        {
            Results["Sample"] = $"{iterate}";
            Results["Minimum"] = $"{Min:F4}";
        }

        public async Task Sampling(Chart chart, 
                                   RichTextBox SampleNameBox, 
                                   RichTextBox SampleValueBox, 
                                   IDictionary<string, string> Results)
        {
            cts = new CancellationTokenSource();

            for (int i = 0; i < SampleCount; i++)
            {
                Samples = AddSamplesToList();
                
                Min = CalcMin(Samples);

                FillResultDictionary(i + 1, Results);

                SampleNameBox.Invoke(new Action(() => SampleNameBox.Text = string.Join(Environment.NewLine, Results.Select(kv => $"{kv.Key}:"))));
                SampleValueBox.Invoke(new Action(() => SampleValueBox.Text = string.Join(Environment.NewLine, Results.Select(kv => $"{kv.Value}"))));

                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "results.txt");
                lock (FileLocker)
                {
                    using (var writer = new StreamWriter(filePath, false))
                    {
                        writer.WriteLine($"Results from {i + 1} measurement from {SampleCount} samples:\n");
                        writer.WriteLine(string.Join(Environment.NewLine, Results.Select(kv => $"{kv.Key}: {kv.Value}")));
                    }
                }

                await Task.Delay(50);

                if (cts.Token.IsCancellationRequested)
                    break;
            }
        }

        public void CancelSampling() => cts.Cancel();
        #endregion

        #region Calculations

        private double CalcMin(List<double> samples)
        {
            int samplesCount = samples.Count;
            float[] samplesArray = samples.Select(Convert.ToSingle).ToArray();

            long maxWorkGroupSize = context.Devices[0].MaxWorkGroupSize;
            int localSize = (int)Math.Min(1024, maxWorkGroupSize);
            int globalSize = CalculateGlobalWorkSize(samplesCount, localSize);

            using (ComputeBuffer<float> inputBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, samplesArray))
            using (ComputeBuffer<float> outputBuffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly, 1))
            {
                kernel.SetMemoryArgument(0, inputBuffer);
                kernel.SetMemoryArgument(1, outputBuffer);
                kernel.SetValueArgument(2, samplesCount);
                kernel.SetLocalArgument(3, localSize * sizeof(float));

                queue.Execute(kernel, null, new long[] { globalSize }, new long[] { localSize }, null);

                float[] minResult = new float[1];
                queue.ReadFromBuffer(outputBuffer, ref minResult, true, null);

                return minResult[0];
            }
        }

        private int CalculateGlobalWorkSize(int dataSize, int localSize)
        {
            int r = dataSize % localSize;
            return r == 0 ? dataSize : dataSize + localSize - r;
        }

        #endregion 
    }
}
