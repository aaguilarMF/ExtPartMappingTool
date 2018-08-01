using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtPartMappingTool.Models
{
    public class Types
    {

        public enum ProcessFilesStages
        {
            StagingInit = 1,
            StagingComplete = 2,
            ProductionInit = 3,
            ProductionComplete = 4
        }
    }
}
