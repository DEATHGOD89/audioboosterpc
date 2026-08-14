using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AudioCinematicBoost
{
    static class Program
    {
        private static Mutex? _appMutex;
        private const string MutexName = "Global\\CinematicAudioBoostApp";

        [STAThread]
        static void Main(string[] args)
        {
            bool createdNew;
            _appMutex = new Mutex(true, MutexName, out createdNew);

            // Handle command line arguments
            if (args.Length > 0)
            {
                string arg = args[0].ToLowerInvariant();

                if (arg == "/stop")
                {
                    // Try to send stop signal to running tray app
                    if (!createdNew)
                    {
                        SendStopSignal();
                    }
                    return; // Exit immediately
                }
                else if (arg == "/start")
                {
                    // If already running, just bring it to front (or do nothing)
                    if (!createdNew)
                        return;
                    // else continue to run the tray
                }
                else
                {
                    MessageBox.Show(
                        "Usage:\n  /start  - Enable Cinematic Audio (runs in tray)\n  /stop   - Disable and exit",
                        "Cinematic Audio Booster",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }
            }

            // If no args OR "/start", and we are the first instance -> run tray
            if (!createdNew)
                return; // Already running, silently exit

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayAppContext());

            _appMutex?.ReleaseMutex();
        }

        private static void SendStopSignal()
        {
            try
            {
                using (var pipe = new NamedPipeClientStream(".", "CinematicAudioPipe", PipeDirection.Out))
                {
                    pipe.Connect(1000);
                    byte[] msg = Encoding.UTF8.GetBytes("STOP");
                    pipe.Write(msg, 0, msg.Length);
                }
            }
            catch
            {
                // Server not running, ignore
            }
        }
    }
}