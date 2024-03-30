using System;
using System.Diagnostics;
using System.Windows;

namespace GPUStatistics
{
    public class ProcessListener : TraceListener
    {
        public override void Write(string? message)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                int index = message.IndexOf("Process");
                if (index != -1)
                {
                    string numberString = message.Substring(index + "Process".Length).Trim();
                    if (int.TryParse(numberString, out int number))
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            MainWindow.main.AsyncBar.Value += number;
                        }));
                    }
                }
            }
        }
    }
}