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
using System.Drawing;
using System.Windows.Forms;

namespace StopWatch
{
    public partial class EditTimeForm : Form
    {
        public TimeSpan Time { get; private set; }


        public EditTimeForm(TimeSpan time)
        {
            InitializeComponent();
            UpdateTheme();

            Time = time;

            tbTime.Text = JiraTimeHelpers.TimeSpanToJiraTime(Time);
        }

        public void UpdateTheme()
        {
            this.BackColor = Theme.ModalBackground;
            lblHeader.ForeColor = Theme.Text;
            tbTime.BackColor = Theme.TextBackground;
            tbTime.ForeColor = Theme.Text;
            tbTime.BorderStyle = BorderStyle.None;
            lblHint.ForeColor = Theme.Text;
            btnOk.ForeColor = Theme.Blue;
            btnOk.BackColor = Theme.ButtonBackground;
            btnCancel.ForeColor = Theme.TextMuted;
            btnCancel.BackColor = Theme.ButtonBackground;
            lblHint.Font = lblHeader.Font = new Font(Theme.RegularFont, 10.0F);
            tbTime.Font = new Font(Theme.BoldFont, 14.0F, FontStyle.Bold);
        }


            private void btnOk_Click(object sender, EventArgs e)
        {
            if (!ValidateTimeInput())
            {
                DialogResult = DialogResult.None;
                tbTime.BackColor = Color.Tomato;
                return;
            }

            tbTime.BackColor = Theme.TextBackground;
        }


        private bool ValidateTimeInput()
        {
            TimeSpan? time = JiraTimeHelpers.JiraTimeToTimeSpan(tbTime.Text);
            if (time == null)
                return false;

            Time = time.Value;

            return true;
        }

        private void tbTime_TextChanged(object sender, EventArgs e)
        {
            tbTime.BackColor = Theme.TextBackground;
        }
    }
}
