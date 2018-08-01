using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExtPartMappingTool.Models.ProcessFiles.Response
{
    [DataContract]
    public class PipelineStatus
    {
        /// <summary>
        /// If true there exists a batch in the processing files pipeline that is currently active,
        /// false, nothing in the pipline exists
        /// </summary>
        [DataMember]
        public bool Active { get; set; }

        /// <summary>
        /// This field only matters if Active is true. And this field stores the stage that the current files are on standby
        /// in the pipeline.
        /// </summary>
        [DataMember]
        public Types.ProcessFilesStages ActiveStage { get; set; }

        [DataMember]
        public int BatchId { get; set; }
    }
}
