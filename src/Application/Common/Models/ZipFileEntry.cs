using System;
using System.Collections.Generic;
using System.Text;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Models
{
    public class ZipFileEntry
    {
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
    }
}
