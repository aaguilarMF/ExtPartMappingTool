using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExtPartMappingTool.Models.Response
{
    [DataContract]
    public class Response
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string JSON_RESPONSE_DATA { get; set; }
        [DataMember]
        public string Message { get; set; }
    }
}