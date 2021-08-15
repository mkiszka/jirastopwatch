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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Svg;

namespace StopWatch
{
    internal class IssueControl : UserControl
    {
        #region public members
        public string IssueKey
        {
            get
            {
                return cbJira.Text;
            }

            set
            {
                cbJira.Text = value;
                UpdateSummary();
            }
        }


        public WatchTimer WatchTimer { get; private set; }

        public bool MarkedForRemoval
        {
            get
            {
                return _MarkedForRemoval;
            }
        }

        public IEnumerable<Issue> AvailableIssues
        {
            set
            {
                cbJira.Items.Clear();
                foreach (var issue in value)
                    cbJira.Items.Add(new CBIssueItem(issue.Key, issue.Fields.Summary));
            }
        }


        public string Comment { get; set; }
        public EstimateUpdateMethods EstimateUpdateMethod { get; set; }
        public string EstimateUpdateValue { get; set; }

        private bool _current;
        public bool Current
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
                BackColor = tbTime.BackColor = cbJira.BackColor = cbJira.BorderColor = cbJira.ButtonColor = 
                    btnOpen.ForeColor = btnPostAndReset.ForeColor = btnRemoveIssue.ForeColor = btnReset.ForeColor = btnStartStop.ForeColor =
                    wbProject.Document.BackColor = wbIssueType.Document.BackColor = wbPriority.Document.BackColor =
                WatchTimer.Running ? Theme.TimeBackgroundRunning : value ? Theme.IssueBackgroundSelected : Theme.WindowBackground;
            }
        }

        public event EventHandler RemoveMeTriggered;

        public event EventHandler Selected;

        public event EventHandler TimeEdited;
        #endregion


        #region public events
        public event EventHandler TimerStarted;

        public event EventHandler TimerReset;
        #endregion


        #region public methods
        public IssueControl(MainForm mainForm, JiraClient jiraClient, Settings settings)
            : base()
        {
            InitializeComponent();
            UpdateTheme();

            cbJiraTbEvents = new ComboTextBoxEvents(cbJira);
            cbJiraTbEvents.Paste += cbJiraTbEvents_Paste;
            cbJiraTbEvents.MouseDown += CbJiraTbEvents_MouseDown;

            Comment = null;
            EstimateUpdateMethod = EstimateUpdateMethods.Auto;
            EstimateUpdateValue = null;

            this.settings = settings;

            this.jiraClient = jiraClient;
            this.WatchTimer = new WatchTimer();

            _mainForm = mainForm;
        }

        private void UpdateTheme()
        {
            this.BackColor = Theme.WindowBackground;
            this.btnRemoveIssue.BackColor = Theme.ButtonBackground;
            this.btnRemoveIssue.ForeColor = Theme.WindowBackground;
            this.btnRemoveIssue.Image = Theme.imgDelete;
            this.btnPostAndReset.BackColor = Theme.ButtonBackground;
            this.btnPostAndReset.ForeColor = Theme.WindowBackground;
            this.btnPostAndReset.Image = Theme.imgPostTime;
            this.btnReset.BackColor = Theme.ButtonBackground;
            this.btnReset.ForeColor = Theme.WindowBackground;
            this.btnReset.Image = Theme.imgReset;
            this.btnStartStop.BackColor = Theme.ButtonBackground;
            this.btnStartStop.ForeColor = Theme.WindowBackground;
            this.btnOpen.BackColor = Theme.ButtonBackground;
            this.btnOpen.ForeColor = Theme.WindowBackground;
            this.btnOpen.Image = Theme.imgOpenBrowser;
            this.BackColor = Theme.WindowBackground;
            this.cbJira.BackColor = Theme.TextBackground;
            this.cbJira.ForeColor = Theme.Text;
            this.cbJira.ButtonColor = Theme.WindowBackground;
            this.cbJira.BorderColor = Theme.WindowBackground;
            this.tbTime.BackColor = Theme.WindowBackground;
            this.tbTime.ForeColor = Theme.Text;
            this.lblSummary.ForeColor = Theme.Text;
            this.lblProject.ForeColor = Theme.TextMuted;
            this.wbProject.Document.BackColor = Theme.WindowBackground;
            this.wbIssueType.Document.BackColor = Theme.WindowBackground;
            this.wbPriority.Document.BackColor = Theme.WindowBackground;
            this.pSeperator.BackColor = Theme.Border;

            this.lblProject.Font = lblSummary.Font = new Font(Theme.RegularFont, 10.0F);
            this.cbJira.Font = this.tbTime.Font = new Font(Theme.BoldFont, 14.0F, FontStyle.Bold);
        }

        private void CbJiraTbEvents_MouseDown(object sender, EventArgs e)
        {
            SetSelected();
        }

        public void ToggleRemoveIssueButton(bool Enable)
        {
            this.btnRemoveIssue.Enabled = Enable;
            this.btnRemoveIssue.BackColor = Enable ? Theme.ButtonBackground : Theme.ButtonBackgroundDisabled;
        }

        public bool focusJiraField()
        {
            return this.cbJira.Focus();
        }

        public void UpdateOutput(bool updateSummary = false)
        {
            tbTime.Text = JiraTimeHelpers.TimeSpanToJiraTime(WatchTimer.TimeElapsed);

            if (WatchTimer.Running)
            {
                btnStartStop.BackgroundImage = Theme.imgPause;
                btnStartStop.Image = Theme.imgSpinner;
                this.BackColor = cbJira.BackColor = cbJira.ButtonColor = cbJira.BorderColor = tbTime.BackColor = btnOpen.ForeColor = btnReset.ForeColor = btnPostAndReset.ForeColor = btnRemoveIssue.ForeColor = btnStartStop.ForeColor =
                    Theme.TimeBackgroundRunning;
            }
            else {
                btnStartStop.BackgroundImage = Theme.imgPlay;
                btnStartStop.Image = null;
                this.BackColor = cbJira.BackColor = cbJira.ButtonColor = cbJira.BorderColor = tbTime.BackColor = btnOpen.ForeColor = btnReset.ForeColor = btnPostAndReset.ForeColor = btnRemoveIssue.ForeColor = btnStartStop.ForeColor =
                    this.Current ? Theme.IssueBackgroundSelected : Theme.WindowBackground;
            }

            if (string.IsNullOrEmpty(Comment))
                btnPostAndReset.Image = Theme.imgPostTime;
            else
                btnPostAndReset.Image = Theme.imgPostTimeNote;

            btnOpen.Enabled = cbJira.Text.Trim() != "";
            btnOpen.BackColor = btnOpen.Enabled ? Theme.ButtonBackground : Theme.ButtonBackgroundDisabled;
            btnReset.Enabled = WatchTimer.Running || WatchTimer.TimeElapsed.Ticks > 0;
            btnReset.BackColor = btnReset.Enabled ? Theme.ButtonBackground : Theme.ButtonBackgroundDisabled;
            btnPostAndReset.Enabled = WatchTimer.TimeElapsedNearestMinute.TotalMinutes >= 1;
            btnPostAndReset.BackColor = btnPostAndReset.Enabled ? Theme.ButtonBackground : Theme.ButtonBackgroundDisabled;

            if (updateSummary)
                UpdateSummary();
        }


        public void Start()
        {
            WatchTimer.Start();
            UpdateOutput();
        }


        public void Pause()
        {
            WatchTimer.Pause();
            UpdateOutput();
        }

        public void FocusKey()
        {
            cbJira.Focus();
        }
        #endregion


        #region private methods
        public void OpenJira()
        {
            if (cbJira.Text == "")
                return;

            OpenIssueInBrowser(cbJira.Text);
        }


        private void UpdateSummary()
        {

            if (cbJira.Text == "")
            {
                lblSummary.Text = "";
                lblProject.Text = "";
                wbProject.Navigate("");
                wbProject.Document.BackColor = wbProject.Parent.BackColor;
                wbIssueType.Navigate("");
                wbIssueType.Document.BackColor = wbIssueType.Parent.BackColor;
                wbPriority.Navigate("");
                wbPriority.Document.BackColor = wbPriority.Parent.BackColor;
                return;
            }
            if (!jiraClient.SessionValid)
            {
                lblSummary.Text = "";
                lblProject.Text = "";
                wbProject.Navigate("");
                wbProject.Document.BackColor = wbProject.Parent.BackColor;
                wbIssueType.Navigate("");
                wbIssueType.Document.BackColor = wbIssueType.Parent.BackColor;
                wbPriority.Navigate("");
                wbPriority.Document.BackColor = wbPriority.Parent.BackColor;
                return;
            }

            Task.Factory.StartNew(
                () => {
                    string key = "";
                    this.InvokeIfRequired(
                        () => key = cbJira.Text
                    );
                    try
                    {
                        Issue issue = jiraClient.GetIssueSummary(key, settings.IncludeProjectName);
                        this.InvokeIfRequired(
                            () =>
                            {
                                lblSummary.Text = issue.Fields.Summary;
                                lblProject.Text = issue.Fields.Project.Name;
                                wbProject.NavigateWithAuthorization(new Uri(issue.Fields.Project.AvatarUrls["16x16"]), settings);
                                wbIssueType.NavigateWithAuthorization(new Uri(issue.Fields.IssueType.IconUrl), settings);
                                wbPriority.NavigateWithAuthorization(new Uri(issue.Fields.Priority.IconUrl), settings);
                            }
                        );
                    }
                    catch (RequestDeniedException)
                    {
                        // just leave the existing summary there when fetch fails
                    }
                }
            );
        }

        private void UpdateRemainingEstimate(WorklogForm  worklogForm)
        {
            RemainingEstimate = "";
            RemainingEstimateSeconds = -1;

            if (cbJira.Text == "")
                return;
            if (!jiraClient.SessionValid)
                return;

            Task.Factory.StartNew(
                () =>
                {
                    string key = "";
                    this.InvokeIfRequired(
                        () => key = cbJira.Text
                    );

                    TimetrackingFields timetracking = jiraClient.GetIssueTimetracking(key);
                    if (timetracking == null)
                        return;

                    this.InvokeIfRequired(
                        () => RemainingEstimate = timetracking.RemainingEstimate
                    );
                    this.InvokeIfRequired(
                        () => RemainingEstimateSeconds = timetracking.RemainingEstimateSeconds
                    );
                    if (worklogForm != null)
                    {
                        this.InvokeIfRequired(
                            () => worklogForm.RemainingEstimate = timetracking.RemainingEstimate
                        );
                        this.InvokeIfRequired(
                            () => worklogForm.RemainingEstimateSeconds = timetracking.RemainingEstimateSeconds
                        );                        
                    }
                }
            );
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tbTime = new System.Windows.Forms.TextBox();
            this.lblSummary = new System.Windows.Forms.Label();
            this.ttIssue = new System.Windows.Forms.ToolTip(this.components);
            this.btnRemoveIssue = new System.Windows.Forms.Button();
            this.btnPostAndReset = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.lblProject = new System.Windows.Forms.Label();
            this.wbProject = new System.Windows.Forms.WebBrowser();
            this.wbIssueType = new System.Windows.Forms.WebBrowser();
            this.wbPriority = new System.Windows.Forms.WebBrowser();
            this.pSeperator = new System.Windows.Forms.Panel();
            this.cbJira = new FlatComboBox();
            this.pbProject = new System.Windows.Forms.PictureBox();
            this.pbIssueType = new System.Windows.Forms.PictureBox();
            this.pbPriority = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbProject)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbIssueType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPriority)).BeginInit();
            this.SuspendLayout();
            // 
            // tbTime
            // 
            this.tbTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbTime.Font = new System.Drawing.Font("Avenir Next", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbTime.Location = new System.Drawing.Point(277, 20);
            this.tbTime.Name = "tbTime";
            this.tbTime.ReadOnly = true;
            this.tbTime.Size = new System.Drawing.Size(99, 29);
            this.tbTime.TabIndex = 3;
            this.tbTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tbTime.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbTime_KeyDown);
            this.tbTime.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tbTime_MouseDoubleClick);
            this.tbTime.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tbTime_MouseUp);
            // 
            // lblSummary
            // 
            this.lblSummary.AutoEllipsis = true;
            this.lblSummary.Font = new System.Drawing.Font("Avenir Next", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSummary.Location = new System.Drawing.Point(35, 46);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(343, 17);
            this.lblSummary.TabIndex = 6;
            this.lblSummary.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblSummary_MouseUp);
            // 
            // btnRemoveIssue
            // 
            this.btnRemoveIssue.BackColor = System.Drawing.Color.Transparent;
            this.btnRemoveIssue.Enabled = false;
            this.btnRemoveIssue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveIssue.ForeColor = System.Drawing.Color.Transparent;
            this.btnRemoveIssue.Image = global::StopWatch.Properties.Resources.delete24;
            this.btnRemoveIssue.Location = new System.Drawing.Point(469, 32);
            this.btnRemoveIssue.Name = "btnRemoveIssue";
            this.btnRemoveIssue.Size = new System.Drawing.Size(31, 32);
            this.btnRemoveIssue.TabIndex = 7;
            this.ttIssue.SetToolTip(this.btnRemoveIssue, "Remove issue row (CTRL-DEL)");
            this.btnRemoveIssue.UseVisualStyleBackColor = false;
            this.btnRemoveIssue.Click += new System.EventHandler(this.btnRemoveIssue_Click);
            // 
            // btnPostAndReset
            // 
            this.btnPostAndReset.BackColor = System.Drawing.Color.Transparent;
            this.btnPostAndReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPostAndReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPostAndReset.ForeColor = System.Drawing.Color.Transparent;
            this.btnPostAndReset.Image = global::StopWatch.Properties.Resources.posttime26;
            this.btnPostAndReset.Location = new System.Drawing.Point(437, 1);
            this.btnPostAndReset.Name = "btnPostAndReset";
            this.btnPostAndReset.Size = new System.Drawing.Size(31, 32);
            this.btnPostAndReset.TabIndex = 4;
            this.ttIssue.SetToolTip(this.btnPostAndReset, "Submit worklog to Jira and reset timer (CTRL-L)");
            this.btnPostAndReset.UseVisualStyleBackColor = false;
            this.btnPostAndReset.Click += new System.EventHandler(this.btnPostAndReset_Click);
            this.btnPostAndReset.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnPostAndReset_MouseUp);
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.Transparent;
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReset.ForeColor = System.Drawing.Color.Transparent;
            this.btnReset.Image = global::StopWatch.Properties.Resources.reset24;
            this.btnReset.Location = new System.Drawing.Point(437, 32);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(31, 32);
            this.btnReset.TabIndex = 5;
            this.ttIssue.SetToolTip(this.btnReset, "Reset timer (CTRL-R)");
            this.btnReset.UseVisualStyleBackColor = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            this.btnReset.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnReset_MouseUp);
            // 
            // btnStartStop
            // 
            this.btnStartStop.BackColor = System.Drawing.Color.Transparent;
            this.btnStartStop.BackgroundImage = global::StopWatch.Properties.Resources.play26;
            this.btnStartStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStartStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartStop.ForeColor = System.Drawing.Color.Transparent;
            this.btnStartStop.Location = new System.Drawing.Point(381, 7);
            this.btnStartStop.Margin = new System.Windows.Forms.Padding(0);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(51, 52);
            this.btnStartStop.TabIndex = 2;
            this.ttIssue.SetToolTip(this.btnStartStop, "Start/stop timer (CTRL-P)");
            this.btnStartStop.UseVisualStyleBackColor = false;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            this.btnStartStop.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnStartStop_MouseUp);
            // 
            // btnOpen
            // 
            this.btnOpen.BackColor = System.Drawing.Color.Transparent;
            this.btnOpen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpen.ForeColor = System.Drawing.Color.Transparent;
            this.btnOpen.Image = global::StopWatch.Properties.Resources.openbrowser26;
            this.btnOpen.Location = new System.Drawing.Point(469, 1);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(31, 32);
            this.btnOpen.TabIndex = 1;
            this.ttIssue.SetToolTip(this.btnOpen, "Open issue in browser (CTRL-O)");
            this.btnOpen.UseVisualStyleBackColor = false;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            this.btnOpen.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnOpen_MouseUp);
            // 
            // lblProject
            // 
            this.lblProject.AutoEllipsis = true;
            this.lblProject.Font = new System.Drawing.Font("Avenir Next", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProject.Location = new System.Drawing.Point(35, 2);
            this.lblProject.Name = "lblProject";
            this.lblProject.Size = new System.Drawing.Size(343, 17);
            this.lblProject.TabIndex = 8;
            this.lblProject.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblSummary_MouseUp);
            // 
            // wbProject
            // 
            this.wbProject.AllowWebBrowserDrop = false;
            this.wbProject.IsWebBrowserContextMenuEnabled = false;
            this.wbProject.Location = new System.Drawing.Point(16, 2);
            this.wbProject.Margin = new System.Windows.Forms.Padding(0);
            this.wbProject.MaximumSize = new System.Drawing.Size(16, 16);
            this.wbProject.MinimumSize = new System.Drawing.Size(16, 16);
            this.wbProject.Name = "wbProject";
            this.wbProject.ScriptErrorsSuppressed = true;
            this.wbProject.ScrollBarsEnabled = false;
            this.wbProject.Size = new System.Drawing.Size(16, 16);
            this.wbProject.TabIndex = 13;
            this.wbProject.Url = new System.Uri("", System.UriKind.Relative);
            this.wbProject.WebBrowserShortcutsEnabled = false;
            this.wbProject.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.wb_DocumentCompleted);
            // 
            // wbIssueType
            // 
            this.wbIssueType.AllowWebBrowserDrop = false;
            this.wbIssueType.IsWebBrowserContextMenuEnabled = false;
            this.wbIssueType.Location = new System.Drawing.Point(16, 25);
            this.wbIssueType.Margin = new System.Windows.Forms.Padding(0);
            this.wbIssueType.MinimumSize = new System.Drawing.Size(16, 16);
            this.wbIssueType.Name = "wbIssueType";
            this.wbIssueType.ScriptErrorsSuppressed = true;
            this.wbIssueType.ScrollBarsEnabled = false;
            this.wbIssueType.Size = new System.Drawing.Size(16, 16);
            this.wbIssueType.TabIndex = 14;
            this.wbIssueType.Url = new System.Uri("", System.UriKind.Relative);
            this.wbIssueType.WebBrowserShortcutsEnabled = false;
            this.wbIssueType.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.wb_DocumentCompleted);
            // 
            // wbPriority
            // 
            this.wbPriority.AllowWebBrowserDrop = false;
            this.wbPriority.IsWebBrowserContextMenuEnabled = false;
            this.wbPriority.Location = new System.Drawing.Point(16, 46);
            this.wbPriority.Margin = new System.Windows.Forms.Padding(0);
            this.wbPriority.MinimumSize = new System.Drawing.Size(16, 16);
            this.wbPriority.Name = "wbPriority";
            this.wbPriority.ScriptErrorsSuppressed = true;
            this.wbPriority.ScrollBarsEnabled = false;
            this.wbPriority.Size = new System.Drawing.Size(16, 16);
            this.wbPriority.TabIndex = 15;
            this.wbPriority.Url = new System.Uri("", System.UriKind.Relative);
            this.wbPriority.WebBrowserShortcutsEnabled = false;
            this.wbPriority.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.wb_DocumentCompleted);
            // 
            // pSeperator
            // 
            this.pSeperator.Location = new System.Drawing.Point(0, 65);
            this.pSeperator.Name = "pSeperator";
            this.pSeperator.Size = new System.Drawing.Size(517, 1);
            this.pSeperator.TabIndex = 16;
            // 
            // cbJira
            // 
            this.cbJira.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbJira.BorderColor = System.Drawing.Color.Transparent;
            this.cbJira.DisplayMember = "Key";
            this.cbJira.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbJira.DropDownHeight = 90;
            this.cbJira.DropDownWidth = 488;
            this.cbJira.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbJira.Font = new System.Drawing.Font("Avenir Next Demi Bold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbJira.IntegralHeight = false;
            this.cbJira.ItemHeight = 26;
            this.cbJira.Location = new System.Drawing.Point(35, 18);
            this.cbJira.Name = "cbJira";
            this.cbJira.Size = new System.Drawing.Size(132, 32);
            this.cbJira.TabIndex = 0;
            this.cbJira.ValueMember = "Key";
            this.cbJira.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cbJira_DrawItem);
            this.cbJira.DropDown += new System.EventHandler(this.cbJira_DropDown);
            this.cbJira.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.cbJira_MeasureItem);
            this.cbJira.SelectionChangeCommitted += new System.EventHandler(this.cbJira_SelectionChangeCommitted);
            this.cbJira.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbJira_KeyDown);
            this.cbJira.Leave += new System.EventHandler(this.cbJira_Leave);
            // 
            // pbProject
            // 
            this.pbProject.BackColor = System.Drawing.Color.Transparent;
            this.pbProject.Location = new System.Drawing.Point(16, 2);
            this.pbProject.Name = "pbProject";
            this.pbProject.Size = new System.Drawing.Size(16, 16);
            this.pbProject.TabIndex = 17;
            this.pbProject.TabStop = false;
            // 
            // pbIssueType
            // 
            this.pbIssueType.BackColor = System.Drawing.Color.Transparent;
            this.pbIssueType.Location = new System.Drawing.Point(16, 25);
            this.pbIssueType.Name = "pbIssueType";
            this.pbIssueType.Size = new System.Drawing.Size(16, 16);
            this.pbIssueType.TabIndex = 18;
            this.pbIssueType.TabStop = false;
            // 
            // pbPriority
            // 
            this.pbPriority.BackColor = System.Drawing.Color.Transparent;
            this.pbPriority.Location = new System.Drawing.Point(16, 46);
            this.pbPriority.Name = "pbPriority";
            this.pbPriority.Size = new System.Drawing.Size(16, 16);
            this.pbPriority.TabIndex = 19;
            this.pbPriority.TabStop = false;
            // 
            // IssueControl
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.pSeperator);
            this.Controls.Add(this.wbPriority);
            this.Controls.Add(this.wbIssueType);
            this.Controls.Add(this.wbProject);
            this.Controls.Add(this.lblProject);
            this.Controls.Add(this.btnRemoveIssue);
            this.Controls.Add(this.btnPostAndReset);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.tbTime);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.cbJira);
            this.Controls.Add(this.pbProject);
            this.Controls.Add(this.pbIssueType);
            this.Controls.Add(this.pbPriority);
            this.Name = "IssueControl";
            this.Size = new System.Drawing.Size(517, 66);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.IssueControl_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.pbProject)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbIssueType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPriority)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        public void Reset()
        {
            Comment = null;
            EstimateUpdateMethod = EstimateUpdateMethods.Auto;
            EstimateUpdateValue = null;
            this.WatchTimer.Reset();
            UpdateOutput();

            if (this.TimerReset != null)
                this.TimerReset(this, new EventArgs());
        }


        public void OpenIssueInBrowser(string key)
        {
            if (string.IsNullOrEmpty(this.settings.JiraBaseUrl))
                return;

            string url = this.settings.JiraBaseUrl;
            if (!url.EndsWith("/"))
                url += "/";
            url += "browse/";
            url += key.Trim();
            System.Diagnostics.Process.Start(url);
        }
        #endregion


        #region private eventhandlers
        void cbJira_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index == 0)
                keyWidth = 0;
            CBIssueItem item = (CBIssueItem)cbJira.Items[e.Index];
            Font font = new Font(cbJira.Font.FontFamily, cbJira.Font.Size * 0.8f, cbJira.Font.Style);
            Size size = TextRenderer.MeasureText(e.Graphics, item.Key, font);
            e.ItemHeight = size.Height;
            if (keyWidth < size.Width)
                keyWidth = size.Width;
        }


        void cbJira_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the default background
            e.DrawBackground();

            CBIssueItem item = (CBIssueItem)cbJira.Items[e.Index];

            // Create rectangles for the columns to display
            Rectangle r1 = e.Bounds;
            Rectangle r2 = e.Bounds;

            r1.Width = keyWidth;

            r2.X = r1.Width + 5;
            r2.Width = 500 - keyWidth;

            Font font = new Font(e.Font.FontFamily, e.Font.Size * 0.8f, e.Font.Style);

            // Draw the text on the first column
            using (SolidBrush sb = new SolidBrush(e.ForeColor))
                e.Graphics.DrawString(item.Key, font, sb, r1);

            // Draw a line to isolate the columns 
            using (Pen p = new Pen(Color.Black))
                e.Graphics.DrawLine(p, r1.Right, 0, r1.Right, r1.Bottom);

            // Draw the text on the second column
            using (SolidBrush sb = new SolidBrush(e.ForeColor))
                e.Graphics.DrawString(item.Summary, font, sb, r2);

            // Draw a line to isolate the columns 
            using (Pen p = new Pen(Color.Black))
                e.Graphics.DrawLine(p, r1.Right, 0, r1.Right, 140);

        }


        private void cbJira_DropDown(object sender, EventArgs e)
        {
            LoadIssues();
        }


        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenJira();
        }


        private void cbJira_Leave(object sender, EventArgs e)
        {
            UpdateOutput(true);
        }


        private void cbJira_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Enter)
                UpdateOutput(true);
        }


        private void cbJiraTbEvents_Paste(object sender, EventArgs e)
        {
            PasteKeyFromClipboard();
        }

        public void PasteKeyFromClipboard()
        {
            if (Clipboard.ContainsText())
            {
                cbJira.Text = JiraKeyHelpers.ParseUrlToKey(Clipboard.GetText());
                UpdateOutput(true);
            }
        }

        public void CopyKeyToClipboard()
        {
            if (string.IsNullOrEmpty(cbJira.Text))
                return;
            Clipboard.SetText(cbJira.Text);
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            StartStop();
        }

        public void StartStop()
        {
            if (WatchTimer.Running) {
                this.WatchTimer.Pause();
            }
            else {
                this.WatchTimer.Start();

                this.TimerStarted?.Invoke(this, new EventArgs());
            }
            UpdateOutput(true);
        }

        private void btnRemoveIssue_Click(object sender, EventArgs e)
        {
            Remove();
        }

        public void Remove()
        {
            this._MarkedForRemoval = true;
            RemoveMeTriggered?.Invoke(this, new EventArgs());
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Reset();
        }


        private void btnPostAndReset_Click(object sender, EventArgs e)
        {
            PostAndReset();
        }

        public void PostAndReset()
        {
            using (var worklogForm = new WorklogForm(WatchTimer.GetInitialStartTime(), WatchTimer.TimeElapsedNearestMinute, Comment, EstimateUpdateMethod, EstimateUpdateValue))
            {
                UpdateRemainingEstimate(worklogForm);
                var formResult = worklogForm.ShowDialog(this);
                if (formResult == DialogResult.OK)
                {
                    Comment = worklogForm.Comment.Trim();
                    EstimateUpdateMethod = worklogForm.estimateUpdateMethod;
                    EstimateUpdateValue = worklogForm.EstimateValue;

                    PostAndReset(cbJira.Text, worklogForm.InitialStartTime, WatchTimer.TimeElapsedNearestMinute, Comment, EstimateUpdateMethod, EstimateUpdateValue);
                }
                else if (formResult == DialogResult.Yes)
                {
                    Comment = string.Format("{0}:{1}{2}", DateTime.Now.ToString("g"), Environment.NewLine, worklogForm.Comment.Trim());
                    EstimateUpdateMethod = worklogForm.estimateUpdateMethod;
                    EstimateUpdateValue = worklogForm.EstimateValue;
                    UpdateOutput();
                }
            }
        }

        private void tbTime_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            EditTime();

        }


        private void tbTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Enter)
                EditTime();
        }
        #endregion


        #region private methods
        private void PostAndReset(string key, DateTimeOffset startTime, TimeSpan timeElapsed, string comment, EstimateUpdateMethods estimateUpdateMethod, string estimateUpdateValue)
        {
            Task.Factory.StartNew(
                () =>
                {
                    this.InvokeIfRequired(
                        () => {
                            btnPostAndReset.Enabled = false;
                            btnPostAndReset.BackColor = Theme.ButtonBackgroundDisabled;
                            Cursor.Current = Cursors.WaitCursor;
                        }
                    );

                    bool postSuccesful = true;

                    // First post comment in Comment-track - and clear the comment string, if it should only be posted here
                    // Only actually post in Comment-track if text is not empty
                    if (settings.PostWorklogComment != WorklogCommentSetting.WorklogOnly && !string.IsNullOrEmpty(comment))
                    {
                        postSuccesful = jiraClient.PostComment(key, comment);
                        if (postSuccesful && settings.PostWorklogComment == WorklogCommentSetting.CommentOnly)
                            comment = "";
                    }

                    // Now post the WorkLog with timeElapsed - and comment unless it was reset
                    if (postSuccesful)
                        postSuccesful = jiraClient.PostWorklog(key, startTime, timeElapsed, comment, estimateUpdateMethod, estimateUpdateValue);

                    if (postSuccesful)
                    {
                        this.InvokeIfRequired(
                            () => {
                                _mainForm.UpdateTotalTimeLogged(timeElapsed);
                                Reset();
                            }
                        );
                    }

                    this.InvokeIfRequired(
                        () => {
                            btnPostAndReset.Enabled = true;
                            btnPostAndReset.BackColor = Theme.ButtonBackground;
                            Cursor.Current = DefaultCursor;
                        }
                    );
                }
            );
        }


        private void LoadIssues()
        {
            // TODO: This + the datasource for cbFilters should be moved into a controller layer
            var ctrlList = (Application.OpenForms[0] as MainForm).Controls.Find("cbFilters", true);
            if (ctrlList.Length == 0)
                return;

            var cbFilters = ctrlList[0] as ComboBox;
            if (cbFilters.SelectedIndex < 0)
                return;

            string jql = (cbFilters.SelectedItem as CBFilterItem).Jql;

            Task.Factory.StartNew(
                () =>
                {
                    List<Issue> availableIssues = jiraClient.GetIssuesByJQL(jql).Issues;

                    if (availableIssues == null)
                        return;

                    this.InvokeIfRequired(
                        () =>
                        {
                            AvailableIssues = availableIssues;
                            cbJira.DropDownHeight = 120;
                            cbJira.Invalidate();
                        }
                    );
                }
            );
        }


        public void EditTime()
        {
            using (var editTimeForm = new EditTimeForm(WatchTimer.TimeElapsed))
            {
                if (editTimeForm.ShowDialog(this) == DialogResult.OK)
                {
                    WatchTimer.TimeElapsed = editTimeForm.Time;

                    UpdateOutput();

                    TimeEdited?.Invoke(this, new EventArgs());
                }
            }
        }


        public void OpenCombo()
        {
            cbJira.Focus();
            cbJira.DroppedDown = true;
        }


        private void SetSelected()
        {
            Selected?.Invoke(this, new EventArgs());
        }
        #endregion

        #region private members
        private FlatComboBox cbJira;
        private Button btnOpen;
        private TextBox tbTime;
        private Button btnStartStop;
        private Button btnReset;
        private Label lblSummary;

        private ToolTip ttIssue;
        private System.ComponentModel.IContainer components;
        private Button btnPostAndReset;

        private JiraClient jiraClient;

        private Settings settings;

        private int keyWidth;
        private string RemainingEstimate;
        private int RemainingEstimateSeconds;
        private Button btnRemoveIssue;
        private bool _MarkedForRemoval = false;

        private ComboTextBoxEvents cbJiraTbEvents;
        private Label lblProject;
        private WebBrowser wbProject;
        private WebBrowser wbIssueType;
        private WebBrowser wbPriority;
        private Panel pSeperator;
        private PictureBox pbProject;
        private PictureBox pbIssueType;
        private PictureBox pbPriority;
        private MainForm _mainForm;
        #endregion


        #region private classes
        // content item for the combo box
        private class CBIssueItem {
            public string Key { get; set; }
            public string Summary { get; set; }

            public CBIssueItem(string key, string summary) {
                Key = key;
                Summary = summary;
            }
        }
        #endregion

        private void IssueControl_MouseUp(object sender, MouseEventArgs e)
        {
            SetSelected();
        }

        private void btnOpen_MouseUp(object sender, MouseEventArgs e)
        {
            SetSelected();
        }

        private void btnStartStop_MouseUp(object sender, MouseEventArgs e)
        {
            SetSelected();
        }

        private void tbTime_MouseUp(object sender, MouseEventArgs e)
        {
            SetSelected();
        }

        private void btnPostAndReset_MouseUp(object sender, MouseEventArgs e)
        {
            SetSelected();
        }

        private void btnReset_MouseUp(object sender, MouseEventArgs e)
        {
            SetSelected();
        }

        private void lblSummary_MouseUp(object sender, MouseEventArgs e)
        {
            SetSelected();
        }

        private void cbJira_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SetSelected();
            UpdateOutput(true);
        }

        private void wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var wb = sender as WebBrowser;
            if (wb != null)
            {
                if (wb.Document.Body != null)
                {
                    wb.Document.Body.InnerHtml += "<style>*{padding:0;margin:0;max-width:16px;max-height:16px;}</style>";
                    this.pbFromWb(wb)?.Hide();
                    wb.Show();
                }
                else if (wb.Url.ToString() != "about:blank")
                {
                    if (!string.IsNullOrEmpty(wb.DocumentText)) {
                        SvgDocument svg = SvgDocument.FromSvg<SvgDocument>(wb.DocumentText);
                        svg.Width = 16;
                        svg.Height = 16;
                        this.pbFromWb(wb).Image = svg.Draw();
                        this.pbFromWb(wb).Show();
                        wb.Hide();
                    }
                }
                wb.Document.BackColor = wb.Parent.BackColor;
            }
        }
        
        private PictureBox pbFromWb(WebBrowser wb)
        {
            if (wb == this.wbIssueType) return this.pbIssueType;
            if (wb == this.wbPriority) return this.pbPriority;
            if (wb == this.wbProject) return this.pbProject;
            return null;
        }
    }

    internal static class WebBrowserExtensions
    {
        internal static void NavigateWithAuthorization(this WebBrowser browser, Uri uri, Settings settings)
        {
            byte[] authData = System.Text.Encoding.UTF8.GetBytes($"{settings.Username}:{settings.PrivateApiToken}");
            string authHeader = "Authorization: Basic " + Convert.ToBase64String(authData) + "\r\n";
            browser.Navigate(uri, "", null, authHeader);
        }
    }
}
