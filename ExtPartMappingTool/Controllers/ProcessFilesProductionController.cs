using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ExtPartMappingTool.Controllers
{
    [RoutePrefix("api/ProcessFiles/Production")]
    public class ProcessFilesProductionController : ApiController
    {
        private const String _Aces = "Aces";
        private const String _Pies = "Pies";
        private const String _Complete = "Complete";
        private const String _BAT = "BAT";
        private const String _ImportAcesBat = "testBatchFileImportAces.bat";
        private const String _ImportPiesBat = "testBatchFileImportPies.bat";


        private static Models.ProcessFiles.Response.PipelineStatus CheckPipelineStatus()
        {
            var context = new Magnaflow_WebEntities();
            Models.ProcessFiles.Response.PipelineStatus pipelineStatus = new Models.ProcessFiles.Response.PipelineStatus();
            var activeFileBatch = context.EPM_ProcessFilesBatch.Where(x => x.Active == true).FirstOrDefault();

            if (activeFileBatch == null)
            {
                pipelineStatus.Active = false;
            }
            else
            {
                pipelineStatus.Active = true;
                pipelineStatus.ActiveStage = activeFileBatch.StageId;
                pipelineStatus.BatchId = activeFileBatch.BatchId;
            }

            return pipelineStatus;
        }

        [Route("AcesPiesContents")]
        [HttpGet]
        public IHttpActionResult AcesPiesContents()
        {
            try
            {
                var stagingFilesContents = new ExtPartMappingTool.Models.ProcessFiles.Response.StagingDirectoryContent();
                Models.ProcessFiles.Response.PipelineStatus pipelineStatus;
                NetworkCredential NCredentials = GetNetworkCredential();
                using (new NetworkConnection(GetFileStoreHost(), NCredentials))
                {
                    //The Host Directroy
                    DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileStoreHost());

                    //Individual Aces and Pies Directory
                    DirectoryInfo directoryAces = stagingDirectoryInfo.CreateSubdirectory(_Aces);
                    DirectoryInfo directoryPies = stagingDirectoryInfo.CreateSubdirectory(_Pies);


                    if (AcesPiesIsEmptyCheck())
                    {
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            ReasonPhrase = "Pies and Aces in Production are both empty"
                        });
                    }

                    //Get Name Files/Contents.
                    var fileNamesAces = directoryAces.EnumerateFiles().Select(x => x.Name).ToList();
                    var fileNamesPies = directoryPies.EnumerateFiles().Select(x => x.Name).ToList();

                    stagingFilesContents.FileNamesAces = fileNamesAces;
                    stagingFilesContents.FileNamesPies = fileNamesPies;


                    pipelineStatus = CheckPipelineStatus();
                    AssertStep(pipelineStatus, Models.Types.ProcessFilesStages.ProductionInit);

                    return Ok(stagingFilesContents);
                }
            }
            catch (HttpResponseException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Route("CompleteContents")]
        [HttpGet]
        public IHttpActionResult CompleteContents()
        {
            try
            {
                var context = new Magnaflow_WebEntities();
                var stagingFilesContents = new Models.ProcessFiles.Response.StagingDirectoryContent();
                Models.ProcessFiles.Response.PipelineStatus pipelineStatus = CheckPipelineStatus();
                AssertStep(pipelineStatus, Models.Types.ProcessFilesStages.ProductionComplete);
                NetworkCredential NCredentials = GetNetworkCredential();
                using (new NetworkConnection(GetFileStoreHost(), NCredentials))
                {
                    //The Host Directroy
                    DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileStoreHost());

                    //Individual Aces and Pies Directory
                    DirectoryInfo directoryComplete = stagingDirectoryInfo.CreateSubdirectory(_Complete);

                    //find files that are currently active
                    var namesOfActiveFiles = context.EPM_ProcessFilesDetail.Where(x => x.BatchId == pipelineStatus.BatchId).Select(x => x.FileName).ToList();


                    //Get Name Files/Contents.
                    var matchingNamesOfActiveFiles = directoryComplete.EnumerateFiles().Where(x => namesOfActiveFiles.Contains(x.Name)).Select(x => x.Name).ToList();
                    //var fileNamesPies = directoryPies.EnumerateFiles().Select(x => x.Name).ToList();

                    //stagingFilesContents.FileNamesAces = fileNamesAces;
                    //stagingFilesContents.FileNamesPies = fileNamesPies;
                    stagingFilesContents.FileNamesComplete = matchingNamesOfActiveFiles;

                    //and before closing, release the pipeline
                    Helpers.ProcessFilePipelineTracker.ClosePipeline(pipelineStatus.BatchId);

                    return Ok(stagingFilesContents);
                }
            }
            catch (HttpResponseException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static void AssertStep(Models.ProcessFiles.Response.PipelineStatus pipelineStatus, Models.Types.ProcessFilesStages expectedStage)
        {
            if (pipelineStatus.Active == false)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "Pipeline is empty. Start by uploading files to Aces/Pies."
                });
            }
            else if (pipelineStatus.ActiveStage != expectedStage)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Process pipeline is already in use at another stage."
                });
            }
        }

        [Route("Import")]
        [HttpGet]
        public IHttpActionResult Import()
        {
            try
            {
                Models.ProcessFiles.Response.PipelineStatus pipelineStatus = CheckPipelineStatus();
                AssertStep(pipelineStatus, Models.Types.ProcessFilesStages.ProductionInit);
                NetworkCredential NCredentials = GetNetworkCredential();
                using (new NetworkConnection(GetFileStoreHost(), NCredentials))
                {
                    //The Host Directroy
                    DirectoryInfo productionDirectoryInfo = Directory.CreateDirectory(GetFileStoreHost());

                    //Individual Aces and Pies Directory
                    var importAcesSuccess = Import(productionDirectoryInfo, _ImportAcesBat); //gets hung up here for some time 
                    var importPiesSuccess = Import(productionDirectoryInfo, _ImportPiesBat); // gets hung up here too. 

                    if (!importAcesSuccess)
                    {
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                        {
                            ReasonPhrase = "Aces import process failed"
                        });
                    }
                    if (!importPiesSuccess)
                    {
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                        {
                            ReasonPhrase = "Pies import process failed"
                        });
                    }
                    Helpers.ProcessFilePipelineTracker.ImportComplete(pipelineStatus.BatchId, false);

                }
                return Ok(true);
            }
            catch (HttpResponseException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        [Route("SendToProduction")]
        [HttpGet]
        public IHttpActionResult SendToProduction()
        {
            try
            {
                return Ok(true);

            }
            catch (HttpResponseException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static bool Import(DirectoryInfo stagingDirectoryInfo, String batFileToRun)
        {
            DirectoryInfo directoryBAT = stagingDirectoryInfo.CreateSubdirectory(_BAT);

            var importBatFile = directoryBAT.EnumerateFiles().Where(x => x.Name == batFileToRun).Select(x => x.FullName).FirstOrDefault();

            if (importBatFile == null)
            {
                if (batFileToRun == _ImportAcesBat)
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        ReasonPhrase = "Did not find .bat file to import Aces"
                    });
                }
                else
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        ReasonPhrase = "Did not find .bat file to import Pies"
                    });
                }

            }

            //launch process
            ProcessStartInfo info = new ProcessStartInfo(importBatFile);
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardInput = true;
            info.CreateNoWindow = true;
            var proc = Process.Start(info);
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (Boolean.Parse(line))
                {
                    return true;
                }
                continue;
                // do something with line
            }
            return false;
        }



        /// <summary>
        /// Checks that the Aces and Pies files are empty
        /// </summary>
        /// <returns></returns>
        [Route("AcesPiesIsEmpty")]
        [HttpGet]
        public IHttpActionResult AcesPiesIsEmpty()
        {
            try
            {
                return Ok(AcesPiesIsEmptyCheck());

            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Checks that the Aces and Pies files are empty
        /// </summary>
        /// <returns></returns>
        private bool AcesPiesIsEmptyCheck()
        {
            try
            {
                NetworkCredential NCredentials = GetNetworkCredential();
                using (new NetworkConnection(GetFileStoreHost(), NCredentials))
                {
                    //The Host Directroy
                    DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileStoreHost());

                    //Individual Aces and Pies Directory
                    DirectoryInfo directoryAces = stagingDirectoryInfo.CreateSubdirectory(_Aces);
                    DirectoryInfo directoryPies = stagingDirectoryInfo.CreateSubdirectory(_Pies);

                    var fileCountAces = directoryAces.EnumerateFiles().Count();
                    var fileCountPies = directoryPies.EnumerateFiles().Count();

                    if (fileCountAces > 0 || fileCountPies > 0)
                    {
                        return false;
                    }
                    return true;
                }

            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }



        #region HELPERS
        private static string GetFileStoreHost()
        {
            return ConfigurationManager.AppSettings.Get("ProdWeb_SQLDATA_StoreHost");
        }

        private static NetworkCredential GetNetworkCredential()
        {
            return new NetworkCredential(ConfigurationManager.AppSettings.Get("ProdWeb_SQLDATA_UserName"), ConfigurationManager.AppSettings.Get("ProdWeb_SQLDATA_PassWord"));
        }
        #endregion
    }
}
