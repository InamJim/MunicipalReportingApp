using System;
using System.Drawing;
using System.Windows.Forms;
using MunicipalReportingApp.Data;

namespace MunicipalReportingApp.Forms
{
    /// <summary>
    /// The application's landing screen. Presents the three top-level
    /// citizen tasks. Report Issues and Local Events are available; Service
    /// Request Status remains a placeholder for Task 3.
    /// </summary>
    public class MainMenuForm : Form
    {
        // Shared colour palette so every form in the app looks consistent.
        public static readonly Color PrimaryColor = Color.FromArgb(23, 63, 95);      // deep municipal blue
        public static readonly Color AccentColor = Color.FromArgb(0, 153, 116);      // civic green (progress / success)
        public static readonly Color BackgroundColor = Color.FromArgb(245, 247, 249);
        public static readonly Color DisabledColor = Color.FromArgb(200, 200, 200);
        public static readonly Font HeadingFont = new Font("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font SubHeadingFont = new Font("Segoe UI", 10F, FontStyle.Italic);
        public static readonly Font ButtonFont = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 9.5F);

        private Label lblStatus;

        public MainMenuForm()
        {
            BuildUi();
        }

        private void BuildUi()
        {
            SuspendLayout();

            Text = "Municipal Services Portal";
            ClientSize = new Size(520, 480);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = BackgroundColor;
            Font = BodyFont;

            var lblTitle = new Label
            {
                Text = "Municipal Services Portal",
                Font = HeadingFont,
                ForeColor = PrimaryColor,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 30),
                Size = new Size(480, 40)
            };

            var lblSubtitle = new Label
            {
                Text = "Empowering citizens. Improving service delivery.",
                Font = SubHeadingFont,
                ForeColor = Color.DimGray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 72),
                Size = new Size(480, 26)
            };

            var lblInstruction = new Label
            {
                Text = "Please choose one of the options below to get started:",
                Font = BodyFont,
                ForeColor = Color.Black,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(40, 120),
                Size = new Size(440, 24)
            };

            var btnReportIssues = CreateMenuButton(
                "Report Issues",
                "Tell us about a problem in your area - roads, water, electricity, sanitation and more.",
                160, enabled: true);
            btnReportIssues.Click += BtnReportIssues_Click;

            var btnLocalEvents = CreateMenuButton(
                "Local Events and Announcements",
                "Browse upcoming community events and municipal announcements.",
                250, enabled: true);
            btnLocalEvents.Click += BtnLocalEvents_Click;

            var btnServiceStatus = CreateMenuButton(
                "Service Request Status",
                "Planned for Task 3 - track requests you have already submitted.",
                340, enabled: false);

            lblStatus = new Label
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = AccentColor,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 430),
                Size = new Size(480, 24)
            };
            UpdateStatusLabel();

            Controls.Add(lblTitle);
            Controls.Add(lblSubtitle);
            Controls.Add(lblInstruction);
            Controls.Add(btnReportIssues);
            Controls.Add(btnLocalEvents);
            Controls.Add(btnServiceStatus);
            Controls.Add(lblStatus);

            ResumeLayout(false);
        }

        /// <summary>
        /// Builds one of the three main-menu task buttons, including a
        /// short description underneath it, so the layout, colour scheme
        /// and interaction pattern stay identical across all three options.
        /// </summary>
        private Button CreateMenuButton(string title, string description, int top, bool enabled)
        {
            var button = new Button
            {
                Text = title,
                Font = ButtonFont,
                Size = new Size(440, 46),
                Location = new Point(40, top),
                FlatStyle = FlatStyle.Flat,
                Enabled = enabled,
                Cursor = enabled ? Cursors.Hand : Cursors.No,
                UseVisualStyleBackColor = false,
                BackColor = enabled ? PrimaryColor : DisabledColor,
                ForeColor = enabled ? Color.White : Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };
            button.FlatAppearance.BorderSize = 0;
            if (!enabled)
            {
                var toolTip = new ToolTip();
                toolTip.SetToolTip(button, "This feature will be implemented in a future release.");
            }

            var lblDescription = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.DimGray,
                AutoSize = false,
                Location = new Point(40, top + 48),
                Size = new Size(440, 32)
            };

            Controls.Add(lblDescription);
            return button;
        }

        private void BtnReportIssues_Click(object sender, EventArgs e)
        {
            using (var reportForm = new ReportIssuesForm())
            {
                reportForm.ShowDialog(this);
            }

            // Refresh the session summary now that the user may have
            // submitted one or more new reports.
            UpdateStatusLabel();
        }

        private void BtnLocalEvents_Click(object sender, EventArgs e)
        {
            using (var eventsForm = new LocalEventsForm())
            {
                eventsForm.ShowDialog(this);
            }
        }

        private void UpdateStatusLabel()
        {
            int count = IssueRepository.Count;
            lblStatus.Text = count == 1
                ? "1 issue reported so far this session - thank you for your help!"
                : string.Format("{0} issues reported so far this session - thank you for your help!", count);
        }
    }
}
