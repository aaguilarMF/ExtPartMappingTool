using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExtPartMappingTool.Models.ProcessFiles.Response
{
    [DataContract]
    public class StagingDirectoryContent
    {
        [DataMember]
        public List<String> FileNamesAces { get; set; }
        [DataMember]
        public List<String> FileNamesPies { get; set; }
        [DataMember]
        public List<String> FileNamesComplete { get; set; }

        public StagingDirectoryContent()
        {
            FileNamesAces = new List<string>();
            FileNamesPies = new List<string>();
            FileNamesComplete = new List<string>();
        }
    }
}
