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
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace StopWatch
{
    public enum SaveTimerSetting
    {
        NoSave,
        SavePause,
        SaveRunActive
    }

    public enum PauseAndResumeSetting
    {
        NoPause,
        Pause,
        PauseAndResume
    }

    public enum WorklogCommentSetting
    {
        WorklogOnly,
        CommentOnly,
        WorklogAndComment
    }

    internal sealed class Settings
    {
        public static readonly Settings Instance = new Settings();

        #region public members
        public string JiraBaseUrl { get; set; }
        public bool AlwaysOnTop { get; set; }
        public bool MinimizeToTray { get; set; }
        public Dictionary<int,int> IssueCounts { get; set; }
        public Dictionary<int,string> TabNames { get; set; }
        public bool AllowMultipleTimers { get; set; }
        public bool IncludeProjectName { get; set; }

        public SaveTimerSetting SaveTimerState { get; set; }
        public PauseAndResumeSetting PauseOnSessionLock { get; set; }
        public WorklogCommentSetting PostWorklogComment { get; set; }

        public string Username { get; set; }
        public string PrivateApiToken { get; set; }

        public bool FirstRun { get; set; }

        public int CurrentFilter { get; set; }

        public Dictionary<int,List<PersistedIssue>> PersistedIssues { get; private set; }

        public TimeSpan TotalTimeLogged { get; set; }

        public string StartTransitions { get; set; }

        public bool LoggingEnabled { get; set; }

        public bool CheckForUpdate { get; set; }

        public string JiraAvatarUrl { get; set; }
        #endregion


        #region public methods
        public bool Load()
        {
            try
            {
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            }
            catch (ConfigurationErrorsException ex)
            {
                string filename = ex.Filename;
                if (File.Exists(filename))
                    File.Delete(filename);

                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.Reload();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.FirstRun = true;
                Properties.Settings.Default.Save();

                ReadSettings();
                return false;
            }

            // Check for upgrade because of application version change
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            ReadSettings();

            return true;
        }

        private void ReadSettings()
        {
            this.JiraBaseUrl = Properties.Settings.Default.JiraBaseUrl ?? "";

            this.AlwaysOnTop = Properties.Settings.Default.AlwaysOnTop;
            this.IncludeProjectName = Properties.Settings.Default.IncludeProjectName;
            this.MinimizeToTray = Properties.Settings.Default.MinimizeToTray;
            this.IssueCounts = ReadIssueCounts(Properties.Settings.Default.IssueCounts);
            this.TabNames = ReadTabNames(Properties.Settings.Default.TabNames);
            this.Username = Properties.Settings.Default.Username;
            this.PrivateApiToken = Properties.Settings.Default.PrivateApiToken != "" ? DPAPI.Decrypt(Properties.Settings.Default.PrivateApiToken) : "";
            this.FirstRun = Properties.Settings.Default.FirstRun;
            this.SaveTimerState = (SaveTimerSetting)Properties.Settings.Default.SaveTimerState;
            this.PauseOnSessionLock = (PauseAndResumeSetting)Properties.Settings.Default.PauseOnSessionLock;
            this.PostWorklogComment = (WorklogCommentSetting)Properties.Settings.Default.PostWorklogComment;

            this.CurrentFilter = Properties.Settings.Default.CurrentFilter;

            this.PersistedIssues = ReadIssues(Properties.Settings.Default.PersistedIssues);

            if (this.IssueCounts == null || this.IssueCounts.Count == 0)
            {
                int count = this.PersistedIssues.Count > 0 ? this.PersistedIssues[0].Count : 0;
                if (count <= 0) count = 6;
                this.IssueCounts = new Dictionary<int, int> { { 0, count} };
            }
            if (this.TabNames == null)
            {
                this.TabNames = new Dictionary<int, string>();
                foreach (var count in IssueCounts)
                {
                    this.TabNames.Add(count.Key, $"Tab {count.Key}");
                }
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.TotalTimeLogged) && 
                DateTime.Now.ToString("d") == Properties.Settings.Default.WorkingDate)
            {
                this.TotalTimeLogged = TimeSpan.Parse(Properties.Settings.Default.TotalTimeLogged);
            }

            this.AllowMultipleTimers = Properties.Settings.Default.AllowMultipleTimers;
            
            this.StartTransitions = Properties.Settings.Default.StartTransitions;

            this.LoggingEnabled = Properties.Settings.Default.LoggingEnabled;

            this.JiraAvatarUrl = Properties.Settings.Default.JiraAvatarUrl;

            CheckForUpdate = Properties.Settings.Default.CheckForUpdate;
        }


        public void Save()
        {
            lock (_writeLock)
            {
                Properties.Settings.Default.JiraBaseUrl = this.JiraBaseUrl;

                Properties.Settings.Default.AlwaysOnTop = this.AlwaysOnTop;
                Properties.Settings.Default.MinimizeToTray = this.MinimizeToTray;
                Properties.Settings.Default.IssueCounts = WriteIssueCounts(this.IssueCounts);
                Properties.Settings.Default.TabNames = WriteTabNames(this.TabNames);
                Properties.Settings.Default.IncludeProjectName = this.IncludeProjectName;

                Properties.Settings.Default.Username = this.Username;
                Properties.Settings.Default.PrivateApiToken = this.PrivateApiToken != "" ? DPAPI.Encrypt(this.PrivateApiToken) : "";

                Properties.Settings.Default.FirstRun = this.FirstRun;
                Properties.Settings.Default.SaveTimerState = (int)this.SaveTimerState;
                Properties.Settings.Default.PauseOnSessionLock = (int)this.PauseOnSessionLock;
                Properties.Settings.Default.PostWorklogComment = (int)this.PostWorklogComment;

                Properties.Settings.Default.CurrentFilter = this.CurrentFilter;

                Properties.Settings.Default.PersistedIssues = WriteIssues(this.PersistedIssues);

                Properties.Settings.Default.TotalTimeLogged = this.TotalTimeLogged.ToString();

                Properties.Settings.Default.WorkingDate = DateTime.Now.ToString("d");

                Properties.Settings.Default.AllowMultipleTimers = this.AllowMultipleTimers;

                Properties.Settings.Default.StartTransitions = this.StartTransitions;

                Properties.Settings.Default.LoggingEnabled = this.LoggingEnabled;

                Properties.Settings.Default.JiraAvatarUrl = this.JiraAvatarUrl;

                Properties.Settings.Default.CheckForUpdate = CheckForUpdate;

                Properties.Settings.Default.Save();
            }
        }

        public Dictionary<int, List<PersistedIssue>> ReadIssues(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new Dictionary<int, List<PersistedIssue>>();

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(data)))
            {
                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    return (Dictionary<int, List<PersistedIssue>>)bf.Deserialize(ms);
                }
                catch(InvalidCastException)
                {
                    try
                    {
                        ms.Position = 0;
                        List<PersistedIssue> issues = (List<PersistedIssue>)bf.Deserialize(ms);
                        return new Dictionary<int, List<PersistedIssue>>() { { 0, issues } };
                    }
                    catch(InvalidCastException)
                    {
                        return new Dictionary<int, List<PersistedIssue>>();
                    }
                }
            }
        }


        public string WriteIssues(Dictionary<int, List<PersistedIssue>> issues)
        {
            string s;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, issues);
                ms.Position = 0;
                byte[] buffer = new byte[(int)ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                s = Convert.ToBase64String(buffer);
            }

            return s;
        }

        public Dictionary<int, int> ReadIssueCounts(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new Dictionary<int, int>();

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(data)))
            {
                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    return (Dictionary<int, int>)bf.Deserialize(ms);
                }
                catch (InvalidCastException)
                {
                    try
                    {
                        ms.Position = 0;
                        int issueCount = (int)bf.Deserialize(ms);
                        return new Dictionary<int, int>() { { 0, issueCount } };
                    }
                    catch (InvalidCastException)
                    {
                        return new Dictionary<int, int>() { { 0, 1 } };
                    }
                }
            }
        }


        public string WriteIssueCounts(Dictionary<int, int> issueCounts)
        {
            string s;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, issueCounts);
                ms.Position = 0;
                byte[] buffer = new byte[(int)ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                s = Convert.ToBase64String(buffer);
            }

            return s;
        }

        public Dictionary<int, string> ReadTabNames(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(data)))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return (Dictionary<int, string>)bf.Deserialize(ms);
            }
        }


        public string WriteTabNames(Dictionary<int, string> tabNames)
        {
            string s;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, tabNames);
                ms.Position = 0;
                byte[] buffer = new byte[(int)ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                s = Convert.ToBase64String(buffer);
            }

            return s;
        }
        #endregion


        #region private methods
        private Settings()
        {
            this.PersistedIssues = new Dictionary<int, List<PersistedIssue>>();
            this.IssueCounts = new Dictionary<int, int>();
            this.TabNames = new Dictionary<int, string>();
        }
        #endregion


        private Object _writeLock = new Object(); 
    }
}
