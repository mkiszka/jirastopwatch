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
using System.Windows.Forms;

namespace StopWatch
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pbSettings = new System.Windows.Forms.PictureBox();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.lblActiveFilter = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.lblTotalTime = new System.Windows.Forms.Label();
            this.tbTotalTime = new System.Windows.Forms.TextBox();
            this.pBottom = new System.Windows.Forms.Panel();
            this.pBottom.Dock = DockStyle.Bottom;
            this.btnTTLReset = new System.Windows.Forms.Button();
            this.tbTotalTimeRecorded = new System.Windows.Forms.TextBox();
            this.lbTotalTimeRecorded = new System.Windows.Forms.Label();
            this.pbAddIssue = new System.Windows.Forms.PictureBox();
            this.ttMain = new System.Windows.Forms.ToolTip(this.components);
            this.pbHelp = new System.Windows.Forms.PictureBox();
            this.pTop = new System.Windows.Forms.Panel();
            this.pTop.Dock = DockStyle.Top;
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabControl.Dock = DockStyle.Fill;
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.cbFilters = new FlatComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbSettings)).BeginInit();
            this.pBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAddIssue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbHelp)).BeginInit();
            this.pTop.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbSettings
            // 
            this.pbSettings.BackColor = System.Drawing.Color.Transparent;
            this.pbSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbSettings.Image = global::StopWatch.Properties.Resources.settings22;
            this.pbSettings.Location = new System.Drawing.Point(726, 8);
            this.pbSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pbSettings.Name = "pbSettings";
            this.pbSettings.Size = new System.Drawing.Size(36, 38);
            this.pbSettings.TabIndex = 0;
            this.pbSettings.TabStop = false;
            this.ttMain.SetToolTip(this.pbSettings, "Configure Jira Stopwatch");
            this.pbSettings.Click += new System.EventHandler(this.pbSettings_Click);
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblConnectionStatus.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.lblConnectionStatus.Location = new System.Drawing.Point(18, 12);
            this.lblConnectionStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(220, 32);
            this.lblConnectionStatus.TabIndex = 3;
            this.lblConnectionStatus.Text = "x";
            this.lblConnectionStatus.Click += new System.EventHandler(this.lblConnectionStatus_Click);
            // 
            // lblActiveFilter
            // 
            this.lblActiveFilter.AutoSize = true;
            this.lblActiveFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblActiveFilter.ForeColor = System.Drawing.Color.White;
            this.lblActiveFilter.Location = new System.Drawing.Point(18, 12);
            this.lblActiveFilter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblActiveFilter.Name = "lblActiveFilter";
            this.lblActiveFilter.Size = new System.Drawing.Size(54, 25);
            this.lblActiveFilter.TabIndex = 5;
            this.lblActiveFilter.Text = "Filter";
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "JIRA StopWatch";
            this.notifyIcon.Click += new System.EventHandler(this.notifyIcon_Click);
            // 
            // lblTotalTime
            // 
            this.lblTotalTime.AutoSize = true;
            this.lblTotalTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblTotalTime.Location = new System.Drawing.Point(208, 11);
            this.lblTotalTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTotalTime.Name = "lblTotalTime";
            this.lblTotalTime.Size = new System.Drawing.Size(181, 25);
            this.lblTotalTime.TabIndex = 6;
            this.lblTotalTime.Text = "Total Time Elapsed";
            this.lblTotalTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tbTotalTime
            // 
            this.tbTotalTime.BackColor = System.Drawing.SystemColors.Window;
            this.tbTotalTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbTotalTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbTotalTime.Location = new System.Drawing.Point(412, 9);
            this.tbTotalTime.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbTotalTime.Name = "tbTotalTime";
            this.tbTotalTime.ReadOnly = true;
            this.tbTotalTime.Size = new System.Drawing.Size(152, 30);
            this.tbTotalTime.TabIndex = 8;
            this.tbTotalTime.Text = "2D 45H 34M";
            this.tbTotalTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // pBottom
            // 
            this.pBottom.BackColor = System.Drawing.Color.Transparent;
            this.pBottom.Controls.Add(this.btnTTLReset);
            this.pBottom.Controls.Add(this.tbTotalTimeRecorded);
            this.pBottom.Controls.Add(this.lbTotalTimeRecorded);
            this.pBottom.Controls.Add(this.tbTotalTime);
            this.pBottom.Controls.Add(this.lblTotalTime);
            this.pBottom.Controls.Add(this.lblConnectionStatus);
            this.pBottom.ForeColor = System.Drawing.Color.Transparent;
            this.pBottom.Location = new System.Drawing.Point(0, 405);
            this.pBottom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(775, 91);
            this.pBottom.TabIndex = 10;
            // 
            // btnTTLReset
            // 
            this.btnTTLReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTTLReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTTLReset.Image = global::StopWatch.Properties.Resources.reset24;
            this.btnTTLReset.Location = new System.Drawing.Point(588, 42);
            this.btnTTLReset.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnTTLReset.Name = "btnTTLReset";
            this.btnTTLReset.Size = new System.Drawing.Size(45, 46);
            this.btnTTLReset.TabIndex = 12;
            this.btnTTLReset.UseVisualStyleBackColor = true;
            this.btnTTLReset.Click += new System.EventHandler(this.btnTotalTimeLogged_Click);
            // 
            // tbTotalTimeRecorded
            // 
            this.tbTotalTimeRecorded.BackColor = System.Drawing.SystemColors.Window;
            this.tbTotalTimeRecorded.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbTotalTimeRecorded.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbTotalTimeRecorded.Location = new System.Drawing.Point(412, 52);
            this.tbTotalTimeRecorded.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbTotalTimeRecorded.Name = "tbTotalTimeRecorded";
            this.tbTotalTimeRecorded.ReadOnly = true;
            this.tbTotalTimeRecorded.Size = new System.Drawing.Size(152, 30);
            this.tbTotalTimeRecorded.TabIndex = 11;
            this.tbTotalTimeRecorded.Text = "2D 45H 34M";
            this.tbTotalTimeRecorded.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lbTotalTimeRecorded
            // 
            this.lbTotalTimeRecorded.AutoSize = true;
            this.lbTotalTimeRecorded.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lbTotalTimeRecorded.Location = new System.Drawing.Point(218, 52);
            this.lbTotalTimeRecorded.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbTotalTimeRecorded.Name = "lbTotalTimeRecorded";
            this.lbTotalTimeRecorded.Size = new System.Drawing.Size(176, 25);
            this.lbTotalTimeRecorded.TabIndex = 10;
            this.lbTotalTimeRecorded.Text = "Total Time Logged";
            this.lbTotalTimeRecorded.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // pbAddIssue
            // 
            this.pbAddIssue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbAddIssue.Image = global::StopWatch.Properties.Resources.addissue22;
            this.pbAddIssue.Location = new System.Drawing.Point(633, 8);
            this.pbAddIssue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pbAddIssue.Name = "pbAddIssue";
            this.pbAddIssue.Size = new System.Drawing.Size(36, 38);
            this.pbAddIssue.TabIndex = 11;
            this.pbAddIssue.TabStop = false;
            this.ttMain.SetToolTip(this.pbAddIssue, "Add another issue row (CTRL-N)");
            this.pbAddIssue.Click += new System.EventHandler(this.pbAddIssue_Clicked);
            // 
            // pbHelp
            // 
            this.pbHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbHelp.Image = global::StopWatch.Properties.Resources.help22;
            this.pbHelp.Location = new System.Drawing.Point(680, 8);
            this.pbHelp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbHelp.Name = "pbHelp";
            this.pbHelp.Size = new System.Drawing.Size(36, 38);
            this.pbHelp.TabIndex = 12;
            this.pbHelp.TabStop = false;
            this.ttMain.SetToolTip(this.pbHelp, "Open help page in your browser");
            this.pbHelp.Click += new System.EventHandler(this.pbHelp_Click);
            // 
            // pTop
            // 
            this.pTop.BackColor = System.Drawing.Color.SteelBlue;
            this.pTop.Controls.Add(this.pbHelp);
            this.pTop.Controls.Add(this.lblActiveFilter);
            this.pTop.Controls.Add(this.cbFilters);
            this.pTop.Controls.Add(this.pbAddIssue);
            this.pTop.Controls.Add(this.pbSettings);
            this.pTop.Location = new System.Drawing.Point(0, 0);
            this.pTop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pTop.Name = "pTop";
            this.pTop.Size = new System.Drawing.Size(775, 54);
            this.pTop.TabIndex = 11;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Location = new System.Drawing.Point(-6, 54);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(788, 359);
            this.tabControl.TabIndex = 12;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage1.Size = new System.Drawing.Size(780, 326);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage2.Size = new System.Drawing.Size(780, 326);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // cbFilters
            // 
            this.cbFilters.DropDownWidth = 422;
            this.cbFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.cbFilters.FormattingEnabled = true;
            this.cbFilters.Location = new System.Drawing.Point(84, 8);
            this.cbFilters.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbFilters.Name = "cbFilters";
            this.cbFilters.Size = new System.Drawing.Size(224, 33);
            this.cbFilters.TabIndex = 4;
            this.cbFilters.DropDown += new System.EventHandler(this.cbFilters_DropDown);
            this.cbFilters.SelectedIndexChanged += new System.EventHandler(this.cbFilters_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(775, 498);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "JIRA StopWatch";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pbSettings)).EndInit();
            this.pBottom.ResumeLayout(false);
            this.pBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAddIssue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbHelp)).EndInit();
            this.pTop.ResumeLayout(false);
            this.pTop.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbSettings;
        private System.Windows.Forms.Label lblConnectionStatus;
        private FlatComboBox cbFilters;
        private System.Windows.Forms.Label lblActiveFilter;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Label lblTotalTime;
        private System.Windows.Forms.TextBox tbTotalTime;
        private System.Windows.Forms.Panel pBottom;
        private System.Windows.Forms.ToolTip ttMain;
        private System.Windows.Forms.PictureBox pbAddIssue;
        private System.Windows.Forms.Panel pTop;
        private System.Windows.Forms.PictureBox pbHelp;
        private System.Windows.Forms.TextBox tbTotalTimeRecorded;
        private System.Windows.Forms.Label lbTotalTimeRecorded;
        private System.Windows.Forms.Button btnTTLReset;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}

