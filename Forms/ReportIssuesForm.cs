using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MunicipalReportingApp.Data;
using MunicipalReportingApp.Models;

namespace MunicipalReportingApp.Forms
{
    /// <summary>
    /// Allows a citizen to report a municipal issue: its location, category,
    /// a description, and optional supporting attachments. Includes a
    /// gamified progress-feedback engagement feature that encourages the
    /// citizen to complete the report.
    /// </summary>
    public class ReportIssuesForm : Form
    {
        // Encouraging messages shown as the citizen progresses through the
        // form. This is the "gamified progress feedback" engagement
        // strategy chosen and justified in the accompanying research report.
        private static readonly string[] EngagementMessages =
        {
            "Let's get started - tell us where the issue is located.",
            "Great start! Now choose a category for the issue.",
            "You're doing well - add a short description of the issue.",
            "Almost there! Attach a photo if you can, or submit now.",
            "All set - thank you for helping improve our community! Click Submit."
        };

        private TextBox txtLocation;
        private ComboBox cmbCategory;
        private RichTextBox rtbDescription;
        private ListBox lstAttachments;
        private Button btnAttach;
        private Button btnSubmit;
        private Button btnBack;
        private Label lblEngagement;
        private ProgressBar progressBarEngagement;

        // A List<string> is used to store attachment paths because a
        // report can have zero, one, or many attachments, and the order in
        // which they were added is worth preserving.
        private readonly List<string> _attachedFiles = new List<string>();

        public ReportIssuesForm()
        {
            BuildUi();
            UpdateEngagement();
        }

        private void BuildUi()
        {
            SuspendLayout();

            Text = "Report an Issue";
            ClientSize = new Size(560, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = MainMenuForm.BackgroundColor;
            Font = MainMenuForm.BodyFont;
            AutoScroll = true;

            var lblTitle = new Label
            {
                Text = "Report an Issue",
                Font = MainMenuForm.HeadingFont,
                ForeColor = MainMenuForm.PrimaryColor,
                AutoSize = false,
                Location = new Point(20, 20),
                Size = new Size(500, 36)
            };

            var lblIntro = new Label
            {
                Text = "Please provide as much detail as possible. Fields marked * are required.",
                ForeColor = Color.DimGray,
                AutoSize = false,
                Location = new Point(24, 58),
                Size = new Size(500, 20)
            };

            int y = 92;

            var lblLocation = MakeFieldLabel("Location *", y);
            txtLocation = new TextBox
            {
                Location = new Point(24, y + 20),
                Size = new Size(500, 26),
                PlaceholderText = "e.g. Corner of Main Rd and 5th Ave, Ward 12"
            };
            txtLocation.TextChanged += (s, e) => UpdateEngagement();

            y += 58;
            var lblCategory = MakeFieldLabel("Category *", y);
            cmbCategory = new ComboBox
            {
                Location = new Point(24, y + 20),
                Size = new Size(500, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategory.Items.AddRange(new object[]
            {
                "Sanitation",
                "Water and Sewerage",
                "Roads and Stormwater",
                "Electricity",
                "Waste Management",
                "Parks and Public Spaces",
                "Public Safety",
                "Other"
            });
            cmbCategory.SelectedIndexChanged += (s, e) => UpdateEngagement();

            y += 58;
            var lblDescription = MakeFieldLabel("Description *", y);
            rtbDescription = new RichTextBox
            {
                Location = new Point(24, y + 20),
                Size = new Size(500, 110),
                BorderStyle = BorderStyle.FixedSingle
            };
            rtbDescription.TextChanged += (s, e) => UpdateEngagement();

            y += 140;
            var lblAttachments = MakeFieldLabel("Attachments (optional)", y);
            btnAttach = new Button
            {
                Text = "Attach Image or Document...",
                Location = new Point(24, y + 20),
                Size = new Size(240, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = MainMenuForm.PrimaryColor
            };
            btnAttach.FlatAppearance.BorderColor = MainMenuForm.PrimaryColor;
            btnAttach.Click += BtnAttach_Click;

            lstAttachments = new ListBox
            {
                Location = new Point(280, y + 20),
                Size = new Size(244, 60)
            };

            y += 92;
            var lblEngagementHeading = MakeFieldLabel("Your progress", y);
            progressBarEngagement = new ProgressBar
            {
                Location = new Point(24, y + 20),
                Size = new Size(500, 20),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            lblEngagement = new Label
            {
                Location = new Point(24, y + 46),
                Size = new Size(500, 40),
                ForeColor = MainMenuForm.AccentColor,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic)
            };

            y += 96;
            btnSubmit = new Button
            {
                Text = "Submit Report",
                Location = new Point(24, y),
                Size = new Size(240, 40),
                Font = MainMenuForm.ButtonFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = MainMenuForm.AccentColor,
                ForeColor = Color.White
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += BtnSubmit_Click;

            btnBack = new Button
            {
                Text = "Back to Main Menu",
                Location = new Point(284, y),
                Size = new Size(240, 40),
                Font = MainMenuForm.ButtonFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = MainMenuForm.PrimaryColor,
                ForeColor = Color.White
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => Close();

            Controls.Add(lblTitle);
            Controls.Add(lblIntro);
            Controls.Add(lblLocation);
            Controls.Add(txtLocation);
            Controls.Add(lblCategory);
            Controls.Add(cmbCategory);
            Controls.Add(lblDescription);
            Controls.Add(rtbDescription);
            Controls.Add(lblAttachments);
            Controls.Add(btnAttach);
            Controls.Add(lstAttachments);
            Controls.Add(lblEngagementHeading);
            Controls.Add(progressBarEngagement);
            Controls.Add(lblEngagement);
            Controls.Add(btnSubmit);
            Controls.Add(btnBack);

            ResumeLayout(false);
        }

        private Label MakeFieldLabel(string text, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = MainMenuForm.PrimaryColor,
                AutoSize = false,
                Location = new Point(24, y),
                Size = new Size(400, 18)
            };
        }

        private void BtnAttach_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Attach an image or document";
                dialog.Filter =
                    "Images and Documents (*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.pdf;*.doc;*.docx)|" +
                    "*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.pdf;*.doc;*.docx|" +
                    "All files (*.*)|*.*";
                dialog.Multiselect = true;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (string fileName in dialog.FileNames)
                    {
                        if (!_attachedFiles.Contains(fileName))
                        {
                            _attachedFiles.Add(fileName);
                            lstAttachments.Items.Add(Path.GetFileName(fileName));
                        }
                    }
                    UpdateEngagement();
                }
            }
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            string location = txtLocation.Text.Trim();
            string description = rtbDescription.Text.Trim();

            if (string.IsNullOrEmpty(location) ||
                cmbCategory.SelectedIndex < 0 ||
                string.IsNullOrEmpty(description))
            {
                MessageBox.Show(
                    this,
                    "Please fill in the location, category and description before submitting your report.",
                    "Missing information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var issue = new Issue
            {
                Location = location,
                Category = cmbCategory.SelectedItem.ToString(),
                Description = description,
                AttachmentPaths = new List<string>(_attachedFiles)
            };

            IssueRepository.Add(issue);

            MessageBox.Show(
                this,
                string.Format(
                    "Thank you! Your report has been submitted successfully.\n\n" +
                    "Reference number: #{0}\n" +
                    "Category: {1}\n" +
                    "Location: {2}\n\n" +
                    "You have now helped submit {3} report(s) to your municipality. " +
                    "Every report helps our community!",
                    issue.Id, issue.Category, issue.Location, IssueRepository.Count),
                "Report submitted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ResetForm();
        }

        /// <summary>
        /// Clears the form so the citizen can immediately report another
        /// issue if they wish to, reinforcing repeat engagement.
        /// </summary>
        private void ResetForm()
        {
            txtLocation.Clear();
            cmbCategory.SelectedIndex = -1;
            rtbDescription.Clear();
            _attachedFiles.Clear();
            lstAttachments.Items.Clear();
            UpdateEngagement();
            txtLocation.Focus();
        }

        /// <summary>
        /// Core of the gamified progress-feedback engagement strategy:
        /// updates the progress bar and an encouraging message as the
        /// citizen fills in each field of the report.
        /// </summary>
        private void UpdateEngagement()
        {
            int score = 0;
            if (!string.IsNullOrWhiteSpace(txtLocation.Text)) score++;
            if (cmbCategory.SelectedIndex >= 0) score++;
            if (!string.IsNullOrWhiteSpace(rtbDescription.Text)) score++;
            if (_attachedFiles.Count > 0) score++;

            progressBarEngagement.Value = score * 25;
            lblEngagement.Text = EngagementMessages[score];
        }
    }
}
