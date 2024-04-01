using GPUStatistics.CPUHandling;
using GPUStatistics.GPUHandling;
using MathNet.Numerics.Distributions;
using System.Diagnostics;
using System.Windows;

namespace GPUStatistics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsInSequence { get; set; }
        public static MainWindow main;
        public GPUHandler GPUHandler { get; set; }
        public CPUCalculations CPUCalculations { get; set; }

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                GPUHandler = new GPUHandler(256);
                CPUCalculations = new CPUCalculations();
                main = this;

                HideComponents();
                Trace.Listeners.Add(new ProcessListener());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsInSequence)
                    return;

                IsInSequence = true;
                AsyncBar.Visibility = Visibility.Visible;
                AsyncBar.Value = 1;
                ResultBox.Document.Blocks.Clear();

                long arraySize = (long)SizeSlider.Value;
                float[] array = new float[arraySize];
                float sum = 0;

                Normal normalDistribution = new Normal();
                for (int i = 0; i < arraySize; i++)
                {
                    array[i] = (float)normalDistribution.Sample();
                    sum += array[i];
                }

                Task<(float, double)> cpuSumTask = Task.Run(() => CPUCalculations.CalculateSum(array));

                EnableComponents();

                (float cpuSumResult, double cpuSumTime) = await cpuSumTask;
                ResultBox.AppendText($"Sum (CPU): {cpuSumResult}\n" +
                                      $"Timespan (CPU): {cpuSumTime}\n\n");

                Task<(float, double)> gpuSumTask = Task.Run(() => GPUHandler.CalculateSum(array));
                (float gpuSumResult, double gpuSumTime) = await gpuSumTask;
                ResultBox.AppendText($"Sum (GPU): {gpuSumResult}\n" +
                                      $"Timespan (GPU): {gpuSumTime}");

                Task<(float, double)> cpuAvgTask = Task.Run(() => CPUCalculations.CalculateAverage(array));
                (float cpuAvgResult, double cpuAvgTime) = await cpuAvgTask;
                ResultBox.AppendText($"\nAverage (CPU): {cpuAvgResult}\n" +
                                      $"Timespan (CPU): {cpuAvgTime}\n");

                Task<(float, double)> gpuAvgTask = Task.Run(() => GPUHandler.CalculateAverage(array));
                (float gpuAvgResult, double gpAvgTime) = await gpuAvgTask;
                ResultBox.AppendText($"Average (GPU): {gpuAvgResult}\n" +
                                      $"Timespan (GPU): {gpAvgTime}");

                Task<(float, double)> cpuMinTask = Task.Run(() => CPUCalculations.CalculateMin(array));
                (float cpuMinResult, double cpuMinTime) = await cpuMinTask;
                ResultBox.AppendText($"\nMinimum (CPU): {cpuMinResult}\n" +
                                      $"Timespan (CPU): {cpuMinTime}\n");

                Task<(float, double)> gpuMinTask = Task.Run(() => GPUHandler.CalculateMin(array));
                (float gpuMinResult, double gpuMinTime) = await gpuMinTask;
                ResultBox.AppendText($"Minimum (GPU): {gpuMinResult}\n" +
                                      $"Timespan (GPU): {gpuMinTime}");

                Task<(float, double)> cpuMaxTask = Task.Run(() => CPUCalculations.CalculateMax(array));
                (float cpuMaxResult, double cpuMaxTime) = await cpuMaxTask;
                ResultBox.AppendText($"\nMaximum (CPU): {cpuMaxResult}\n" +
                                      $"Timespan (CPU): {cpuMaxTime}\n");

                Task<(float, double)> gpuMaxTask = Task.Run(() => GPUHandler.CalculateMax(array));
                (float gpuMaxResult, double gpuMaxTime) = await gpuMaxTask;
                ResultBox.AppendText($"Maximum (GPU): {gpuMaxResult}\n" +
                                      $"Timespan (GPU): {gpuMaxTime}");

                Task<(float, double)> cpuMedianTask = Task.Run(() => CPUCalculations.CalculateMedian(array));
                (float cpuMedianResult, double cpuMedianTime) = await cpuMedianTask;
                ResultBox.AppendText($"\nMedian (CPU): {cpuMedianResult}\n" +
                                      $"Timespan (CPU): {cpuMedianTime}\n");

                Task<(float, double)> gpuMedianTask = Task.Run(() => GPUHandler.CalculateMedian(array));
                (float gpuMedianResult, double gpuMedianTime) = await gpuMedianTask;
                ResultBox.AppendText($"Median (GPU): {gpuMedianResult}\n" +
                                      $"Timespan (GPU): {gpuMedianTime}");

                IsInSequence = false;
                AsyncBar.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                IsInSequence = false;
                AsyncBar.Visibility = Visibility.Collapsed;
                ResultBox.Document.Blocks.Clear();
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HideComponents()
        {
            CPUResultLabel.Visibility = Visibility.Collapsed;
            GPUResultLabel.Visibility = Visibility.Collapsed;
            SumLabel.Visibility = Visibility.Collapsed;
            AvgLabel.Visibility = Visibility.Collapsed;
            MinLabel.Visibility = Visibility.Collapsed;
            MaxLabel.Visibility = Visibility.Collapsed;
            MedianLabel.Visibility = Visibility.Collapsed;
            CPUSumResultBox.Visibility = Visibility.Collapsed;
            GPUSumResultBox.Visibility = Visibility.Collapsed;
            CPUAvgResultBox.Visibility = Visibility.Collapsed;
            GPUAvgResultBox.Visibility = Visibility.Collapsed;
            CPUMinResultBox.Visibility = Visibility.Collapsed;
            GPUMinResultBox.Visibility = Visibility.Collapsed;
            CPUMaxResultBox.Visibility = Visibility.Collapsed;
            GPUMaxResultBox.Visibility = Visibility.Collapsed;
            CPUMedianResultBox.Visibility = Visibility.Collapsed;
            GPUMedianResultBox.Visibility = Visibility.Collapsed;

            CPUSumTimeBox.Visibility = Visibility.Collapsed;
            GPUSumTimeBox.Visibility = Visibility.Collapsed;
            CPUAvgTimeBox.Visibility = Visibility.Collapsed;
            GPUAvgTimeBox.Visibility = Visibility.Collapsed;
            CPUMinTimeBox.Visibility = Visibility.Collapsed;
            GPUMinTimeBox.Visibility = Visibility.Collapsed;
            CPUMaxTimeBox.Visibility = Visibility.Collapsed;
            GPUMaxTimeBox.Visibility = Visibility.Collapsed;
            CPUMedianTimeBox.Visibility = Visibility.Collapsed;
            GPUMedianTimeBox.Visibility = Visibility.Collapsed;
            AsyncBar.Visibility = Visibility.Collapsed;
        }

        private void EnableComponents()
        {
            CPUResultLabel.Visibility = Visibility.Visible;
            GPUResultLabel.Visibility = Visibility.Visible;
            SumLabel.Visibility = Visibility.Visible;
            AvgLabel.Visibility = Visibility.Visible;
            MinLabel.Visibility = Visibility.Visible;
            MaxLabel.Visibility = Visibility.Visible;
            MedianLabel.Visibility = Visibility.Visible;
            CPUSumResultBox.Visibility = Visibility.Visible;
            GPUSumResultBox.Visibility = Visibility.Visible;
            CPUAvgResultBox.Visibility = Visibility.Visible;
            GPUAvgResultBox.Visibility = Visibility.Visible;
            CPUMinResultBox.Visibility = Visibility.Visible;
            GPUMinResultBox.Visibility = Visibility.Visible;
            CPUMaxResultBox.Visibility = Visibility.Visible;
            GPUMaxResultBox.Visibility = Visibility.Visible;
            CPUMedianResultBox.Visibility = Visibility.Visible;
            GPUMedianResultBox.Visibility = Visibility.Visible;

            CPUSumTimeBox.Visibility = Visibility.Visible;
            GPUSumTimeBox.Visibility = Visibility.Visible;
            CPUAvgTimeBox.Visibility = Visibility.Visible;
            GPUAvgTimeBox.Visibility = Visibility.Visible;
            CPUMinTimeBox.Visibility = Visibility.Visible;
            GPUMinTimeBox.Visibility = Visibility.Visible;
            CPUMaxTimeBox.Visibility = Visibility.Visible;
            GPUMaxTimeBox.Visibility = Visibility.Visible;
            CPUMedianTimeBox.Visibility = Visibility.Visible;
            GPUMedianTimeBox.Visibility = Visibility.Visible;
            AsyncBar.Visibility = Visibility.Visible;
        }
    }
}