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
using System.Collections.Generic;

namespace StopWatch
{
    internal class IssueFields
    {
        public string Summary { get; set; }
        public TimetrackingFields Timetracking { get; set; }
        public ProjectFields Project { get; set; }
        public StatusFields Status { get; set; }
        public IssueTypeFields IssueType { get; set; }
        public PriorityFields Priority { get; set; }
    }

    internal class TimetrackingFields
    {
        public string RemainingEstimate { get; set; }
        public int RemainingEstimateSeconds { get; set; }
    }

    internal class ProjectFields
    {
        public string Name { get; set; } 
        public string Key { get; set; }
        public Dictionary<string, string> AvatarUrls { get; set; }
    }

    internal class StatusFields
    {
        public string Name { get; set; }
        public string IconUrl { get; set; }
        public string Description { get; set; }
    }

    internal class IssueTypeFields
    {
        public string Name { get; set; }
        public bool Subtask { get; set; }
        public string IconUrl { get; set; }
        public string Description { get; set; }
    }

    public class PriorityFields
    {
        public string Name { get; set; }
        public string IconUrl { get; set; }
    }
}
