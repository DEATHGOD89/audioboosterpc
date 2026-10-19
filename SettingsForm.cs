#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AudioCinematicBoost
{
    public class SettingsForm : Form
    {
        private readonly TrackBar _bassTrackBar;
        private readonly TrackBar _volumeTrackBar;
        private readonly Label _bassLabel;
        private readonly Label _volumeLabel;
        private readonly Label _statusLabel;
        private readonly System.Windows.Forms.Timer _debounceTimer;
        private readonly Action _onSettingsChanged;

        public SettingsForm(Action onSettingsChanged)
        {
            _onSettingsChanged = onSettingsChanged;

            // Form properties
            this.Text = "Cinematic Bass & PC Volume Booster";
            this.Icon = AppResources.AppIcon;
            this.ShowIcon = true;
            this.Size = new Size(360, 415);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 24, 24);
            this.ForeColor = Color.White;

            // Debounce timer for smooth slider dragging (75ms)
            _debounceTimer = new System.Windows.Forms.Timer { Interval = 75 };
            _debounceTimer.Tick += (s, e) =>
            {
                _debounceTimer.Stop();
                _onSettingsChanged?.Invoke();
            };

            // Top Branded Header with Logo
            var logoPic = new PictureBox
            {
                Image = AppResources.LogoImage,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(18, 14),
                Size = new Size(54, 54),
                BackColor = Color.Transparent
            };
            this.Controls.Add(logoPic);

            var appTitle = new Label
            {
                Text = "AudioCinematicBoost",
                Location = new Point(80, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(appTitle);

            var appSubtitle = new Label
            {
                Text = "Cinema Bass & PC Volume Booster",
                Location = new Point(81, 37),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25f, FontStyle.Regular),
                ForeColor = Color.FromArgb(0, 220, 255)
            };
            this.Controls.Add(appSubtitle);

            var appEngine = new Label
            {
                Text = "Real-Time 32-Bit Equalizer APO Engine",
                Location = new Point(81, 53),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(145, 145, 145)
            };
            this.Controls.Add(appEngine);

            var headerDivider = new Panel
            {
                Location = new Point(18, 76),
                Size = new Size(308, 1),
                BackColor = Color.FromArgb(45, 45, 45)
            };
            this.Controls.Add(headerDivider);

            // 1. Bass Controls (Unchanged logic)
            _bassLabel = new Label
            {
                Text = $"Sub & Punch Bass: {(int)(AudioProcessor.BassMultiplier * 100)}%",
                Location = new Point(20, 88),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 255)
            };
            this.Controls.Add(_bassLabel);

            _bassTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 200,
                Value = Math.Max(0, Math.Min(200, (int)(AudioProcessor.BassMultiplier * 100))),
                TickFrequency = 20,
                Location = new Point(18, 112),
                Width = 308
            };
            _bassTrackBar.Scroll += (s, e) => 
            {
                _bassLabel.Text = $"Sub & Punch Bass: {_bassTrackBar.Value}%";
                AudioProcessor.BassMultiplier = _bassTrackBar.Value / 100f;
                RestartDebounce();
            };
            this.Controls.Add(_bassTrackBar);

            // 2. PC Audio Booster (Replaces Vocal Clarity - boosts volume above 100% for quiet movies & games)
            int initialVol = Math.Max(100, Math.Min(300, (int)(AudioProcessor.VolumeMultiplier * 100)));
            float initialBoostDb = (initialVol - 100) / 100f * 10.0f;
            string initialVolText = initialVol > 100 ? $" (+{initialBoostDb:0.0} dB Boost)" : " (Normal)";

            _volumeLabel = new Label
            {
                Text = $"🔊 PC Audio Booster: {initialVol}%{initialVolText}",
                Location = new Point(20, 166),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 50)
            };
            this.Controls.Add(_volumeLabel);

            _volumeTrackBar = new TrackBar
            {
                Minimum = 100,
                Maximum = 300,
                Value = initialVol,
                TickFrequency = 20,
                Location = new Point(18, 190),
                Width = 308
            };
            _volumeTrackBar.Scroll += (s, e) => 
            {
                int val = _volumeTrackBar.Value;
                float boostDb = (val - 100) / 100f * 10.0f;
                string boostText = val > 100 ? $" (+{boostDb:0.0} dB Boost)" : " (Normal)";
                _volumeLabel.Text = $"🔊 PC Audio Booster: {val}%{boostText}";
                AudioProcessor.VolumeMultiplier = val / 100f;
                RestartDebounce();
            };
            this.Controls.Add(_volumeTrackBar);

            // 3. Reset Button
            var resetBtn = new Button
            {
                Text = "↺ Reset to Default (100%)",
                Location = new Point(20, 242),
                Size = new Size(304, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            resetBtn.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            resetBtn.Click += (s, e) =>
            {
                _bassTrackBar.Value = 100;
                _volumeTrackBar.Value = 100;
                AudioProcessor.BassMultiplier = 1.0f;
                AudioProcessor.VolumeMultiplier = 1.0f;
                _bassLabel.Text = "Sub & Punch Bass: 100%";
                _volumeLabel.Text = "🔊 PC Audio Booster: 100% (Normal)";
                _debounceTimer.Stop();
                _onSettingsChanged?.Invoke();
            };
            this.Controls.Add(resetBtn);

            // 4. Audio Engine Diagnostic Status
            var audit = AudioEngineHealer.PerformHealthAudit();
            _statusLabel = new Label
            {
                Text = audit.StatusMessage,
                Location = new Point(20, 278),
                Size = new Size(304, 32),
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = audit.IsFullyHooked ? Color.FromArgb(100, 255, 120) : Color.FromArgb(255, 160, 60)
            };
            this.Controls.Add(_statusLabel);

            // 5. Repair Button
            var repairBtn = new Button
            {
                Text = "🔧 Repair Audio Engine (Post-Windows Update)",
                Location = new Point(20, 316),
                Size = new Size(304, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(32, 75, 135),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            repairBtn.FlatAppearance.BorderSize = 0;
            repairBtn.Click += (s, e) =>
            {
                repairBtn.Enabled = false;
                repairBtn.Text = "Checking...";
                Application.DoEvents();

                bool ok = AudioEngineHealer.RepairAudioEngine(out string logDetails);
                var newAudit = AudioEngineHealer.PerformHealthAudit();
                _statusLabel.Text = newAudit.StatusMessage;
                _statusLabel.ForeColor = newAudit.IsFullyHooked ? Color.FromArgb(100, 255, 120) : Color.FromArgb(255, 160, 60);

                repairBtn.Enabled = true;
                repairBtn.Text = "🔧 Repair Audio Engine (Post-Windows Update)";

                if (ok)
                {
                    MessageBox.Show(
                        "Audio Engine Configurator launched!\n\nEnsure your playback device (Speakers/Headphones) is selected and click OK.",
                        "Audio Engine Repair", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _onSettingsChanged?.Invoke();
                }
                else
                {
                    MessageBox.Show($"Diagnostic result:\n\n{logDetails}", "Repair Result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            this.Controls.Add(repairBtn);
        }

        private void RestartDebounce()
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _debounceTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
