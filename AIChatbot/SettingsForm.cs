namespace AIChatbot
{
    /// <summary>
    /// Simple settings dialog that lets the user view/update the API key at runtime.
    /// </summary>
    public class SettingsForm : Form
    {
        private Label? lblApiKey;
        private TextBox? txtApiKey;
        private Label? lblInfo;
        private Button? btnSave;
        private Button? btnCancel;
        private string _appsettingsPath;

        public SettingsForm()
        {
            _appsettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            InitUI();
            LoadCurrentKey();
        }

        private void InitUI()
        {
            this.Text            = "Settings";
            this.Size            = new Size(520, 220);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = Color.FromArgb(26, 26, 36);
            this.ForeColor       = Color.FromArgb(200, 200, 220);

            lblApiKey = new Label
            {
                Text     = "Google Gemini API Key (Free):",
                Location = new Point(20, 20),
                Size     = new Size(300, 22),
                Font     = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(180, 180, 210)
            };

            txtApiKey = new TextBox
            {
                Location      = new Point(20, 46),
                Size          = new Size(462, 28),
                Font          = new Font("Segoe UI", 9.5f),
                BackColor     = Color.FromArgb(32, 32, 44),
                ForeColor     = Color.FromArgb(220, 220, 240),
                BorderStyle   = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            lblInfo = new Label
            {
                Text      = "Free key from aistudio.google.com · stored locally in appsettings.json",
                Location  = new Point(20, 82),
                Size      = new Size(462, 20),
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 100, 140)
            };

            btnSave = new Button
            {
                Text      = "Save & Restart",
                Location  = new Point(282, 120),
                Size      = new Size(120, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 70, 200),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text      = "Cancel",
                Location  = new Point(362, 120),
                Size      = new Size(120, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 55),
                ForeColor = Color.FromArgb(160, 160, 200),
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 80);
            btnCancel.Click += (_, _) => Close();

            this.Controls.AddRange(new Control[] { lblApiKey, txtApiKey, lblInfo, btnSave, btnCancel });
        }

        private void LoadCurrentKey()
        {
            try
            {
                if (!File.Exists(_appsettingsPath)) return;
                var json = File.ReadAllText(_appsettingsPath);
                var match = System.Text.RegularExpressions.Regex.Match(json, @"""ApiKey""\s*:\s*""([^""]+)""");
                if (match.Success) txtApiKey!.Text = match.Groups[1].Value;
            }
            catch { }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var key = txtApiKey?.Text.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show("API key cannot be empty.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var json = File.ReadAllText(_appsettingsPath);
                var updated = System.Text.RegularExpressions.Regex.Replace(
                    json, @"""ApiKey""\s*:\s*""[^""]*""", $"\"ApiKey\": \"{key}\"");
                File.WriteAllText(_appsettingsPath, updated);

                MessageBox.Show(
                    "API key saved. Please restart the application for changes to take effect.",
                    "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
