using System;

namespace MunicipalReportingApp.Models
{
    public class LocalEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public bool IsAnnouncement { get; set; }
        public int Priority { get; set; }

        public override string ToString ()
        {
            return string.Format("{0:ddd, dd MMM yyyy}  |  {1}", EventDate, Title);
        }
    }
}
