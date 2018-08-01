using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExtPartMappingTool.Models.Request.NestedViews
{
    public class Save
    {
        public int? Id { get; set; }
        public string OldPartId { get; set; }
        public string NewPartId { get; set; }
    }
}