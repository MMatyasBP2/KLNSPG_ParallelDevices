using MathNet.Numerics.Statistics;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;

namespace StatisticalApp.Managing
{
    public static class ChartController
    {
        private static CancellationTokenSource cts;
        private readonly static StatisticsController Stat = new StatisticsController();

        public static Chart SetupChartSettings()
        {
            var chart = new Chart();
            chart.ChartAreas.Add(new ChartArea("Default"));

            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Maximum = 100;
            chart.ChartAreas[0].AxisY.Minimum = -50;
            chart.ChartAreas[0].AxisY.Maximum = 50;
            chart.Size = new Size(800, 600);

            chart.Series.Add(new Series());
            chart.Series[0].ChartType = SeriesChartType.Column;

            return chart;
        }

        public static void Charting(int SampleCount, Chart chart)
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();

            cts = new CancellationTokenSource();

            chart.Series[0].ChartType = SeriesChartType.Column;
            var form = GenerateChartingForm(chart);

            Task.Run(() =>
            {
                for (int i = 0; i < SampleCount; i++)
                {
                    if (cts.Token.IsCancellationRequested)
                        break;

                    var samples = Stat.AddSamplesToList();

                    var histogram = new Histogram(samples, 10);

                    double minX = samples.Min();
                    double maxX = samples.Max();
                    double minY = histogram.LowerBound;
                    double maxY = histogram.UpperBound;

                    try
                    {
                        chart.Invoke(new Action(() =>
                        {
                            chart.Series[0].Points.Clear();

                            chart.ChartAreas[0].AxisX.Minimum = minX;
                            chart.ChartAreas[0].AxisX.Maximum = maxX;
                            chart.ChartAreas[0].AxisY.Minimum = minY;
                            chart.ChartAreas[0].AxisY.Maximum = maxY;

                            for (int j = 0; j < histogram.BucketCount; j++)
                            {
                                chart.Series[0].Points.AddXY(histogram[j].LowerBound, histogram[j].Count);
                            }

                            chart.Invalidate();
                        }));
                    }
                    catch (InvalidOperationException)
                    {
                        MessageBox.Show("Please restart sampling!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Thread.Sleep(50);
                }
            }, cts.Token);

            form.FormClosing += (s, ev) => cts.Cancel();

            form.ShowDialog();
        }

        private static Form GenerateChartingForm(Chart chart)
        {
            var form = new Form();
            form.Size = new Size(900, 700);
            form.ShowIcon = false;
            form.Controls.Add(chart);
            return form;
        }
    }
}
