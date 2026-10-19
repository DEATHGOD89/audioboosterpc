#nullable enable
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AudioCinematicBoost
{
    static class Program
    {
        private const string MutexName = "Global\\CinematicAudioBoostApp";
        private static Mutex? _appMutex;

        [STAThread]
        static void Main(string[] args)
        {
            bool hasHandle = false;

            try
            {
                try
                {
                    _appMutex = new Mutex(true, MutexName, out hasHandle);
                }
                catch (AbandonedMutexException)
                {
                    // Previous instance crashed or was terminated abnormally
                    hasHandle = true;
                }

                bool autoEnable = true;

                // Handle command line arguments
                if (args.Length > 0)
                {
                    string arg = args[0].ToLowerInvariant();

                    if (arg == "/stop")
                    {
                        if (!hasHandle)
                        {
                            SendPipeMessage("STOP\n");
                        }
                        else
                        {
                            // App was not running, but reset audio to flat anyway
                            AudioProcessor.ApplyFlat();
                        }
                        return;
                    }
                    else if (arg == "/start")
                    {
                        if (!hasHandle)
                        {
                            // Already running in background: send START command to activate
                            SendPipeMessage("START\n");
                            return;
                        }
                        autoEnable = true;
                    }
                    else
                    {
                        MessageBox.Show(
                            "Usage:\n  /start  - Enable Cinematic Audio (runs in system tray)\n  /stop   - Disable audio boost and exit",
                            "Cinematic Audio Booster",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }
                }

                // If another instance is already running and no specific command was given
                if (!hasHandle)
                {
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TrayAppContext(autoEnable));

                GC.KeepAlive(_appMutex);
            }
            finally
            {
                if (hasHandle && _appMutex != null)
                {
                    try
                    {
                        _appMutex.ReleaseMutex();
                    }
                    catch { }
                }

                _appMutex?.Dispose();
            }
        }

        private static void SendPipeMessage(string message)
        {
            try
            {
                using var pipe = new NamedPipeClientStream(".", "CinematicAudioPipe", PipeDirection.Out);
                pipe.Connect(1500);
                byte[] msg = Encoding.UTF8.GetBytes(message);
                pipe.Write(msg, 0, msg.Length);
                pipe.Flush();
            }
            catch
            {
                // Running instance was not listening or already terminated
            }
        }
    }
}