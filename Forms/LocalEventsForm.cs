using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MunicipalReportingApp.Data;
using MunicipalReportingApp.Models;

namespace MunicipalReportingApp.Forms
{
    /// <summary>
    /// Browse upcoming community events and announcements, search by keyword,
    /// category and date range, and view recommendations based on search history.
    /// </summary>
    public class LocalEventsForm : Form
    {
        private readonly EventRecommendationEngine _recommendationEngine = new EventRecommendationEngine();

        private TextBox txtKeyword;
        private ComboBox cmbCategory;
        private CheckBox chkUseDates;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private Label lblSearchSummary;
        private Label lblUpcomingCount;
        private Label lblCategoryCount;
        private Label lblDateCount;
        private Label lblSearchCount;
        private FlowLayoutPanel flowEvents;
        private ListBox lstRecommendations;
        private ListBox lstAnnouncements;
        private ListBox lstSearchHistory;
        private ToolTip toolTip;

        public LocalEventsForm()
        {
            toolTip = new ToolTip();
            BuildUi();
            PopulateCategories();
            ShowInitialEvents();
            RefreshSidePanels(null);
        }

        private void BuildUi()
        {
            SuspendLayout();
            Text = "Local Events and Announcements";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(980, 680);
            ClientSize = new Size(1240, 820);
            BackColor = MainMenuForm.BackgroundColor;
            Font = MainMenuForm.BodyFont;
            AutoScaleMode = AutoScaleMode.Font;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = MainMenuForm.BackgroundColor,
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(root);

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildSearchPanel(), 0, 1);
            root.Controls.Add(BuildStatisticsPanel(), 0, 2);
            root.Controls.Add(BuildContentPanel(), 0, 3);
            ResumeLayout(true);
        }

        private Control BuildHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = MainMenuForm.PrimaryColor,
                Padding = new Padding(28, 15, 24, 12)
            };
            var title = new Label
            {
                Text = "Local Events & Announcements",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 15),
                Size = new Size(730, 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var subtitle = new Label
            {
                Text = "Discover what is happening in your community and stay informed.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(220, 231, 239),
                Location = new Point(28, 58),
                Size = new Size(740, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var back = new Button
            {
                Text = "←  Main Menu",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = MainMenuForm.PrimaryColor,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(142, 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(Math.Max(780, ClientSize.Width - 190), 30),
                Cursor = Cursors.Hand
            };
            back.FlatAppearance.BorderSize = 0;
            back.Click += (s, e) => Close();
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(back);
            header.Resize += (s, e) =>
            {
                back.Left = header.ClientSize.Width - back.Width - 24;
                title.Width = Math.Max(300, back.Left - title.Left - 24);
                subtitle.Width = title.Width;
            };
            return header;
        }

        private Control BuildSearchPanel()
        {
            var outer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(22, 10, 22, 10)
            };
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 2,
                BackColor = Color.White
            };
            float[] widths = { 25F, 20F, 12F, 15F, 15F, 13F };
            foreach (float width in widths) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, width));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            outer.Controls.Add(grid);

            grid.Controls.Add(MakeFilterLabel("Search events"), 0, 0);
            grid.Controls.Add(MakeFilterLabel("Category"), 1, 0);
            grid.Controls.Add(MakeFilterLabel("Limit dates"), 2, 0);
            grid.Controls.Add(MakeFilterLabel("From"), 3, 0);
            grid.Controls.Add(MakeFilterLabel("To"), 4, 0);
            grid.Controls.Add(MakeFilterLabel("Actions"), 5, 0);

            txtKeyword = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 3, 8, 2),
                PlaceholderText = "Keyword, place or topic...",
                BorderStyle = BorderStyle.FixedSingle
            };
            txtKeyword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplySearch(); } };
            grid.Controls.Add(txtKeyword, 0, 1);

            cmbCategory = new ComboBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 3, 8, 2),
                DropDownStyle = ComboBoxStyle.DropDownList,
                IntegralHeight = false
            };
            grid.Controls.Add(cmbCategory, 1, 1);

            chkUseDates = new CheckBox
            {
                Text = "Use range",
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 3, 4, 2),
                ForeColor = MainMenuForm.PrimaryColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            chkUseDates.CheckedChanged += (s, e) =>
            {
                dtpStart.Enabled = chkUseDates.Checked;
                dtpEnd.Enabled = chkUseDates.Checked;
            };
            grid.Controls.Add(chkUseDates, 2, 1);

            dtpStart = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 3, 8, 2),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Enabled = false
            };
            grid.Controls.Add(dtpStart, 3, 1);

            dtpEnd = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 3, 8, 2),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(30),
                MinDate = DateTime.Today,
                Enabled = false
            };
            grid.Controls.Add(dtpEnd, 4, 1);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            var btnSearch = MakeButton("Search", MainMenuForm.AccentColor, Color.White, 76);
            btnSearch.Click += (s, e) => ApplySearch();
            var btnClear = MakeButton("Clear", Color.FromArgb(235, 239, 242), MainMenuForm.PrimaryColor, 60);
            btnClear.Click += (s, e) => ClearSearch();
            actions.Controls.Add(btnSearch);
            actions.Controls.Add(btnClear);
            grid.Controls.Add(actions, 5, 1);

            return outer;
        }

        private Control BuildStatisticsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = MainMenuForm.BackgroundColor, Padding = new Padding(22, 8, 22, 8) };
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int i = 0; i < 4; i++) table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            panel.Controls.Add(table);
            lblUpcomingCount = MakeStat("UPCOMING EVENTS", "0");
            lblCategoryCount = MakeStat("CATEGORIES", "0");
            lblDateCount = MakeStat("EVENT DATES", "0");
            lblSearchCount = MakeStat("YOUR SEARCHES", "0");
            table.Controls.Add(WrapStat(lblUpcomingCount), 0, 0);
            table.Controls.Add(WrapStat(lblCategoryCount), 1, 0);
            table.Controls.Add(WrapStat(lblDateCount), 2, 0);
            table.Controls.Add(WrapStat(lblSearchCount), 3, 0);
            return panel;
        }

        private Control BuildContentPanel()
        {
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(22, 0, 22, 18),
                BackColor = MainMenuForm.BackgroundColor
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var eventsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(14) };
            var eventsHeader = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.White };
            var eventsTitle = new Label
            {
                Text = "Upcoming in your area",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = MainMenuForm.PrimaryColor,
                Dock = DockStyle.Top,
                Height = 25
            };
            lblSearchSummary = new Label
            {
                Text = "Browse the upcoming community calendar.",
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 8.5F),
                Dock = DockStyle.Top,
                Height = 20
            };
            eventsHeader.Controls.Add(lblSearchSummary);
            eventsHeader.Controls.Add(eventsTitle);
            flowEvents = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = MainMenuForm.BackgroundColor,
                Padding = new Padding(2, 6, 6, 8),
                Margin = new Padding(0)
            };
            flowEvents.SizeChanged += (s, e) => ResizeEventCards();
            eventsPanel.Controls.Add(flowEvents);
            eventsPanel.Controls.Add(eventsHeader);
            content.Controls.Add(eventsPanel, 0, 0);

            var side = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(14, 0, 0, 0),
                BackColor = MainMenuForm.BackgroundColor
            };
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 39F));
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 27F));
            lstRecommendations = CreateEventListBox();
            lstAnnouncements = CreateEventListBox();
            lstSearchHistory = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 8.5F),
                IntegralHeight = false
            };
            side.Controls.Add(BuildSideBox("Recommended for you", lstRecommendations, "Suggestions adapt to your searches."), 0, 0);
            side.Controls.Add(BuildSideBox("Priority announcements", lstAnnouncements, "Higher priority notices are shown first."), 0, 1);
            side.Controls.Add(BuildSideBox("Recent searches", lstSearchHistory, "Search history is kept for this app session."), 0, 2);
            content.Controls.Add(side, 1, 0);

            return content;
        }

        private Control BuildSideBox(string heading, Control list, string subtitle)
        {
            var box = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12, 8, 12, 8), Margin = new Padding(0, 0, 0, 10) };
            var head = new Label
            {
                Text = heading,
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = MainMenuForm.PrimaryColor
            };
            var sub = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Top,
                Height = 21,
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.DimGray
            };
            list.Dock = DockStyle.Fill;
            box.Controls.Add(list);
            box.Controls.Add(sub);
            box.Controls.Add(head);
            return box;
        }

        private ListBox CreateEventListBox()
        {
            var list = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                IntegralHeight = false,
                FormattingEnabled = true
            };
            list.Format += (s, e) =>
            {
                LocalEvent localEvent = e.ListItem as LocalEvent;
                if (localEvent != null)
                    e.Value = string.Format("{0:dd MMM}  ·  {1}  [{2}]", localEvent.EventDate, localEvent.Title, localEvent.Category);
            };
            list.DoubleClick += (s, e) =>
            {
                LocalEvent selected = list.SelectedItem as LocalEvent;
                if (selected != null) ShowEventDetails(selected);
            };
            return list;
        }

        private void PopulateCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("All categories");
            foreach (string category in EventRepository.Categories) cmbCategory.Items.Add(category);
            cmbCategory.SelectedIndex = 0;
        }

        private void ShowInitialEvents()
        {
            List<LocalEvent> events = EventRepository.GetUpcomingQueueSnapshot();
            RenderEvents(events);
            lblSearchSummary.Text = string.Format("Showing {0} upcoming event(s). Use search to narrow the list.", events.Count);
            UpdateStatistics();
        }

        private void ApplySearch()
        {
            string category = cmbCategory.SelectedItem == null ? "All categories" : cmbCategory.SelectedItem.ToString();
            if (chkUseDates.Checked && dtpEnd.Value.Date < dtpStart.Value.Date)
            {
                MessageBox.Show(this, "The end date must be on or after the start date.", "Check date range",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime? start = chkUseDates.Checked ? dtpStart.Value.Date : (DateTime?)null;
            DateTime? end = chkUseDates.Checked ? dtpEnd.Value.Date : (DateTime?)null;
            List<LocalEvent> results = EventRepository.Search(txtKeyword.Text, category, start, end);
            _recommendationEngine.RecordSearch(txtKeyword.Text, category, chkUseDates.Checked,
                start ?? DateTime.Today, end ?? DateTime.Today.AddDays(30), results);
            RenderEvents(results);
            lblSearchSummary.Text = string.Format("{0} result(s) for your search · sorted by date", results.Count);
            RefreshSidePanels(results);
            UpdateStatistics();
        }

        private void ClearSearch()
        {
            txtKeyword.Clear();
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;
            chkUseDates.Checked = false;
            dtpStart.Value = DateTime.Today;
            dtpEnd.Value = DateTime.Today.AddDays(30);
            ShowInitialEvents();
            RefreshSidePanels(null);
        }

        private void RenderEvents(IList<LocalEvent> events)
        {
            flowEvents.SuspendLayout();
            flowEvents.Controls.Clear();
            if (events == null || events.Count == 0)
            {
                flowEvents.Controls.Add(new Label
                {
                    Text = "No matching upcoming events found.\r\nTry another keyword, category or date range.",
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = Color.DimGray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Width = Math.Max(250, flowEvents.ClientSize.Width - 28),
                    Height = 100,
                    Margin = new Padding(5, 18, 5, 5)
                });
            }
            else
            {
                foreach (LocalEvent localEvent in events) flowEvents.Controls.Add(CreateEventCard(localEvent));
            }
            flowEvents.ResumeLayout(true);
            ResizeEventCards();
        }

        private Control CreateEventCard(LocalEvent localEvent)
        {
            var card = new Panel
            {
                Width = Math.Max(300, flowEvents.ClientSize.Width - 28),
                Height = 150,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(3, 3, 5, 8),
                Tag = localEvent
            };
            var accent = new Panel
            {
                Width = 5,
                Dock = DockStyle.Left,
                BackColor = localEvent.IsAnnouncement ? Color.FromArgb(205, 133, 52) : MainMenuForm.AccentColor
            };
            var category = new Label
            {
                Text = localEvent.Category.ToUpperInvariant(),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = MainMenuForm.AccentColor,
                Location = new Point(17, 10),
                Size = new Size(390, 18),
                AutoEllipsis = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var date = new Label
            {
                Text = localEvent.EventDate.ToString("ddd, dd MMM yyyy · h:mm tt"),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = MainMenuForm.PrimaryColor,
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(Math.Max(350, card.Width - 230), 10),
                Size = new Size(210, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            var title = new Label
            {
                Text = localEvent.Title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = MainMenuForm.PrimaryColor,
                Location = new Point(17, 31),
                Size = new Size(card.Width - 38, 24),
                AutoEllipsis = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var location = new Label
            {
                Text = "●  " + localEvent.Location,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.DimGray,
                Location = new Point(18, 57),
                Size = new Size(card.Width - 40, 18),
                AutoEllipsis = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var description = new Label
            {
                Text = localEvent.Description,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(70, 78, 84),
                Location = new Point(18, 79),
                Size = new Size(card.Width - 40, 43),
                AutoEllipsis = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var tag = new Label
            {
                Text = localEvent.IsAnnouncement ? "ANNOUNCEMENT  ·  PRIORITY " + localEvent.Priority : "COMMUNITY EVENT",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = localEvent.IsAnnouncement ? Color.FromArgb(152, 91, 21) : Color.FromArgb(83, 105, 117),
                Location = new Point(18, 126),
                Size = new Size(card.Width - 40, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            toolTip.SetToolTip(description, localEvent.Description);
            toolTip.SetToolTip(title, localEvent.Title);
            card.Controls.Add(accent);
            card.Controls.Add(category);
            card.Controls.Add(date);
            card.Controls.Add(title);
            card.Controls.Add(location);
            card.Controls.Add(description);
            card.Controls.Add(tag);
            card.Click += (s, e) => ShowEventDetails(localEvent);
            foreach (Control child in card.Controls)
                if (child != accent) child.Click += (s, e) => ShowEventDetails(localEvent);
            return card;
        }

        private void ResizeEventCards()
        {
            if (flowEvents == null) return;
            int width = Math.Max(300, flowEvents.ClientSize.Width - 28);
            foreach (Control control in flowEvents.Controls)
            {
                control.Width = width;
                Panel card = control as Panel;
                if (card == null) continue;
                foreach (Control child in card.Controls)
                {
                    if (child is Label && child.Anchor.HasFlag(AnchorStyles.Right))
                    {
                        if (child.Text.IndexOf(" · ", StringComparison.Ordinal) >= 0 && child.Text.Length > 15 && child.Top == 10)
                            child.Left = Math.Max(250, card.Width - child.Width - 12);
                        else if (child.Top != 10)
                            child.Width = Math.Max(150, card.Width - child.Left - 12);
                    }
                    else if (child is Label && child.Anchor.HasFlag(AnchorStyles.Left))
                    {
                        if (child.Top != 10) child.Width = Math.Max(150, card.Width - child.Left - 12);
                    }
                }
            }
        }

        private void RefreshSidePanels(IList<LocalEvent> searchResults)
        {
            IEnumerable<LocalEvent> allUpcoming = EventRepository.GetUpcomingEvents();
            IEnumerable<int> excluded = searchResults == null ? Enumerable.Empty<int>() : searchResults.Select(ev => ev.Id);
            List<LocalEvent> recommendations = _recommendationEngine.GetRecommendations(allUpcoming, excluded, 5);
            if (recommendations.Count == 0 && searchResults != null)
                recommendations = _recommendationEngine.GetRecommendations(allUpcoming, Enumerable.Empty<int>(), 5);
            SetEventList(lstRecommendations, recommendations);
            SetEventList(lstAnnouncements, EventRepository.GetPriorityAnnouncements(6));
            lstSearchHistory.Items.Clear();
            foreach (EventSearchRecord search in _recommendationEngine.GetRecentSearches(5))
                lstSearchHistory.Items.Add(search.ToString());
            if (lstSearchHistory.Items.Count == 0) lstSearchHistory.Items.Add("Your searches will appear here.");
        }

        private void SetEventList(ListBox list, IList<LocalEvent> events)
        {
            list.BeginUpdate();
            list.Items.Clear();
            foreach (LocalEvent localEvent in events) list.Items.Add(localEvent);
            if (list.Items.Count == 0) list.Items.Add("No events to display.");
            list.EndUpdate();
        }

        private void UpdateStatistics()
        {
            SetStat(lblUpcomingCount, "UPCOMING EVENTS", EventRepository.GetUpcomingEvents().Count);
            SetStat(lblCategoryCount, "CATEGORIES", EventRepository.CategoryCount);
            SetStat(lblDateCount, "EVENT DATES", EventRepository.UniqueDateCount);
            SetStat(lblSearchCount, "YOUR SEARCHES", _recommendationEngine.SearchCount);
        }

        private void ShowEventDetails(LocalEvent localEvent)
        {
            string type = localEvent.IsAnnouncement ? "Announcement" : "Community event";
            string message = string.Format("{0}\r\n{1:dddd, dd MMMM yyyy} at {1:h:mm tt}\r\n\r\nCategory: {2}\r\nLocation: {3}\r\n{4}\r\n\r\n{5}",
                localEvent.Title, localEvent.EventDate, localEvent.Category, localEvent.Location,
                type + (localEvent.IsAnnouncement ? " · Priority " + localEvent.Priority + "/5" : string.Empty), localEvent.Description);
            MessageBox.Show(this, message, localEvent.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static Label MakeFilterLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = MainMenuForm.PrimaryColor,
                Margin = new Padding(2, 0, 2, 2)
            };
        }

        private static Button MakeButton(string text, Color background, Color foreground, int width)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = 31,
                Margin = new Padding(1, 2, 4, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = background,
                ForeColor = foreground,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Label MakeStat(string heading, string value)
        {
            return new Label
            {
                Text = value + "   " + heading,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.White,
                ForeColor = MainMenuForm.PrimaryColor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(12, 0, 8, 0),
                Margin = new Padding(4, 0, 4, 0),
                AutoEllipsis = true
            };
        }

        private static void SetStat(Label label, string heading, int value)
        {
            label.Text = value + "   " + heading;
        }

        private static Control WrapStat(Control content)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(2), BackColor = MainMenuForm.BackgroundColor };
            panel.Controls.Add(content);
            return panel;
        }
    }
}
