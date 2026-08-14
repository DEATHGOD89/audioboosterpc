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
        private Action _onSettingsChanged;

        public SettingsForm(Action onSettingsChanged)
        {
            _onSettingsChanged = onSettingsChanged;

            this.Text = "EQ Settings";
            this.Size = new Size(350, 250);
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
                Location = new Point(20, 50),
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
                Location = new Point(20, 110),
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
                Location = new Point(20, 140),
                Width = 290
            };
            _clarityTrackBar.Scroll += (s, e) => 
            {
                _clarityLabel.Text = $"Sound Clarity: {_clarityTrackBar.Value}%";
                AudioProcessor.ClarityMultiplier = _clarityTrackBar.Value / 100f;
                _onSettingsChanged?.Invoke();
            };
            this.Controls.Add(_clarityTrackBar);
        }
    }
}
