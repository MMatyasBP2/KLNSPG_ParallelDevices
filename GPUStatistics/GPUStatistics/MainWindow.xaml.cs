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
                AsyncBar.Visibility = Visibility.Collapsed;
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
    }
}