/**************************************************************************
Copyright 2016 Carsten Gehling

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
**************************************************************************/
using StopWatch.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp.Authenticators;
using System.Runtime.InteropServices;

namespace StopWatch
{
    public partial class MainForm : Form
    {
        #region public methods
        public MainForm()
        {
            settings = Settings.Instance;
            if (!settings.Load())
                MessageBox.Show(string.Format("An error occurred while loading settings for Jira StopWatch. Your configuration file has most likely become corrupted.{0}{0}And older configuration file has been loaded instead, so please verify your settings.", Environment.NewLine), "Jira StopWatch");

            Logger.Instance.LogfilePath = Path.Combine(Application.UserAppDataPath, "jirastopwatch.log");
            Logger.Instance.Enabled = settings.LoggingEnabled;

            restRequestFactory = new RestRequestFactory();
            jiraApiRequestFactory = new JiraApiRequestFactory(restRequestFactory);

            restClientFactory = new RestClientFactory();
            restClientFactory.BaseUrl = this.settings.JiraBaseUrl;

            jiraApiRequester = new JiraApiRequester(restClientFactory, jiraApiRequestFactory, new HttpBasicAuthenticator(this.settings.Username, this.settings.PrivateApiToken));

            jiraClient = new JiraClient(jiraApiRequestFactory, jiraApiRequester);

            InitializeComponent();
            UpdateTheme();

            Text = string.Format("{0} v. {1}", Application.ProductName, Application.ProductVersion);

            cbFilters.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilters.DisplayMember = "Name";

            ticker = new Timer();
            // First run should be almost immediately after start
            ticker.Interval = firstDelay;
            ticker.Tick += ticker_Tick;

            UpdateTotalTimeLogged(new TimeSpan());

            this.tabControl.TabPages[this.tabControl.TabCount - 1].Text = "";
            this.tabControl.Padding = new Point(12, 4);
            this.tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;

            this.tabControl.DrawItem += tabControl1_DrawItem;
            this.tabControl.MouseDown += tabControl1_MouseDown;
            this.tabControl.Selecting += tabControl1_Selecting;
            this.tabControl.HandleCreated += tabControl1_HandleCreated;
            this.tabControl.MouseDoubleClick += TabControl_MouseDoubleClick;
        }
        
        public void UpdateTheme()
        {
            this.BackColor = Theme.WindowBackground;
            //this.lblActiveFilter.ForeColor = Theme.Text;
            this.cbFilters.BackColor = Theme.TextBackground;
            this.cbFilters.ForeColor = Theme.Text;
            this.cbFilters.ButtonColor = Theme.ButtonBackground;
            this.cbFilters.BorderColor = Theme.Border;
            this.lblConnectionStatus.ForeColor = Theme.Text;
            this.lblTotalTime.ForeColor = Theme.Text;
            this.lbTotalTimeRecorded.ForeColor = Theme.Text;
            this.tbTotalTime.BackColor = Theme.TimeBackground;
            this.tbTotalTime.ForeColor = Theme.Text;
            this.tbTotalTime.BorderStyle = BorderStyle.None;
            this.tbTotalTimeRecorded.BackColor = Theme.TimeBackground;
            this.tbTotalTimeRecorded.ForeColor = Theme.Text;
            this.tbTotalTimeRecorded.BorderStyle = BorderStyle.None;
            this.btnTTLReset.BackColor = Theme.ButtonBackground;
            this.btnTTLReset.ForeColor = Theme.WindowBackground;
            this.pTop.BackColor = Theme.Blue;
            this.tabControl.SelectedTab.BackColor = Theme.WindowBackground;
            this.tabControl.SelectedTab.BorderStyle = BorderStyle.None;

            lblActiveFilter.Font = lblTotalTime.Font = new Font(Theme.RegularFont, 10.0F);
            this.lblConnectionStatus.Font = this.tbTotalTime.Font = this.tbTotalTimeRecorded.Font = new Font(Theme.BoldFont, 10.0F, FontStyle.Bold);
        }


        public void HandleSessionLock()
        {
            if (settings.PauseOnSessionLock == PauseAndResumeSetting.NoPause)
                return;

            foreach (var issue in issueControls)
            {
                if (issue.WatchTimer.Running)
                {
                    lastRunningIssue = issue;
                    issue.InvokeIfRequired(
                        () => issue.Pause()
                    );
                    return;
                }
            }
        }


        public void HandleSessionUnlock()
        {
            if (settings.PauseOnSessionLock != PauseAndResumeSetting.PauseAndResume)
                return;

            if (lastRunningIssue != null)
            {
                lastRunningIssue.InvokeIfRequired(
                    () => lastRunningIssue.Start()
                );
                lastRunningIssue = null;
            }
        }
        #endregion


        #region private eventhandlers
        void issue_TimerStarted(object sender, EventArgs e)
        {
            IssueControl senderCtrl = (IssueControl)sender;
            ChangeIssueState(senderCtrl.IssueKey);

            if (settings.AllowMultipleTimers)
                return;

            foreach (var issue in this.issueControls)
                if (issue != senderCtrl)
                    issue.Pause();
        }


        void Issue_TimerReset(object sender, EventArgs e)
        {
            UpdateTotalTime();
        }


        void ticker_Tick(object sender, EventArgs e)
        {
            bool firstTick = ticker.Interval == firstDelay;

            ticker.Interval = defaultDelay;

            UpdateJiraRelatedData(firstTick);
            UpdateIssuesOutput(firstTick);

            SaveSettingsAndIssueStates();

            if (firstTick)
            {
                CheckForUpdates();
            }
        }

        public void UpdateTotalTimeLogged(TimeSpan timeElipsed)
        {
            TotalTimeLogged += timeElipsed;

            tbTotalTimeRecorded.Text = JiraTimeHelpers.TimeSpanToJiraTime(TotalTimeLogged);
        }

        private void pbSettings_Click(object sender, EventArgs e)
        {
            if (settings.AlwaysOnTop)
                this.TopMost = false;
            EditSettings();
            if (settings.AlwaysOnTop)
                this.TopMost = settings.AlwaysOnTop;
        }


        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettingsAndIssueStates();
        }


        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (this.settings.FirstRun)
            {
                this.settings.FirstRun = false;
                EditSettings();
            }
            else
            {
                if (IsJiraEnabled)
                    AuthenticateJira(this.settings.Username, this.settings.PrivateApiToken);
            }



            while (this.tabControl.TabCount <= this.settings.IssueCounts.Count())
            {
                int lastIndex = this.tabControl.TabCount - 1;
                string tabName = this.settings.TabNames.ContainsKey(lastIndex) ? this.settings.TabNames[lastIndex] : "New tab";

                this.tabControl.TabPages.Insert(lastIndex, tabName);
                this.tabControl.TabPages[lastIndex].BackColor = Theme.WindowBackground;
                this.tabControl.TabPages[lastIndex].BorderStyle = BorderStyle.None;
                Panel panel = this.GetPanel(lastIndex);
                this.tabControl.TabPages[lastIndex].Controls.Add(panel);
            }

            for (int tab = 0; tab < this.settings.IssueCounts.Count; tab++)
            {
                this.tabControl.SelectedIndex = tab;
                this.tabControl.SelectedTab.Text = this.settings.TabNames.ContainsKey(tab) ? this.settings.TabNames[tab] : "New tab";
                InitializeIssueControls();

                // Add issuekeys from settings to issueControl controls
                int i = 0;
                foreach (var issueControl in this.issueControls)
                {
                    if (settings.PersistedIssues.ContainsKey(tab) && i < settings.PersistedIssues[tab].Count)
                    {
                        var persistedIssue = settings.PersistedIssues[tab][i];
                        issueControl.IssueKey = persistedIssue.Key;

                        if (this.settings.SaveTimerState != SaveTimerSetting.NoSave)
                        {
                            TimerState timerState = new TimerState
                            {
                                Running = this.settings.SaveTimerState == SaveTimerSetting.SavePause ? false : persistedIssue.TimerRunning,
                                SessionStartTime = persistedIssue.SessionStartTime,
                                InitialStartTime = persistedIssue.InitialStartTime,
                                TotalTime = persistedIssue.TotalTime
                            };
                            issueControl.WatchTimer.SetState(timerState);
                            issueControl.Comment = persistedIssue.Comment;
                            issueControl.EstimateUpdateMethod = persistedIssue.EstimateUpdateMethod;
                            issueControl.EstimateUpdateValue = persistedIssue.EstimateUpdateValue;
                        }
                    }
                    i++;
                }
            }
            this.tabControl.SelectedIndex = 0;
            TotalTimeLogged = settings.TotalTimeLogged;

            UpdateTotalTimeLogged(new TimeSpan());

            ticker.Start();
        }


        private void cbFilters_DropDown(object sender, EventArgs e)
        {
            LoadFilters();
        }


        private void cbFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (CBFilterItem)cbFilters.SelectedItem;
            this.settings.CurrentFilter = item.Id;
        }


        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Mono for MacOSX and Linux do not implement the notifyIcon
            // so ignore this feature if we are not running on Windows
            if (!CrossPlatformHelpers.IsWindowsEnvironment())
                return;

            if (!this.settings.MinimizeToTray)
                return;

            if (WindowState == FormWindowState.Minimized)
            {
                this.notifyIcon.Visible = true;
                this.Hide();
            }
            else if (WindowState == FormWindowState.Normal)
            {
                this.notifyIcon.Visible = false;
            }
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void lblConnectionStatus_Click(object sender, EventArgs e)
        {
            if (jiraClient.SessionValid)
                return;

            string msg = string.Format("Jira StopWatch could not connect to your Jira server. Error returned:{0}{0}{1}", Environment.NewLine, jiraClient.ErrorMessage);
            MessageBox.Show(msg, "Connection error");
        }
        #endregion


        #region private methods
        private void AuthenticateJira(string username, string privateApiToken)
        {
            Task.Factory.StartNew(
                () =>
                {
                    this.InvokeIfRequired(
                        () =>
                        {
                            lblConnectionStatus.Text = "Connecting...";
                            lblConnectionStatus.ForeColor = Theme.Text;
                        }
                    );

                    if (jiraClient.Authenticate())
                        this.InvokeIfRequired(
                            () => UpdateIssuesOutput(true)
                        );

                    this.InvokeIfRequired(
                        () => UpdateJiraRelatedData(true)
                    );
                }
            );
        }

        private void issue_RemoveMeTriggered(object sender, EventArgs e)
        {
            //if (this.settings.IssueCount > 1)
            //{
            //    this.settings.IssueCount--;
            //    int currentIndex = this.tabControl.SelectedIndex;

            //    int issueCounts = this.GetCurrentPanelsIssueCount;
            //    this.issueCounts[currentIndex] = issueCounts--;
            //}
            int issueCounts;

            this.settings.IssueCounts.TryGetValue(this.tabControl.SelectedIndex, out issueCounts);

            if (issueCounts > 1)
            {
                issueCounts--;
                this.settings.IssueCounts[this.tabControl.SelectedIndex] = issueCounts;
            }

            this.InitializeIssueControls();
        }

        private void pbAddIssue_Clicked(object sender, EventArgs e)
        {
            IssueAdd();
        }

        private void AddCurrentIssue(int issues = 1)
        {
            int currentIndex = this.tabControl.SelectedIndex;
            int currentIssueCount;
            bool tabHasIssues = this.settings.IssueCounts.TryGetValue(currentIndex, out currentIssueCount);

            if (tabHasIssues)
            {
                int currentPanelIssues = this.GetCurrentPanelsIssueCount;
                this.settings.IssueCounts[currentIndex] = currentPanelIssues++;
            }
            else
            {
                this.settings.IssueCounts.Add(currentIndex, issues);
            }
        }

        private void IssueAdd()
        {
            this.AddCurrentIssue();

            //if (this.settings.IssueCount < maxIssues || this.issueControls.Count() < maxIssues)
            if (this.GetCurrentPanelsIssueCount < maxIssues)
            {
                int issueCounts;
                this.settings.IssueCounts.TryGetValue(this.tabControl.SelectedIndex, out issueCounts);
                issueCounts++;
                this.settings.IssueCounts[this.tabControl.SelectedIndex] = issueCounts;

                this.InitializeIssueControls();
                IssueControl AddedIssue = this.issueControls.Last();
                IssueSetCurrentByControl(AddedIssue);
                Panel currentPanel = this.GetCurrentPanel;
                int currentIndex = this.tabControl.SelectedIndex;

                if (currentPanel != null)
                {
                    currentPanel.ScrollControlIntoView(AddedIssue);
                    this.panels[currentIndex] = currentPanel;
                    this.tabControl.SelectedTab.Controls.Add(currentPanel);
                }
            }
        }

        private void InitializeIssueControls()
        {
            this.SuspendLayout();

            if (this.settings.IssueCounts == null)
            {
                this.settings.IssueCounts = new Dictionary<int, int>();
                this.AddCurrentIssue(1);
            }

            if (this.GetCurrentPanelsIssueCount >= maxIssues)
            {
                // Max reached.  Reset number in case it is larger 
                this.settings.IssueCounts[this.tabControl.SelectedIndex] = maxIssues;


                // Update tooltip to reflect the fact that you can't add anymore
                // We don't disable the button since then the tooltip doesn't show but
                // the click won't do anything if we have too many issues
                this.ttMain.SetToolTip(this.pbAddIssue, string.Format("You have reached the max limit of {0} issues and cannot add another", maxIssues.ToString()));
                this.pbAddIssue.Cursor = System.Windows.Forms.Cursors.No;
            }
            else
            {
                //if (this.GetCurrentPanelsIssueCount < 1)
                //{
                //    this.settings.IssueCounts[this.tabControl.SelectedIndex] = 1;
                //}

                // Reset status 
                this.ttMain.SetToolTip(this.pbAddIssue, "Add another issue row (CTRL-N)");
                this.pbAddIssue.Cursor = System.Windows.Forms.Cursors.Hand;
            }

            int count = this.tabControl.TabCount;
            int currentIndex = this.tabControl.SelectedIndex;
            if (this.panels == null)
            {
                this.GetPanel(0);
            }

            Panel currentPanel = this.GetCurrentPanel;

            // Remove IssueControl where user has clicked the remove button
            foreach (IssueControl issue in this.issueControls)
            {
                if (issue.MarkedForRemoval)
                {
                    if (currentPanel != null)
                    {
                        currentPanel.Controls.Remove(issue);
                        this.panels[currentIndex] = currentPanel;
                        this.tabControl.SelectedTab.Controls.Add(currentPanel);
                    }
                }
            }

            // In case a new tab was added but no issue were assigned
            int res;
            bool hasValue = this.settings.IssueCounts.TryGetValue(this.tabControl.SelectedIndex, out res);

            if (!hasValue)
            {
                this.AddCurrentIssue();
            }


            // If we have too many issueControl controls, compared to this.IssueCount
            // remove the ones not needed
            while (this.GetCurrentPanelsIssueCount > this.settings.IssueCounts[this.tabControl.SelectedIndex])
            {
                var issue = this.issueControls.Last();
                currentPanel.Controls.Remove(issue);
            }

            // Create issueControl controls needed
            while (this.GetCurrentPanelsIssueCount < this.settings.IssueCounts[this.tabControl.SelectedIndex])
            {
                var issue = new IssueControl(this, this.jiraClient, this.settings);
                issue.RemoveMeTriggered += new EventHandler(this.issue_RemoveMeTriggered);
                issue.TimerStarted += issue_TimerStarted;
                issue.TimerReset += Issue_TimerReset;
                issue.Selected += Issue_Selected;
                issue.TimeEdited += Issue_TimeEdited;
                currentPanel.Controls.Add(issue);
            }

            // To make sure that pMain's scrollbar doesn't screw up, all IssueControls need to have
            // their position reset, before positioning them again
            foreach (IssueControl issue in this.issueControls)
            {
                issue.Left = 0;
                issue.Top = 0;
            }

            // Now position all issueControl controls
            int i = 0;
            bool EnableRemoveIssue = this.issueControls.Count() > 1;
            foreach (IssueControl issue in this.issueControls)
            {
                issue.ToggleRemoveIssueButton(EnableRemoveIssue);
                issue.Top = i * issue.Height;
                i++;
            }
            int panelWithMostIssuesCount = this.GetPanelWithHighestIssueCount;
            this.ClientSize = new Size(pBottom.Width, 25 + (panelWithMostIssuesCount * issueControls.Last().Height + tabControl.Top + pBottom.Height));
            this.panels[currentIndex] = currentPanel;
            var workingArea = Screen.FromControl(this).WorkingArea;
            if (this.Height > workingArea.Height)
                this.Height = workingArea.Height;

            if (this.Bottom > workingArea.Bottom)
                this.Top = workingArea.Bottom - this.Height;

            tabControl.Height = (ClientSize.Height - pTop.Height - pBottom.Height + 5);
            this.tabControl.SelectedTab.Controls.Add(currentPanel);
            pBottom.Top = (ClientSize.Height - pBottom.Height);

            this.TopMost = this.settings.AlwaysOnTop;

            if (this.GetCurrentTabIssueIndex() >= issueControls.Count())
                IssueSetCurrent(issueControls.Count() - 1);
            else
                IssueSetCurrent(this.GetCurrentTabIssueIndex());

            this.ResumeLayout(false);
            this.PerformLayout();
            UpdateIssuesOutput(true);
        }

        private void Issue_TimeEdited(object sender, EventArgs e)
        {
            UpdateTotalTime();
        }

        private void Issue_Selected(object sender, EventArgs e)
        {
            IssueSetCurrentByControl((IssueControl)sender);
        }

        private void IssueSetCurrentByControl(IssueControl control)
        {
            int i = 0;
            foreach (var issue in issueControls)
            {
                if (issue == control)
                {
                    IssueSetCurrent(i);
                    return;
                }
                i++;
            }
        }

        private void UpdateIssuesOutput(bool updateSummary = false)
        {
            foreach (var issue in this.allIssueControls)
                issue.UpdateOutput(updateSummary);
            UpdateTotalTime();
        }


        private void UpdateTotalTime()
        {
            TimeSpan totalTime = new TimeSpan();
            foreach (var issue in this.allIssueControls)
                totalTime += issue.WatchTimer.TimeElapsed;
            tbTotalTime.Text = JiraTimeHelpers.TimeSpanToJiraTime(totalTime);
        }


        private void UpdateJiraRelatedData(bool firstTick)
        {
            Task.Factory.StartNew(
                () =>
                {
                    if (!IsJiraEnabled)
                    {
                        SetConnectionStatus(false);
                        return;
                    }

                    if (jiraClient.SessionValid || jiraClient.ValidateSession())
                    {
                        SetConnectionStatus(true);

                        this.InvokeIfRequired(
                            () =>
                            {
                                if (firstTick)
                                    LoadFilters();

                                UpdateIssuesOutput(firstTick);
                            }
                        );
                        return;
                    }

                    SetConnectionStatus(false);
                }
            );
        }


        private void SetConnectionStatus(bool connected)
        {
            this.InvokeIfRequired(
                () =>
                {
                    if (connected)
                    {
                        lblConnectionStatus.Text = "Connected";
                        lblConnectionStatus.ForeColor = Color.DarkGreen;
                        lblConnectionStatus.Font = new Font(Theme.BoldFont, 11.0F, FontStyle.Bold);
                        lblConnectionStatus.Cursor = Cursors.Default;
                    }
                    else
                    {
                        lblConnectionStatus.Text = "Not connected";
                        lblConnectionStatus.ForeColor = Color.Tomato;
                        lblConnectionStatus.Font = new Font(Theme.BoldFont, 11.0F, FontStyle.Bold | FontStyle.Underline);
                        lblConnectionStatus.Cursor = Cursors.Hand;
                    }
                }
            );
        }


        private void ChangeIssueState(string issueKey)
        {
            if (string.IsNullOrWhiteSpace(settings.StartTransitions))
                return;

            Task.Factory.StartNew(
                () =>
                {
                    var startTransitions = this.settings.StartTransitions
                        .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim().ToLower()).ToArray();

                    var availableTransitions = jiraClient.GetAvailableTransitions(issueKey);
                    if (availableTransitions == null || availableTransitions.Transitions.Count() == 0)
                        return;

                    foreach (var t in availableTransitions.Transitions)
                    {
                        if (startTransitions.Any(t.Name.ToLower().Contains))
                        {
                            jiraClient.DoTransition(issueKey, t.Id);
                            return;
                        }
                    }
                }
            );
        }


        private void EditSettings()
        {
            using (var form = new SettingsForm(this.settings))
            {
                if (form.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    restClientFactory.BaseUrl = this.settings.JiraBaseUrl;
                    jiraApiRequester = new JiraApiRequester(restClientFactory, jiraApiRequestFactory, new HttpBasicAuthenticator(this.settings.Username, this.settings.PrivateApiToken));
                    jiraClient = new JiraClient(jiraApiRequestFactory, jiraApiRequester);
                    Logging.Logger.Instance.Enabled = settings.LoggingEnabled;
                    if (IsJiraEnabled)
                        AuthenticateJira(this.settings.Username, this.settings.PrivateApiToken);
                    InitializeIssueControls();
                }
            }
        }


        private void SaveSettingsAndIssueStates()
        {
            settings.PersistedIssues.Clear();

            foreach (var panel in this.panels)
            {
                List<PersistedIssue> issueList = new List<PersistedIssue>();
                foreach (var issueControl in panel.Value.Controls.OfType<IssueControl>())
                {
                    TimerState timerState = issueControl.WatchTimer.GetState();

                    var persistedIssue = new PersistedIssue
                    {
                        Key = issueControl.IssueKey,
                        TimerRunning = timerState.Running,
                        SessionStartTime = timerState.SessionStartTime,
                        InitialStartTime = timerState.InitialStartTime,
                        TotalTime = timerState.TotalTime,
                        Comment = issueControl.Comment,
                        EstimateUpdateMethod = issueControl.EstimateUpdateMethod,
                        EstimateUpdateValue = issueControl.EstimateUpdateValue
                    };
                    issueList.Add(persistedIssue);
                }
                settings.PersistedIssues.Add(panel.Key, issueList);
            }

            settings.TotalTimeLogged = this.TotalTimeLogged;
            settings.TabNames = new Dictionary<int, string>();
            for(int tab = 0; tab < tabControl.TabCount - 1; tab++)
            {
                settings.TabNames.Add(tab, tabControl.TabPages[tab].Text);
            }

            this.settings.Save();
        }


        private void LoadFilters()
        {
            Task.Factory.StartNew(
                () =>
                {
                    List<Filter> filters = jiraClient.GetFavoriteFilters();
                    if (filters == null)
                        return;

                    filters.Insert(0, new Filter
                    {
                        Id = -1,
                        Name = "My open issues",
                        Jql = "assignee = currentUser() AND resolution = Unresolved order by updated DESC"
                    });

                    if (filters.Count() > 1)
                    {
                        filters.Insert(1, new Filter
                        {
                            Id = 0,
                            Name = "--------------",
                            Jql = ""
                        });
                    }

                    this.InvokeIfRequired(
                        () =>
                        {
                            CBFilterItem currentItem = null;

                            cbFilters.Items.Clear();
                            foreach (var filter in filters)
                            {
                                var item = new CBFilterItem(filter.Id, filter.Name, filter.Jql);
                                cbFilters.Items.Add(item);
                                if (item.Id == this.settings.CurrentFilter)
                                    currentItem = item;
                            }

                            if (currentItem != null)
                                cbFilters.SelectedItem = currentItem;
                        }
                    );
                }
            );
        }


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SHOWME)
                ShowOnTop();

            base.WndProc(ref m);
        }


        private void ShowOnTop()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Show();
                WindowState = FormWindowState.Normal;
                notifyIcon.Visible = false;
            }

            // get our current "TopMost" value (ours will always be false though)
            // make our form jump to the top of everything
            // set it back to whatever it was
            bool top = TopMost;
            TopMost = true;
            TopMost = top;
        }


        private void CheckForUpdates()
        {
            if (!settings.CheckForUpdate)
                return;

            Task.Factory.StartNew(
                () =>
                {
                    GithubRelease latestRelease = ReleaseHelper.GetLatestVersion();
                    if (latestRelease == null)
                        return;

                    string currentVersion = Application.ProductVersion;
                    if (string.Compare(latestRelease.TagName, currentVersion) <= 0)
                        return;

                    this.InvokeIfRequired(
                        () =>
                        {
                            string msg = string.Format("There is a newer version available of Jira StopWatch.{0}{0}Latest release is {1}. You are running version {2}.{0}{0}Do you want to download latest release?",
                                Environment.NewLine,
                                latestRelease.TagName,
                                currentVersion);
                            if (MessageBox.Show(msg, "New version available", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                System.Diagnostics.Process.Start("https://github.com/carstengehling/jirastopwatch/releases/latest");
                        }
                    );
                }
            );





        }
        #endregion


        #region private members
        private bool IsJiraEnabled
        {
            get
            {
                return !(
                    string.IsNullOrWhiteSpace(settings.JiraBaseUrl) ||
                    string.IsNullOrWhiteSpace(settings.Username) ||
                    string.IsNullOrWhiteSpace(settings.PrivateApiToken)
                );
            }
        }

        private IEnumerable<IssueControl> issueControls
        {
            get
            {
                return this.GetCurrentPanel.Controls.OfType<IssueControl>();
            }
        }

        private IEnumerable<IssueControl> allIssueControls
        {
            get
            {
                List<IssueControl> issueControls = new List<IssueControl>();
                foreach (var panel in this.panels)
                {
                    var issues = panel.Value.Controls.OfType<IssueControl>();
                    issueControls.AddRange(issues);
                }
                return issueControls;
            }
        }

        private Panel GetCurrentPanel
        {
            get
            {
                int currentIndex = this.tabControl.SelectedIndex;

                if (this.panels == null)
                {
                    this.panels = new Dictionary<int, Panel>();
                }

                Panel currentPanel;
                bool hasPanel = this.panels.TryGetValue(currentIndex, out currentPanel);
                if (!hasPanel)
                {
                    currentPanel = this.GetPanel(currentIndex);
                }
                return currentPanel;
            }
        }
        private int GetCurrentPanelsIssueCount
        {
            get
            {
                try
                {
                    int currentPanelIssues = this.GetCurrentPanel.Controls.OfType<IssueControl>().Count();
                    return currentPanelIssues;
                }
                catch
                {
                    return 1; // tab should have at least one issue
                }
            }
            set { }
        }

        private int GetPanelWithHighestIssueCount
        {
            get
            {
                int max = 1;
                foreach (var panel in this.panels)
                {
                    int issues = panel.Value.Controls.OfType<IssueControl>().Count();
                    if (max < issues)
                    {
                        max = issues;
                    }
                }
                return max;
            }
        }

        private IEnumerable<IssueControl> GetCurrentPanelsIssues()
        {
            return this.GetCurrentPanel.Controls.OfType<IssueControl>();
        }

        private Timer ticker;

        private JiraApiRequestFactory jiraApiRequestFactory;
        private RestRequestFactory restRequestFactory;
        private JiraApiRequester jiraApiRequester;
        private RestClientFactory restClientFactory;
        private JiraClient jiraClient;

        private Settings settings;

        private IssueControl lastRunningIssue = null;

        private TimeSpan TotalTimeLogged;

        /// <summary>
        /// key = index
        /// </summary>
        private Dictionary<int, Panel> panels;

        #endregion


        #region private consts
        private const int firstDelay = 500;
        private const int defaultDelay = 30000;
        private const int maxIssues = 20;
        #endregion

        //private int currentIssueIndex;
        private Dictionary<int, int> currentTabIssueIndex = new Dictionary<int, int>();

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Up))
            {
                IssueMoveUp();
                return true;
            }


            if (keyData == (Keys.Control | Keys.Down))
            {
                IssueMoveDown();
                return true;
            }

            if (keyData == (Keys.Control | Keys.P))
            {
                IssueTogglePlay();
                return true;
            }

            if (keyData == (Keys.Control | Keys.L))
            {
                IssuePostWorklog();
                return true;
            }

            if (keyData == (Keys.Control | Keys.E))
            {
                IssueEditTime();
                return true;
            }

            if (keyData == (Keys.Control | Keys.R))
            {
                IssueReset();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Delete))
            {
                IssueDelete();
                return true;
            }

            if (keyData == (Keys.Control | Keys.I))
            {
                IssueFocusKey();
                return true;
            }

            if (keyData == (Keys.Control | Keys.N))
            {
                IssueAdd();
                return true;
            }

            if (keyData == (Keys.Control | Keys.C))
            {
                IssueCopyToClipboard();
                return true;
            }

            if (keyData == (Keys.Control | Keys.V))
            {
                IssuePasteFromClipboard();
                return true;
            }

            if (keyData == (Keys.Control | Keys.O))
            {
                IssueOpenInBrowser();
                return true;
            }

            if (keyData == (Keys.Alt | Keys.Down))
            {
                IssueOpenCombo();
                return true;
            }

            if(keyData == (Keys.F2))
            {
                this.TabControl_MouseDoubleClick(null, null);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }



        private void IssueOpenInBrowser()
        {
            issueControls.ToList()[GetCurrentTabIssueIndex()].OpenJira();
        }

        private void IssueCopyToClipboard()
        {
            issueControls.ToList()[GetCurrentTabIssueIndex()].CopyKeyToClipboard();
        }

        private void IssuePasteFromClipboard()
        {
            issueControls.ToList()[GetCurrentTabIssueIndex()].PasteKeyFromClipboard();
        }

        private void IssueEditTime()
        {
            issueControls.ToList()[GetCurrentTabIssueIndex()].EditTime();
        }

        private void IssueDelete()
        {
            issueControls.ToList()[GetCurrentTabIssueIndex()].Remove();
        }

        private void IssueFocusKey()
        {
            issueControls.ToList()[GetCurrentTabIssueIndex()].FocusKey();
        }

        private void IssueReset()
        {
            issueControls.ToList()[GetCurrentTabIssueIndex()].Reset();
        }

        private void IssuePostWorklog()
        {
            issueControls.ToList()[GetCurrentTabIssueIndex()].PostAndReset();
        }

        private void IssueTogglePlay()
        {
            issueControls.ToList()[GetCurrentTabIssueIndex()].StartStop();
        }

        private void IssueMoveDown()
        {
            //if (currentIssueIndex == issueControls.Count() - 1)
            //    return;

            if (GetCurrentTabIssueIndex() == issueControls.Count() - 1)
            {
                return;
            }

            IssueSetCurrent(GetCurrentTabIssueIndex() + 1);
        }

        private void IssueMoveUp()
        {
            //if (currentIssueIndex == 0)
            //    return;
            int currentIndex = GetCurrentTabIssueIndex();
            if (currentIndex == 0)
            {
                return;
            }

            //IssueSetCurrent(currentIssueIndex - 1);
            IssueSetCurrent(currentIndex - 1);
        }

        private int GetCurrentTabIssueIndex()
        {
            int index;
            bool hasIndex = this.currentTabIssueIndex.TryGetValue(this.tabControl.SelectedIndex, out index);
            if (!hasIndex)
            {
                return default;
            }
            return index;
        }

        private void IssueSetCurrent(int index)
        {
            //currentIssueIndex = index;
            this.currentTabIssueIndex[this.tabControl.SelectedIndex] = index;
            int i = 0;
            Panel currentPanel = this.GetCurrentPanel;
            IEnumerable<IssueControl> issues = this.GetCurrentPanelsIssues();
            foreach (IssueControl issue in issues)
            {
                issue.Current = i == GetCurrentTabIssueIndex();
                if (i == GetCurrentTabIssueIndex())
                {
                    currentPanel.ScrollControlIntoView(issue);
                    issue.Focus();
                }
                i++;
            }
        }

        private void IssueOpenCombo()
        {
            issueControls.ToList()[GetCurrentTabIssueIndex()].OpenCombo();
        }

        private void pbHelp_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://jirastopwatch.com/doc");
        }

        private void btnTotalTimeLogged_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Do you want to reset the total time logged?", "", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                TotalTimeLogged = new TimeSpan();
                UpdateTotalTimeLogged(TotalTimeLogged);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;
        private void tabControl1_HandleCreated(object sender, EventArgs e)
        {
            SendMessage(this.tabControl.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr)16);
        }
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == this.tabControl.TabCount - 1)
                e.Cancel = true;
        }
        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            int lastIndex = this.tabControl.TabCount - 1;
            if (this.tabControl.GetTabRect(lastIndex).Contains(e.Location))
            {
                string tabName = Microsoft.VisualBasic.Interaction.InputBox("Enter a new name for this tab...", "New tab");
                if (!string.IsNullOrEmpty(tabName))
                {
                    this.tabControl.TabPages.Insert(lastIndex, tabName);
                    this.tabControl.SelectedIndex = lastIndex;
                    this.tabControl.TabPages[lastIndex].BackColor = Theme.WindowBackground;
                    this.tabControl.TabPages[lastIndex].BorderStyle = BorderStyle.None;
                    Panel panel = this.GetPanel(lastIndex);
                    this.tabControl.TabPages[lastIndex].Controls.Add(panel);
                    this.settings.IssueCounts.Add(lastIndex, 6);
                    this.InitializeIssueControls();
                }
            }
            else
            {
                for (int i = 0; i < this.tabControl.TabPages.Count; i++)
                {
                    Rectangle tabRect = this.tabControl.GetTabRect(i);
                    tabRect.Inflate(-2, -2);
                    Bitmap closeImage = Properties.Resources.Close;
                    Rectangle imageRect = new Rectangle(
                        (tabRect.Right - closeImage.Width),
                        tabRect.Top + (tabRect.Height - closeImage.Height) / 2,
                        closeImage.Width,
                        closeImage.Height);
                    if (imageRect.Contains(e.Location))
                    {
                        if (MessageBox.Show($"Are you sure you wish to remove the tab \"{this.tabControl.TabPages[i].Text}\"?\nThis will also remove all timers on this tab.\n\nThis action cannot be undone.", "Remove tab", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            this.tabControl.TabPages.RemoveAt(i);
                            this.RemoveTab(i); // remove panel
                        }
                        break;
                    }
                }
            }
        }

        private void TabControl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int lastIndex = this.tabControl.TabCount - 1;
            if (e == null || !this.tabControl.GetTabRect(lastIndex).Contains(e.Location))
            {

                string tabName = Microsoft.VisualBasic.Interaction.InputBox("Enter a new name for this tab...", "Rename tab", tabControl.SelectedTab.Text);
                if (!string.IsNullOrEmpty(tabName))
                {
                    tabControl.SelectedTab.Text = tabName;
                }
            }
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage tabPage = this.tabControl.TabPages[e.Index];
            Rectangle tabRect = this.tabControl.GetTabRect(e.Index);
            tabRect.Inflate(-2, -2);
            if (e.Index == this.tabControl.TabCount - 1)
            {
                Bitmap addImage = Properties.Resources.Add;
                e.Graphics.DrawImage(addImage,
                    tabRect.Left + (tabRect.Width - addImage.Width) / 2,
                    tabRect.Top + (tabRect.Height - addImage.Height) / 2);
            }
            else
            {
                Bitmap closeImage = Properties.Resources.Close;
                e.Graphics.DrawImage(closeImage,
                    (tabRect.Right - closeImage.Width),
                    tabRect.Top + (tabRect.Height - closeImage.Height) / 2);
                TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font,
                    tabRect, tabPage.ForeColor, TextFormatFlags.Left);
            }
        }

        private Panel GetPanel(int index)
        {
            Panel panel = new Panel
            {
                BackColor = Theme.WindowBackground,
                Location = new Point(0, 4),
                Margin = new Padding(0),
                Size = new Size(517, 710),
                TabIndex = 9,
            };
            panel.AutoScroll = false;
            panel.VerticalScroll.Visible = false;
            panel.AutoScroll = true;
            panel.HorizontalScroll.Maximum = 0;
            if (this.panels == null)
            {
                this.panels = new Dictionary<int, Panel>();
            }
            this.panels.Add(index, panel);

            return panel;
        }

        private void RemoveTab(int index)
        {
            this.panels.Remove(index);
            this.settings.IssueCounts.Remove(index);

            // Fix tab order when deleting tab
            int i = 0;
            var issueCounts = new Dictionary<int, int>();
            var newPanels = new Dictionary<int, Panel>();
            foreach (var issueCount in this.settings.IssueCounts.OrderBy(x => x.Key))
            {
                issueCounts[i] = issueCount.Value;
                newPanels[i] = this.panels[issueCount.Key];
                i++;
            }
            this.settings.IssueCounts = issueCounts;
            this.panels = newPanels;
            this.SaveSettingsAndIssueStates();
        }
    }

    // content item for the combo box
    public class CBFilterItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Jql { get; set; }

        public CBFilterItem(int id, string name, string jql)
        {
            Id = id;
            Name = name;
            Jql = jql;
        }
    }
}
