using System;
using System.Collections.Generic;

namespace MunicipalReportingApp.Models
{
    public class Issue
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<string> AttachmentPaths { get; set; }
        public DateTime DateReported { get; set; }
        public string Status { get; set; }
        public Issue ()
        {
            AttachmentPaths = new List<string>();
            DateReported = DateTime.Now;
            Status = "Received";
        }
        public override string ToString ()
        {
            return string.Format(
                "Report #{0} | {1} | {2} | Reported: {3:dd MMM yyyy HH:mm} | Status: {4}",
                Id, Category, Location, DateReported, Status);
        }
    }
}
