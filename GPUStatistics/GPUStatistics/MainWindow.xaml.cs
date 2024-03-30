using Cloo;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace GPUStatistics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ComputeContext Context { get; set; }
        public ComputeProgram Program { get; set; }
        public int WorkGroupSize { get; set; }
        public int NumberOfGroups { get; set; }
        public bool IsInSequence { get; set; }
        public static MainWindow main;

        public MainWindow()
        {
            InitializeComponent();
            InitOpenCL();
            main = this;
            AsyncBar.Visibility = Visibility.Collapsed;
            Trace.Listeners.Add(new ProcessListener());
        }

        private void InitOpenCL()
        {
            try
            {
                ComputePlatform platform = ComputePlatform.Platforms[0];
                Context = new ComputeContext(ComputeDeviceTypes.Gpu, new ComputeContextPropertyList(platform), null, IntPtr.Zero);

                string kernelSource = File.ReadAllText("./Kernelfile/calculations.cl");
                Program = new ComputeProgram(Context, kernelSource);

                Program.Build(null, null, null, IntPtr.Zero);
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
                    MessageBox.Show($"Build log saved to: {logFileName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
                else
                {
                    MessageBox.Show($"Initialization failed, unable to retrieve build log. Exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
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

                await Task.Run(() => {

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