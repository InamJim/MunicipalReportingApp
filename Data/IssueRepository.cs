using System.Collections.Generic;
using MunicipalReportingApp.Models;

namespace MunicipalReportingApp.Data
{

    public static class IssueRepository
    {
        private static readonly List<Issue> _issues = new List<Issue>();

        public static IReadOnlyList<Issue> Issues
        {
            get { return _issues.AsReadOnly(); }
        }
        public static int Count
        {
            get { return _issues.Count; }
        }

        public static Issue Add ( Issue issue )
        {
            issue.Id = _issues.Count + 1;
            _issues.Add(issue);
            return issue;
        }
    }
}
