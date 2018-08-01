using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExtPartMappingTool.Controllers.Helpers
{
    public static class ProcessFilePipelineTracker
    {
        public static bool InitiateFiles(List<String> acesFileNames, List<String> piesFileNames, bool isStaging )
        {
            try
            {
                var context = new Magnaflow_WebEntities();
                var processFilesBatch = new EPM_ProcessFilesBatch()
                {
                    StageId = isStaging? Models.Types.ProcessFilesStages.StagingInit: Models.Types.ProcessFilesStages.ProductionInit,
                    Active = true
                };
                context.EPM_ProcessFilesBatch.Add(processFilesBatch);
                context.SaveChanges();

                foreach(var fileName in acesFileNames)
                {
                    if (!String.IsNullOrWhiteSpace(fileName))
                    {
                        var acesFile = new EPM_ProcessFilesDetail()
                        {
                            BatchId = processFilesBatch.BatchId,
                            FileName = fileName
                        };
                        context.EPM_ProcessFilesDetail.Add(acesFile);
                    }
                }
                
                foreach(var fileName in piesFileNames)
                {
                    if (!String.IsNullOrWhiteSpace(fileName))
                    {
                        var piesFile = new EPM_ProcessFilesDetail()
                        {
                            BatchId = processFilesBatch.BatchId,
                            FileName = fileName
                        };
                        context.EPM_ProcessFilesDetail.Add(piesFile);
                    }
                }
                context.SaveChanges();
                
                return true;

            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static bool ImportComplete(int batchId, bool isStaging)
        {
            try
            {
                var context = new Magnaflow_WebEntities();
                var activeFileBatch = context.EPM_ProcessFilesBatch.Where(x => x.BatchId == batchId).FirstOrDefault();

                if(activeFileBatch == null)
                {
                    return false;
                }
                activeFileBatch.StageId  = isStaging? Models.Types.ProcessFilesStages.StagingComplete: Models.Types.ProcessFilesStages.ProductionComplete;
                context.SaveChanges();
            }
            catch(Exception ex)
            {

            }
            return true;
        }

        public static bool ProductionInit(int batchId)
        {
            try
            {
                var context = new Magnaflow_WebEntities();
                var activeFileBatch = context.EPM_ProcessFilesBatch.Where(x => x.BatchId == batchId).FirstOrDefault();

                if (activeFileBatch == null)
                {
                    return false;
                }
                activeFileBatch.StageId = Models.Types.ProcessFilesStages.ProductionInit;
                context.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            return true;
        }
        public static bool ClosePipeline(int batchId)
        {
            try
            {
                var context = new Magnaflow_WebEntities();
                var activeFileBatch = context.EPM_ProcessFilesBatch.Where(x => x.BatchId == batchId).FirstOrDefault();

                if (activeFileBatch == null)
                {
                    return false;
                }
                activeFileBatch.Active = false;
                context.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            return true;
        }
    }
}