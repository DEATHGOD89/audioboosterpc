using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace AudioCinematicBoost
{
    public class TrayAppContext : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ToolStripMenuItem _toggleMenuItem;
        private bool _isActive;
        private Thread? _pipeThread;
        private Form _hiddenForm;

        public TrayAppContext()
        {
            _hiddenForm = new Form();
            var _ = _hiddenForm.Handle; // Force handle creation for Invoke

            // Build the tray icon
            _trayIcon = new NotifyIcon
            {
                Visible = true,
                Text = "Cinematic Audio Booster"
            };

            // Use default icon if you don't have custom ones
            _trayIcon.Icon = SystemIcons.Application;

            // Context Menu
            var menu = new ContextMenuStrip();
            _toggleMenuItem = new ToolStripMenuItem("Enable Cinematic Audio");
            _toggleMenuItem.Click += (s, e) => ToggleAudio();

            var settingsMenuItem = new ToolStripMenuItem("Adjust EQ Settings...");
            settingsMenuItem.Click += (s, e) => ShowSettings();

            var repairMenuItem = new ToolStripMenuItem("🔧 Repair Audio Engine (Post-Windows Update)");
            repairMenuItem.Click += (s, e) => PerformRepair();

            menu.Items.Add(_toggleMenuItem);
            menu.Items.Add(settingsMenuItem);
            menu.Items.Add(repairMenuItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, e) => Exit());

            _trayIcon.ContextMenuStrip = menu;

            // Check initial state
            _isActive = AudioProcessor.IsCinematicActive();
            UpdateUi();

            // Start background pipe listener for "/stop" command
            _pipeThread = new Thread(StartPipeServer);
            _pipeThread.IsBackground = true;
            _pipeThread.Start();
        }

        private void ShowSettings()
        {
            using (var settingsForm = new SettingsForm(() => 
            {
                // Action to run when settings change
                if (_isActive)
                {
                    // Re-apply immediately if currently active
                    AudioProcessor.ApplyCinematic();
                }
            }))
            {
                settingsForm.ShowDialog();
            }
        }

        private void ToggleAudio()
        {
            if (_isActive)
            {
                // Disable: go flat
                if (AudioProcessor.ApplyFlat())
                {
                    _isActive = false;
                    ShowBalloon("Cinematic Audio", "Disabled. Audio is now flat.");
                }
                else
                {
                    ShowBalloon("Error", "Failed to disable. Run as Administrator.");
                }
            }
            else
            {
                // Enable: apply cinematic
                if (AudioProcessor.ApplyCinematic())
                {
                    _isActive = true;
                    ShowBalloon("Cinematic Audio", "Enabled! Your perfect SPOTY settings are active.");
                }
                else
                {
                    ShowBalloon("Error", "Failed to enable. Is EqualizerAPO installed?");
                }
            }
            UpdateUi();
        }

        private void UpdateUi()
        {
            if (_isActive)
            {
                _toggleMenuItem.Text = "✔ Cinematic Audio (ON)";
                _trayIcon.Text = "Cinematic Audio: ENABLED";
            }
            else
            {
                _toggleMenuItem.Text = " Cinematic Audio (OFF)";
                _trayIcon.Text = "Cinematic Audio: DISABLED";
            }
        }

        private void PerformRepair()
        {
            bool success = AudioEngineHealer.RepairAudioEngine(out string logDetails);

            if (success)
            {
                _isActive = AudioProcessor.ApplyCinematic();
                UpdateUi();
                ShowBalloon("Audio Engine Status", "Configurator opened. Select your audio device if required.");
            }
            else
            {
                MessageBox.Show(
                    $"Repair Diagnostic Output:\n\n{logDetails}",
                    "Repair Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ShowBalloon(string title, string text)
        {
            _trayIcon.ShowBalloonTip(1500, title, text, ToolTipIcon.Info);
        }

        // 🛑 Listens for "/stop" command from a second instance
        private void StartPipeServer()
        {
            while (true)
            {
                try
                {
                    using var server = new NamedPipeServerStream("CinematicAudioPipe", PipeDirection.In);
                    server.WaitForConnection();
                    byte[] buffer = new byte[256];
                    int bytesRead = server.Read(buffer, 0, buffer.Length);
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (msg == "STOP")
                    {
                        // Gracefully disable and exit on UI thread
                        if (_hiddenForm.IsHandleCreated && !_hiddenForm.IsDisposed)
                        {
                            _hiddenForm.Invoke(new Action(() =>
                            {
                                if (_isActive)
                                {
                                    AudioProcessor.ApplyFlat();
                                    _isActive = false;
                                    UpdateUi();
                                }
                                Exit();
                            }));
                        }
                        break;
                    }
                }
                catch { break; }
            }
        }

        private void Exit()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _hiddenForm?.Dispose();
            Application.Exit();
        }
    }
}