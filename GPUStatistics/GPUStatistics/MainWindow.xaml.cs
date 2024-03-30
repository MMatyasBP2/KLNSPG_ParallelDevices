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

                await Task.Run(() => {
                    (float, double) CPUSum = CPUCalculations.CalculateSum(array);
                    (float, double) GPUSum = CPUCalculations.CalculateSum(array);


                    (float cpuSumResult, double cpuSumTime) = CPUSum;
                    (float gpuSumResult, double gpuSumTime) = GPUSum;

                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        ResultBox.AppendText($"Sum (CPU): {cpuSumResult}\n" +
                                         $"Timespan (CPU): {cpuSumTime}\n");
                        ResultBox.AppendText($"\nSum (GPU): {gpuSumResult}\n" +
                                             $"Timespan (GPU): {gpuSumTime}");
                    }));
                });

                IsInSequence = false;
                AsyncBar.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                IsInSequence = false;
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}