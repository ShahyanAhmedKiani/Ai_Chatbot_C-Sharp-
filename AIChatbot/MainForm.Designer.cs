#nullable enable
namespace AIChatbot
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer? components = null;

        // Controls
        private Panel pnlHeader = null!;
        private Label lblTitle = null!;
        private Label lblSubtitle = null!;
        private Button btnClear = null!;
        private Button btnSettings = null!;
        private Panel pnlChatArea = null!;
        private Panel pnlMessages = null!;
        private FlowLayoutPanel flpMessages = null!;
        private Panel pnlTypingIndicator = null!;
        private Label lblTypingDots = null!;
        private Panel pnlInputArea = null!;
        private Panel pnlInputBorder = null!;
        private RichTextBox rtbInput = null!;
        private Button btnSend = null!;
        private Label lblHint = null!;
        private System.Windows.Forms.Timer timerDots = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel tsslStatus = null!;
        private ToolStripProgressBar tspbLoading = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();

            // ── Form ────────────────────────────────────────────────────────────
            Text            = "AI Assistant";
            Size            = new Size(920, 700);
            MinimumSize     = new Size(720, 520);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Color.FromArgb(13, 13, 20);
            Font            = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.Sizable;

            // ── Header ──────────────────────────────────────────────────────────
            pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 72,
                BackColor = Color.FromArgb(18, 18, 28),
            };
            pnlHeader.Paint += (s, e) =>
            {
                // Bottom border gradient line
                using var pen = new Pen(Color.FromArgb(60, 80, 200, 180), 1f);
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
                // Subtle left accent bar
                using var accentBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(0, 0), new Point(0, pnlHeader.Height),
                    Color.FromArgb(180, 100, 200, 255), Color.FromArgb(0, 100, 200, 255));
                e.Graphics.FillRectangle(accentBrush, 0, 0, 3, pnlHeader.Height);
            };

            // Avatar circle
            var pnlAvatar = new Panel
            {
                Size      = new Size(42, 42),
                Location  = new Point(20, 15),
                BackColor = Color.FromArgb(80, 60, 180)
            };
            pnlAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var bg = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(0,0), new Point(42,42),
                    Color.FromArgb(120, 80, 240), Color.FromArgb(60, 140, 255));
                e.Graphics.FillEllipse(bg, 0, 0, 41, 41);
                using var font = new Font("Segoe UI", 14f, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString("✦", font, Brushes.White, new RectangleF(0, 0, 42, 42), sf);
            };
            pnlAvatar.Region = new Region(new System.Drawing.Drawing2D.GraphicsPath());
            pnlAvatar.Paint += (s, e) =>
            {
                var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, 41, 41);
                ((Panel)s!).Region = new Region(path);
            };

            lblTitle = new Label
            {
                Text      = "AI Assistant",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(240, 240, 255),
                AutoSize  = true,
                Location  = new Point(72, 12)
            };

            lblSubtitle = new Label
            {
                Text      = "● Online",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(60, 210, 110),
                AutoSize  = true,
                Location  = new Point(74, 36)
            };

            btnSettings = new Button
            {
                Text      = "⚙",
                Size      = new Size(36, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 30, 45),
                ForeColor = Color.FromArgb(160, 160, 210),
                Font      = new Font("Segoe UI", 12f),
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSettings.FlatAppearance.BorderColor        = Color.FromArgb(50, 50, 70);
            btnSettings.FlatAppearance.BorderSize         = 1;
            btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 75);

            btnClear = new Button
            {
                Text      = "+ New Chat",
                Size      = new Size(100, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 30, 45),
                ForeColor = Color.FromArgb(160, 160, 210),
                Font      = new Font("Segoe UI", 8.5f),
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClear.FlatAppearance.BorderColor        = Color.FromArgb(50, 50, 70);
            btnClear.FlatAppearance.BorderSize         = 1;
            btnClear.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 75);

            pnlHeader.Controls.AddRange(new Control[] { pnlAvatar, lblTitle, lblSubtitle, btnClear, btnSettings });

            // ── Chat area ───────────────────────────────────────────────────────
            pnlChatArea = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(13, 13, 20),
                Padding   = new Padding(0)
            };

            pnlMessages = new Panel
            {
                Dock       = DockStyle.Fill,
                AutoScroll = true,
                BackColor  = Color.FromArgb(13, 13, 20),
                Padding    = new Padding(20, 16, 20, 8)
            };

            flpMessages = new FlowLayoutPanel
            {
                Dock          = DockStyle.Top,
                AutoSize      = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0)
            };

            pnlMessages.Controls.Add(flpMessages);

            // ── Typing indicator ────────────────────────────────────────────────
            pnlTypingIndicator = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 36,
                BackColor = Color.FromArgb(13, 13, 20),
                Visible   = false,
                Padding   = new Padding(24, 6, 0, 0)
            };

            lblTypingDots = new Label
            {
                Text      = "AI is thinking  ●  ○  ○",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 120, 200),
                AutoSize  = true,
                Dock      = DockStyle.Fill
            };
            pnlTypingIndicator.Controls.Add(lblTypingDots);

            timerDots = new System.Windows.Forms.Timer(components) { Interval = 500 };
            int _dotState = 0;
            timerDots.Tick += (_, _) =>
            {
                _dotState = (_dotState + 1) % 3;
                lblTypingDots.Text = _dotState switch
                {
                    0 => "AI is thinking  ●  ○  ○",
                    1 => "AI is thinking  ●  ●  ○",
                    _ => "AI is thinking  ●  ●  ●"
                };
            };

            pnlChatArea.Controls.Add(pnlMessages);
            pnlChatArea.Controls.Add(pnlTypingIndicator);

            // ── Input area ──────────────────────────────────────────────────────
            pnlInputArea = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 88,
                BackColor = Color.FromArgb(18, 18, 28),
                Padding   = new Padding(16, 12, 16, 10)
            };
            pnlInputArea.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(40, 50, 80), 1);
                e.Graphics.DrawLine(pen, 0, 0, ((Panel)s!).Width, 0);
            };

            // Input box with rounded border
            pnlInputBorder = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 24, 38),
                Padding   = new Padding(14, 10, 8, 10)
            };
            pnlInputBorder.Paint += (s, e) =>
            {
                var p = (Panel)s!;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var pen = new Pen(Color.FromArgb(55, 60, 110), 1.5f);
                e.Graphics.DrawRoundedRect(pen, new Rectangle(0, 0, p.Width - 1, p.Height - 1), 12);
            };

            rtbInput = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = Color.FromArgb(24, 24, 38),
                ForeColor   = Color.FromArgb(220, 220, 240),
                Font        = new Font("Segoe UI", 10.5f),
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.None,
                WordWrap    = true,
                AcceptsTab  = false,
                DetectUrls  = false
            };

            btnSend = new Button
            {
                Size      = new Size(48, 48),
                Dock      = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 60, 200),
                ForeColor = Color.White,
                Cursor    = Cursors.Hand,
                Text      = "➤",
                Font      = new Font("Segoe UI", 15f)
            };
            btnSend.FlatAppearance.BorderSize         = 0;
            btnSend.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 80, 220);
            btnSend.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 40, 170);
            btnSend.Paint += (s, e) =>
            {
                var b = (Button)s!;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    b.ClientRectangle,
                    Color.FromArgb(110, 80, 230), Color.FromArgb(60, 120, 255), 135f);
                var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(4, 4, b.Width - 8, b.Height - 8);
                e.Graphics.FillPath(brush, path);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var font = new Font("Segoe UI", 14f);
                e.Graphics.DrawString("➤", font, Brushes.White, b.ClientRectangle, sf);
            };

            lblHint = new Label
            {
                Text      = "Enter to send  ·  Shift+Enter for new line",
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(60, 65, 100),
                Dock      = DockStyle.Bottom,
                Height    = 16,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };

            pnlInputBorder.Controls.AddRange(new Control[] { rtbInput, btnSend });
            pnlInputArea.Controls.Add(pnlInputBorder);
            pnlInputArea.Controls.Add(lblHint);

            // ── Status strip ─────────────────────────────────────────────────────
            statusStrip = new StatusStrip
            {
                BackColor  = Color.FromArgb(12, 12, 18),
                SizingGrip = false
            };
            tsslStatus = new ToolStripStatusLabel
            {
                Text      = "Ready",
                ForeColor = Color.FromArgb(70, 75, 120),
                Spring    = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            tspbLoading = new ToolStripProgressBar
            {
                Size                  = new Size(90, 12),
                Visible               = false,
                Style                 = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 35
            };
            statusStrip.Items.AddRange(new ToolStripItem[] { tsslStatus, tspbLoading });

            // ── Assemble ─────────────────────────────────────────────────────────
            Controls.Add(pnlChatArea);
            Controls.Add(pnlInputArea);
            Controls.Add(pnlHeader);
            Controls.Add(statusStrip);

            ResumeLayout(false);
        }
    }
}
