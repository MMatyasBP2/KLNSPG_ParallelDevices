using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MathNet.Numerics.Statistics;
using StatisticalApp.Managing;

namespace StatisticalApp
{
    public partial class MainWindow : Form
    {
        private readonly IDictionary<string, string> Results;
        private readonly StatisticsController Statistics;
        private Chart Chart;
        private Thread lightingThread;
        private volatile bool isRunning;
        private readonly LedUpdate LedUpdate;

        public MainWindow()
        {
            InitializeComponent();
            Statistics = new StatisticsController();
            Statistics.InitOpenCL();
            Results = new Dictionary<string, string>();
            Chart = new Chart();
            isRunning = false;
            LedUpdate = new LedUpdate();
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            StatLabel.Visible = false;
            StatNameBox.Visible = false;
            StatValueBox.Visible = false;
            ResultButton.Visible = false;
            ParallelResultBox.Clear();
            ParallelResultBox.Visible = false;
            Stopwatch sw = null;

            Results.Clear();
            SampleNameBox.Clear();
            StatNameBox.Clear();

            try
            {
                Chart = ChartController.SetupChartSettings();

                isRunning = true;
                
                sw = new Stopwatch();
                sw.Start();

                lightingThread = new Thread(Lighting);
                lightingThread.IsBackground = true;
                lightingThread.Start();

                var chartData = Chart.Series[0].Points.ToList();

                await Task.Run(() => Statistics.Sampling(Chart, SampleNameBox, SampleValueBox, Results));

                await Task.Run(() =>
                {
                    var samples = Results.Select(kv => kv.Value).ToList();
                    var histogram = new Histogram(Statistics.Samples, 10);

                    BeginInvoke(new Action(() =>
                    {
                        Chart.Series[0].Points.Clear();

                        for (int j = 0; j < histogram.BucketCount; j++)
                        {
                            Chart.Series[0].Points.AddXY(histogram[j].LowerBound, histogram[j].Count);
                        }
                        Chart.Invalidate();
                    }));
                });

                lightingThread.Abort();
                GreenLight.Visible = false;

                sw.Stop();

                Chart.Series[0].Points.Clear();
                Chart.Series[0].Points.Clear();
                foreach (var dataPoint in chartData)
                {
                    Chart.Series[0].Points.Add(dataPoint);
                }
            }
            catch (OperationCanceledException)
            {
            }

            StatLabel.Visible = true;
            StatNameBox.Visible = true;
            StatValueBox.Visible = true;
            ResultButton.Visible = true;
            ParallelResultBox.Visible= true;

            StatNameBox.Text = string.Join(Environment.NewLine, Results.Select(kv => $"{kv.Key}:"));
            StatValueBox.Text = string.Join(Environment.NewLine, Results.Select(kv => $"{kv.Value}"));
            ParallelResultBox.AppendText($"Sampling completed in {sw.ElapsedMilliseconds} ms with multiple threads.\n\n");
            ParallelResultBox.AppendText($"Sampling would have completed in {sw.ElapsedMilliseconds * Environment.ProcessorCount} ms with one thread.");
        }


        private void Lighting() => LedUpdate.ModifyLedActivity(isRunning, GreenLight);

        private void StopButton_Click(object sender, EventArgs e) => Statistics.CancelSampling();

        private void PlotButton_Click(object sender, EventArgs e)
        {
            if (Statistics.cts == null || Statistics.cts.IsCancellationRequested)
            {
                MessageBox.Show("Please start sampling before plotting the normal distribution!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ChartController.Charting(Statistics.SampleCount, Chart);
        }

        private void ConfigButton_Click(object sender, EventArgs e) => Process.Start("appconfig.json");

        private void ResultButton_Click(object sender, EventArgs e) => Process.Start("results.txt");
    }
}