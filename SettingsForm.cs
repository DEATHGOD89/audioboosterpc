using System;
using System.Drawing;
using System.Windows.Forms;

namespace AudioCinematicBoost
{
    public class SettingsForm : Form
    {
        private TrackBar _bassTrackBar;
        private TrackBar _clarityTrackBar;
        private Label _bassLabel;
        private Label _clarityLabel;

        private CheckBox _micCheckBox;
        private Label _micLabel;
        private TrackBar _micTrackBar;

        private Action _onSettingsChanged;

        public SettingsForm(Action onSettingsChanged)
        {
            _onSettingsChanged = onSettingsChanged;

            this.Text = "Cinematic Audio & Mic Settings";
            this.Size = new Size(350, 440);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ForeColor = Color.White;

            // Bass Label
            _bassLabel = new Label
            {
                Text = $"Bass Intensity: {(int)(AudioProcessor.BassMultiplier * 100)}%",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(_bassLabel);

            // Bass TrackBar
            _bassTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 200,
                Value = (int)(AudioProcessor.BassMultiplier * 100),
                TickFrequency = 10,
                Location = new Point(20, 45),
                Width = 290
            };
            _bassTrackBar.Scroll += (s, e) => 
            {
                _bassLabel.Text = $"Bass Intensity: {_bassTrackBar.Value}%";
                AudioProcessor.BassMultiplier = _bassTrackBar.Value / 100f;
                _onSettingsChanged?.Invoke();
            };
            this.Controls.Add(_bassTrackBar);

            // Clarity Label
            _clarityLabel = new Label
            {
                Text = $"Sound Clarity: {(int)(AudioProcessor.ClarityMultiplier * 100)}%",
                Location = new Point(20, 100),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(_clarityLabel);

            // Clarity TrackBar
            _clarityTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 200,
                Value = (int)(AudioProcessor.ClarityMultiplier * 100),
                TickFrequency = 10,
                Location = new Point(20, 125),
                Width = 290
            };
            _clarityTrackBar.Scroll += (s, e) => 
            {
                _clarityLabel.Text = $"Sound Clarity: {_clarityTrackBar.Value}%";
                AudioProcessor.ClarityMultiplier = _clarityTrackBar.Value / 100f;
                _onSettingsChanged?.Invoke();
            };
            this.Controls.Add(_clarityTrackBar);

            // --- Separator Panel ---
            var separator = new Panel
            {
                Location = new Point(20, 175),
                Size = new Size(290, 1),
                BackColor = Color.FromArgb(60, 60, 60)
            };
            this.Controls.Add(separator);

            // --- Microphone Controls ---
            _micCheckBox = new CheckBox
            {
                Text = "Enable Studio Mic Boost & Air",
                Checked = AudioProcessor.EnableMicBoost,
                Location = new Point(20, 190),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 255)
            };
            _micCheckBox.CheckedChanged += (s, e) =>
            {
                AudioProcessor.EnableMicBoost = _micCheckBox.Checked;
                _micTrackBar.Enabled = _micCheckBox.Checked;
                _micLabel.Enabled = _micCheckBox.Checked;
                _onSettingsChanged?.Invoke();
            };
            this.Controls.Add(_micCheckBox);

            // Mic Label
            _micLabel = new Label
            {
                Text = $"Mic Gain & Sensitivity: {(int)(AudioProcessor.MicMultiplier * 100)}% (+{(int)(12 * AudioProcessor.MicMultiplier)} dB)",
                Location = new Point(20, 225),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Enabled = AudioProcessor.EnableMicBoost
            };
            this.Controls.Add(_micLabel);

            // Mic TrackBar
            _micTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 200,
                Value = (int)(AudioProcessor.MicMultiplier * 100),
                TickFrequency = 10,
                Location = new Point(20, 250),
                Width = 290,
                Enabled = AudioProcessor.EnableMicBoost
            };
            _micTrackBar.Scroll += (s, e) =>
            {
                float mult = _micTrackBar.Value / 100f;
                _micLabel.Text = $"Mic Gain & Sensitivity: {_micTrackBar.Value}% (+{(int)(12 * mult)} dB)";
                AudioProcessor.MicMultiplier = mult;
                _onSettingsChanged?.Invoke();
            };
            this.Controls.Add(_micTrackBar);

            // --- Health Diagnostic Status ---
            var audit = AudioEngineHealer.PerformHealthAudit();
            var statusLabel = new Label
            {
                Text = audit.StatusMessage,
                Location = new Point(20, 295),
                Size = new Size(290, 35),
                Font = new Font("Segoe UI", 8.5f, audit.IsFullyHooked ? FontStyle.Regular : FontStyle.Bold),
                ForeColor = audit.IsFullyHooked ? Color.LightGreen : Color.Orange
            };
            this.Controls.Add(statusLabel);

            // Repair Button
            var repairBtn = new Button
            {
                Text = "🔧 Repair Audio Engine (Post-Windows Update)",
                Location = new Point(20, 340),
                Size = new Size(290, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 80, 140),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            repairBtn.FlatAppearance.BorderSize = 0;
            repairBtn.Click += (s, e) =>
            {
                repairBtn.Enabled = false;
                repairBtn.Text = "Repairing...";
                Application.DoEvents();

                bool ok = AudioEngineHealer.RepairAudioEngine(out string logDetails);
                var newAudit = AudioEngineHealer.PerformHealthAudit();
                statusLabel.Text = newAudit.StatusMessage;
                statusLabel.ForeColor = newAudit.IsFullyHooked ? Color.LightGreen : Color.Orange;

                repairBtn.Enabled = true;
                repairBtn.Text = "🔧 Repair Audio Engine (Post-Windows Update)";

                if (ok)
                {
                    MessageBox.Show(
                        "Audio Engine repaired successfully!\n\nEqualizer APO sAPO registry keys were re-injected and Windows Audio Service was reloaded.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _onSettingsChanged?.Invoke();
                }
                else
                {
                    MessageBox.Show($"Repair output:\n\n{logDetails}", "Repair Result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            this.Controls.Add(repairBtn);
        }
    }
}
