#nullable enable
using System;
using System.Drawing;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AudioCinematicBoost
{
    public class TrayAppContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly ToolStripMenuItem _toggleMenuItem;
        private bool _isActive;
        private readonly Control _invoker;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Thread? _pipeThread;

        public TrayAppContext(bool autoEnable = true)
        {
            _invoker = new Control();
            _ = _invoker.Handle; // Ensure native window handle is created for BeginInvoke

            // Build the tray icon
            _trayIcon = new NotifyIcon
            {
                Visible = true,
                Text = "Cinematic Audio Booster",
                Icon = AppResources.AppIcon
            };

            // Double click tray icon toggles audio
            _trayIcon.DoubleClick += (s, e) => ToggleAudio();

            // Context Menu
            var menu = new ContextMenuStrip();

            var brandHeaderItem = new ToolStripMenuItem("Audio Cinematic Boost")
            {
                Image = AppResources.LogoImage,
                Font = new Font(menu.Font.FontFamily, 9f, FontStyle.Bold)
            };
            brandHeaderItem.Click += (s, e) => ShowSettings();

            _toggleMenuItem = new ToolStripMenuItem("Enable Cinematic Audio");
            _toggleMenuItem.Font = new Font(menu.Font, FontStyle.Bold);
            _toggleMenuItem.Click += (s, e) => ToggleAudio();

            var settingsMenuItem = new ToolStripMenuItem("Adjust Bass & Volume Boost...");
            settingsMenuItem.Click += (s, e) => ShowSettings();

            var repairMenuItem = new ToolStripMenuItem("🔧 Repair Audio Engine (Post-Windows Update)");
            repairMenuItem.Click += (s, e) => PerformRepair();

            var exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => Exit());

            menu.Items.Add(brandHeaderItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(_toggleMenuItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(settingsMenuItem);
            menu.Items.Add(repairMenuItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitMenuItem);

            _trayIcon.ContextMenuStrip = menu;

            // Check initial state from Equalizer APO config
            _isActive = AudioProcessor.IsCinematicActive();

            // Auto-enable if requested and not yet active
            if (autoEnable && !_isActive)
            {
                if (AudioProcessor.ApplyCinematic())
                {
                    _isActive = true;
                }
            }

            UpdateUi();

            // Start background pipe listener for "/start" and "/stop" IPC commands
            _pipeThread = new Thread(StartPipeServer)
            {
                IsBackground = true,
                Name = "CinematicPipeServer"
            };
            _pipeThread.Start();
        }

        private void ShowSettings()
        {
            using var settingsForm = new SettingsForm(() =>
            {
                if (_isActive)
                {
                    AudioProcessor.ApplyCinematic();
                }
            });
            settingsForm.ShowDialog();
        }

        private void ToggleAudio()
        {
            if (_isActive)
            {
                if (AudioProcessor.ApplyFlat())
                {
                    _isActive = false;
                    ShowBalloon("Cinematic Audio", "Disabled. Audio is now flat bypass.");
                }
                else
                {
                    ShowBalloon("Error", "Failed to write config. Please check Equalizer APO permissions.");
                }
            }
            else
            {
                if (AudioProcessor.ApplyCinematic())
                {
                    _isActive = true;
                    ShowBalloon("Cinematic Audio", "Enabled! Cinema-grade audio profile is active.");
                }
                else
                {
                    ShowBalloon("Error", "Failed to apply preset. Verify Equalizer APO is installed.");
                }
            }
            UpdateUi();
        }

        private void UpdateUi()
        {
            if (_isActive)
            {
                _toggleMenuItem.Text = "✔ Cinematic Audio (ACTIVE)";
                _trayIcon.Text = "Cinematic Audio: ACTIVE";
            }
            else
            {
                _toggleMenuItem.Text = "○ Enable Cinematic Audio";
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
                ShowBalloon("Audio Engine Status", "Equalizer APO Configurator opened. Check your playback devices.");
            }
            else
            {
                MessageBox.Show(
                    $"Repair Diagnostic Output:\n\n{logDetails}",
                    "Audio Engine Diagnostics",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ShowBalloon(string title, string text)
        {
            try
            {
                _trayIcon.ShowBalloonTip(1500, title, text, ToolTipIcon.Info);
            }
            catch { }
        }

        // Listens for IPC commands from secondary CLI instances
        private void StartPipeServer()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream("CinematicAudioPipe", PipeDirection.In);
                    server.WaitForConnection();

                    byte[] buffer = new byte[64];
                    int bytesRead = server.Read(buffer, 0, buffer.Length);
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                    try { server.Disconnect(); } catch { }

                    if (msg == "STOP")
                    {
                        if (_invoker.IsHandleCreated && !_invoker.IsDisposed)
                        {
                            _invoker.BeginInvoke(new Action(() => Exit()));
                        }
                        break;
                    }
                    else if (msg == "START")
                    {
                        if (_invoker.IsHandleCreated && !_invoker.IsDisposed)
                        {
                            _invoker.BeginInvoke(new Action(() =>
                            {
                                if (!_isActive)
                                {
                                    if (AudioProcessor.ApplyCinematic())
                                    {
                                        _isActive = true;
                                        ShowBalloon("Cinematic Audio", "Enabled! Cinema-grade audio profile is active.");
                                        UpdateUi();
                                    }
                                }
                            }));
                        }
                    }
                }
                catch
                {
                    if (_cts.IsCancellationRequested) break;
                    Thread.Sleep(200);
                }
            }
        }

        private void Exit()
        {
            try
            {
                if (_isActive)
                {
                    AudioProcessor.ApplyFlat();
                    _isActive = false;
                }
            }
            catch { }

            try
            {
                _cts.Cancel();
            }
            catch { }

            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _invoker.Dispose();
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { _cts.Cancel(); } catch { }
                _trayIcon?.Dispose();
                _invoker?.Dispose();
                _cts?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}