using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

namespace StatisticalApp.Managing
{
    public class LedUpdate : DispatcherObject
    {
        public void ModifyLedActivity(bool isRunning, PictureBox GreenLight)
        {
            while (isRunning)
            {
                Thread.Sleep(250);

                Dispatcher.Invoke(() =>
                {
                    GreenLight.Visible = true;
                });

                Thread.Sleep(250);

                Dispatcher.Invoke(() =>
                {
                    GreenLight.Visible = false;
                });
            }
        }
    }
}
