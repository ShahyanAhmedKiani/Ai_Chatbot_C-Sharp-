#nullable enable
using AIChatbot.Models;
using AIChatbot.Services;
using Microsoft.Extensions.Configuration;

namespace AIChatbot
{
    public partial class MainForm : Form
    {
        // ── Fields ──────────────────────────────────────────────────────────────
        private readonly GeminiService _aiService;
        private readonly List<ChatMessage> _history = new();
        private CancellationTokenSource? _cts;
        private bool _isLoading;

        // ── Palette ──────────────────────────────────────────────────────────────
        static readonly Color UserBubbleBg   = Color.FromArgb(72, 52, 190);
        static readonly Color UserBubbleText = Color.FromArgb(235, 235, 255);
        static readonly Color AiBubbleBg     = Color.FromArgb(26, 28, 45);
        static readonly Color AiBubbleText   = Color.FromArgb(210, 215, 240);
        static readonly Color TimestampColor = Color.FromArgb(80, 85, 130);

        // ── Constructor ─────────────────────────────────────────────────────────
        public MainForm(IConfiguration configuration)
        {
            InitializeComponent();
            _aiService = new GeminiService(configuration);
            _aiService.StatusChanged += (_, msg) =>
            {
                if (InvokeRequired) Invoke(() => SetStatus(msg));
                else SetStatus(msg);
            };

            WireEvents();

            // Defer welcome card until form is fully laid out
            Load += (_, _) =>
            {
                SyncFlpWidth();
                ShowWelcomeCard();
            };
        }

        // ── Event Wiring ─────────────────────────────────────────────────────────
        private void WireEvents()
        {
            btnSend.Click     += async (_, _) => await SendAsync();
            btnClear.Click    += BtnClear_Click;
            btnSettings.Click += (_, _) => new SettingsForm().ShowDialog(this);
            rtbInput.KeyDown  += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                { e.SuppressKeyPress = true; await SendAsync(); }
            };
            Resize            += (_, _) => { PositionHeaderButtons(); SyncFlpWidth(); RefreshAllRows(); };
            pnlMessages.Resize += (_, _) => { SyncFlpWidth(); RefreshAllRows(); };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            PositionHeaderButtons();
            SyncFlpWidth();
            rtbInput.Focus();
        }

        private void PositionHeaderButtons()
        {
            btnSettings.Location = new Point(pnlHeader.Width - 50,  18);
            btnClear.Location    = new Point(pnlHeader.Width - 158, 18);
        }

        // ── Sync FlowLayoutPanel width to parent ─────────────────────────────────
        private void SyncFlpWidth()
        {
            int w = Math.Max(200, pnlMessages.ClientSize.Width);
            flpMessages.Width = w;
        }

        // ── Send ─────────────────────────────────────────────────────────────────
        private async Task SendAsync()
        {
            var text = rtbInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text) || _isLoading) return;

            rtbInput.Clear();
            SetLoading(true);

            var userMsg = new ChatMessage("user", text);
            _history.Add(userMsg);
            AddBubble(userMsg);

            _cts = new CancellationTokenSource();
            try
            {
                var reply = await _aiService.SendMessageAsync(_history, _cts.Token);
                var aiMsg = new ChatMessage("assistant", reply);
                _history.Add(aiMsg);
                AddBubble(aiMsg);
            }
            catch (OperationCanceledException)
            {
                if (_history.Count > 0 && _history[^1].IsUser)
                    _history.RemoveAt(_history.Count - 1);
                AddSystemNote("Request cancelled.");
            }
            catch (UnauthorizedAccessException ex)
            {
                AddSystemNote($"Authentication error: {ex.Message}");
                MessageBox.Show(ex.Message, "API Key Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex) { AddSystemNote($"Error: {ex.Message}"); }
            finally
            {
                SetLoading(false);
                _cts?.Dispose();
                _cts = null;
                rtbInput.Focus();
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            if (_history.Count == 0) return;
            if (MessageBox.Show("Start a new conversation?", "New Chat",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _history.Clear();
            flpMessages.Controls.Clear();
            ShowWelcomeCard();
            SetStatus("New conversation started");
        }

        // ── Welcome Card ─────────────────────────────────────────────────────────
        private void ShowWelcomeCard()
        {
            SyncFlpWidth();
            int w = Math.Max(300, flpMessages.ClientSize.Width - 2);

            var card = new Panel
            {
                Width     = w,
                Height    = 190,
                BackColor = Color.Transparent,
                Margin    = new Padding(0, 10, 0, 20)
            };

            card.Paint += (s, e) =>
            {
                var g  = e.Graphics;
                var pw = ((Panel)s!).Width;
                g.SmoothingMode          = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint      = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Card BG
                var cardRect = new Rectangle(0, 0, pw - 1, 184);
                using var bg = new SolidBrush(Color.FromArgb(22, 24, 40));
                g.FillRoundedRect(bg, cardRect, 16);

                // Top accent stripe
                using var accent = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(0, 0), new Point(pw, 0),
                    Color.FromArgb(90, 60, 210), Color.FromArgb(40, 120, 255));
                g.FillRoundedRect(accent, new Rectangle(0, 0, pw - 1, 4), 4);

                // Border
                using var border = new Pen(Color.FromArgb(35, 38, 65), 1f);
                g.DrawRoundedRect(border, cardRect, 16);

                // Icon circle
                int cx = pw / 2;
                using var iconBg = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(cx - 24, 16, 48, 48),
                    Color.FromArgb(110, 70, 230), Color.FromArgb(50, 120, 255), 135f);
                g.FillEllipse(iconBg, cx - 24, 16, 48, 48);
                using var iconFont = new Font("Segoe UI", 20f, FontStyle.Bold);
                var sfC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("✦", iconFont, Brushes.White, new RectangleF(cx - 24, 16, 48, 48), sfC);

                // Title
                using var titleFont = new Font("Segoe UI", 13f, FontStyle.Bold);
                var sfM = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("Hello! How can I help you today?", titleFont,
                    new SolidBrush(Color.FromArgb(230, 230, 255)),
                    new RectangleF(20, 74, pw - 40, 28), sfM);

                // Subtitle
                using var subFont = new Font("Segoe UI", 9.5f);
                g.DrawString("Ask me anything — coding, writing, analysis, math, or just chat.",
                    subFont, new SolidBrush(Color.FromArgb(100, 108, 160)),
                    new RectangleF(20, 104, pw - 40, 24), sfM);

                // Chips
                string[] chips   = { "💡  Explain a concept", "💻  Help with code", "✍  Write something" };
                int      chipH   = 26;
                int      chipW   = Math.Min(180, (pw - 60) / 3);
                int      totalW  = chips.Length * chipW + (chips.Length - 1) * 10;
                int      startX  = (pw - totalW) / 2;
                int      chipY   = 138;

                for (int i = 0; i < chips.Length; i++)
                {
                    int cx2 = startX + i * (chipW + 10);
                    using var chipBg = new SolidBrush(Color.FromArgb(32, 36, 58));
                    g.FillRoundedRect(chipBg, new Rectangle(cx2, chipY, chipW, chipH), 12);
                    using var chipBorder = new Pen(Color.FromArgb(45, 50, 85), 1f);
                    g.DrawRoundedRect(chipBorder, new Rectangle(cx2, chipY, chipW, chipH), 12);
                    using var chipFont = new Font("Segoe UI", 8f);
                    TextRenderer.DrawText(g, chips[i], chipFont,
                        new Rectangle(cx2, chipY, chipW, chipH),
                        Color.FromArgb(120, 128, 190),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };

            flpMessages.Controls.Add(card);
            ScrollToBottom();
        }

        // ── Chat Bubbles ─────────────────────────────────────────────────────────
        private void AddBubble(ChatMessage msg)
        {
            SyncFlpWidth();
            bool isUser  = msg.IsUser;
            int  rowW    = Math.Max(300, flpMessages.ClientSize.Width - 2);
            int  bubbleW = (int)(rowW * 0.68);

            using var measureFont = new Font("Segoe UI", 10.5f);
            var measSize = TextRenderer.MeasureText(
                msg.Content, measureFont,
                new Size(bubbleW - 28, 9000),
                TextFormatFlags.WordBreak | TextFormatFlags.Left);

            int bubbleH = Math.Max(44, measSize.Height + 26);
            int rowH    = bubbleH + 28;

            var row = new Panel
            {
                Width     = rowW,
                Height    = rowH,
                BackColor = Color.Transparent,
                Margin    = new Padding(0, 4, 0, 0)
            };

            // Capture values for Paint closure
            var capturedContent  = msg.Content;
            var capturedTime     = msg.Timestamp.ToString("hh:mm tt");
            var capturedIsUser   = isUser;
            var capturedBubbleW  = bubbleW;
            var capturedBubbleH  = bubbleH;
            var capturedMeasSize = measSize;

            row.Paint += (s, e) =>
            {
                var g   = e.Graphics;
                var pw  = ((Panel)s!).Width;
                g.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                int avatarSize = 34;
                int avatarX    = capturedIsUser ? pw - avatarSize - 4 : 4;

                // Avatar
                using var avBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(avatarX, 2, avatarSize, avatarSize),
                    capturedIsUser ? Color.FromArgb(120, 80, 230) : Color.FromArgb(60, 100, 220),
                    capturedIsUser ? Color.FromArgb(70, 40, 180)  : Color.FromArgb(30, 60, 180), 135f);
                g.FillEllipse(avBrush, avatarX, 2, avatarSize, avatarSize);

                string avChar = capturedIsUser ? "U" : "✦";
                using var avFont = new Font("Segoe UI", 11f, FontStyle.Bold);
                var sfAv = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(avChar, avFont, Brushes.White, new RectangleF(avatarX, 2, avatarSize, avatarSize), sfAv);

                // Bubble dimensions
                int actualBW = Math.Min(capturedBubbleW, capturedMeasSize.Width + 30);
                int bx       = capturedIsUser ? pw - avatarSize - 10 - actualBW : avatarSize + 10;
                var bRect    = new Rectangle(bx, 0, actualBW, capturedBubbleH);

                // Shadow
                using var shadow = new SolidBrush(Color.FromArgb(25, 0, 0, 0));
                g.FillRoundedRect(shadow, new Rectangle(bx + 2, 3, actualBW, capturedBubbleH), 14);

                // Bubble BG
                Color bgColor = capturedIsUser ? UserBubbleBg : AiBubbleBg;
                using var bgBrush = new SolidBrush(bgColor);
                g.FillRoundedRect(bgBrush, bRect, 14);

                // AI border
                if (!capturedIsUser)
                {
                    using var bPen = new Pen(Color.FromArgb(42, 50, 85), 1f);
                    g.DrawRoundedRect(bPen, bRect, 14);
                }

                // Text
                Color fg = capturedIsUser ? UserBubbleText : AiBubbleText;
                TextRenderer.DrawText(g, capturedContent, new Font("Segoe UI", 10.5f),
                    new Rectangle(bx + 14, 12, actualBW - 28, capturedBubbleH - 16),
                    fg, TextFormatFlags.WordBreak | TextFormatFlags.Left);

                // Timestamp
                int tsX = capturedIsUser ? bx + actualBW - 78 : bx;
                TextRenderer.DrawText(g, capturedTime, new Font("Segoe UI", 7.5f),
                    new Rectangle(tsX, capturedBubbleH + 6, 78, 16),
                    TimestampColor,
                    capturedIsUser ? TextFormatFlags.Right : TextFormatFlags.Left);
            };

            flpMessages.Controls.Add(row);
            ScrollToBottom();
        }

        private void AddSystemNote(string text)
        {
            SyncFlpWidth();
            int w   = Math.Max(200, flpMessages.ClientSize.Width - 2);
            var row = new Panel
            {
                Width     = w,
                Height    = 30,
                BackColor = Color.Transparent,
                Margin    = new Padding(0, 2, 0, 2)
            };
            var capturedText = text;
            row.Paint += (_, e) =>
                TextRenderer.DrawText(e.Graphics, "⚠  " + capturedText,
                    new Font("Segoe UI", 8.5f, FontStyle.Italic),
                    row.ClientRectangle, Color.FromArgb(180, 80, 80),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            flpMessages.Controls.Add(row);
            ScrollToBottom();
        }

        // ── Loading ──────────────────────────────────────────────────────────────
        private void SetLoading(bool on)
        {
            _isLoading                 = on;
            btnSend.Enabled            = !on;
            pnlTypingIndicator.Visible = on;
            tspbLoading.Visible        = on;
            if (on) timerDots.Start(); else timerDots.Stop();
            btnSend.Text = on ? "◼" : "➤";
            btnSend.Invalidate();
            if (on) btnSend.Click += CancelClick;
            else    btnSend.Click -= CancelClick;
        }

        private void CancelClick(object? s, EventArgs e)
        {
            _cts?.Cancel();
            SetStatus("Cancelling…");
        }

        // ── Utilities ────────────────────────────────────────────────────────────
        private void SetStatus(string msg) { tsslStatus.Text = msg; statusStrip.Refresh(); }

        private void ScrollToBottom() =>
            pnlMessages.AutoScrollPosition = new Point(0, flpMessages.Height + 500);

        private void RefreshAllRows()
        {
            SyncFlpWidth();
            int w = Math.Max(200, flpMessages.ClientSize.Width - 2);
            foreach (Control c in flpMessages.Controls)
            {
                c.Width = w;
                c.Invalidate();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        { _aiService.Dispose(); base.OnFormClosed(e); }
    }

    // ── Graphics helpers ──────────────────────────────────────────────────────────
    static class GfxHelper
    {
        public static void FillRoundedRect(this Graphics g, Brush br, Rectangle r, int radius)
        {
            using var path = RoundedPath(r, radius);
            g.FillPath(br, path);
        }
        public static void DrawRoundedRect(this Graphics g, Pen pen, Rectangle r, int radius)
        {
            using var path = RoundedPath(r, radius);
            g.DrawPath(pen, path);
        }
        static System.Drawing.Drawing2D.GraphicsPath RoundedPath(Rectangle r, int rad)
        {
            int d = rad * 2;
            var p = new System.Drawing.Drawing2D.GraphicsPath();
            p.AddArc(r.X,             r.Y,              d, d, 180, 90);
            p.AddArc(r.Right - d,     r.Y,              d, d, 270, 90);
            p.AddArc(r.Right - d,     r.Bottom - d,     d, d,   0, 90);
            p.AddArc(r.X,             r.Bottom - d,     d, d,  90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
