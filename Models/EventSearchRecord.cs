using System;

namespace MunicipalReportingApp.Models
{
    public class EventSearchRecord
    {
        public DateTime SearchedAt { get; set; }
        public string Keyword { get; set; }
        public string Category { get; set; }
        public bool UsedDateRange { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ResultCount { get; set; }

        public override string ToString ()
        {
            string term = string.IsNullOrWhiteSpace(Keyword) ? "All events" : "\"" + Keyword.Trim() + "\"";
            if ( !string.IsNullOrWhiteSpace(Category) && !string.Equals(Category, "All categories", StringComparison.OrdinalIgnoreCase) )
                term += " · " + Category;
            if ( UsedDateRange )
                term += string.Format(" · {0:dd MMM}–{1:dd MMM yyyy}", StartDate, EndDate);
            return string.Format("{0:HH:mm}  {1}  ({2} found)", SearchedAt, term, ResultCount);
        }
    }
}
