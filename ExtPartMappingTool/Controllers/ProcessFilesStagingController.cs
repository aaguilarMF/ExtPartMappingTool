using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Http;

namespace ExtPartMappingTool.Controllers
{
    [RoutePrefix("api/ProcessFiles/Staging")]
    public class ProcessFilesStagingController : ApiController
    {
        private const String _Aces = "Aces";
        private const String _Pies = "Pies";
        private const String _Complete = "Complete";
        private const String _BAT = "BAT";
        private const String _ImportAcesBat = "import_aces.bat";
        private const String _ImportPiesBat = "import_pies.bat";
        private const String _ImportMotorcycleBat = "import_motorcycle.bat";
        private const String _MoveToProduction = "move_to_prod.bat";
        private const String _ImportAcesArgs = "import_motorcycle";// "import_aces";
        private const String _MagnaflowScriptsExecutableName = "MagnaFlow.Scripts.exe";


        [Route("~/api/ProcessFiles/PipelineStatus")]
        [HttpGet]
        public IHttpActionResult PipelineStatus()
        {
            try
            {
                Models.ProcessFiles.Response.PipelineStatus pipelineStatus = CheckPipelineStatus();

                return Ok(pipelineStatus);

            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "> From PipelineStatus: "+ ((HttpResponseException)ex).Response.ReasonPhrase
                });
            }
        }

        private static Models.ProcessFiles.Response.PipelineStatus CheckPipelineStatus()
        {
            try
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
            catch (Exception ex)
            {

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "> from CheckPipelineStatus: "+ex.Message
                });
            }
        }

        [Route("AcesPiesContents")]
        [HttpGet]
        public IHttpActionResult AcesPiesContents()
        {
            try
            {
                var stagingFilesContents = new ExtPartMappingTool.Models.ProcessFiles.Response.StagingDirectoryContent();
                Models.ProcessFiles.Response.PipelineStatus pipelineStatus;

                DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileData());

                //Individual Aces and Pies Directory
                DirectoryInfo directoryAces = stagingDirectoryInfo.CreateSubdirectory(_Aces);
                DirectoryInfo directoryPies = stagingDirectoryInfo.CreateSubdirectory(_Pies);


                if (AcesPiesIsEmptyCheck(true))
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        ReasonPhrase = "Pies and Aces in Staging are both empty"
                    });
                }

                //Get Name Files/Contents.
                var fileNamesAces = directoryAces.EnumerateFiles().Select(x => x.Name).ToList();
                var fileNamesPies = directoryPies.EnumerateFiles().Select(x => x.Name).ToList();

                stagingFilesContents.FileNamesAces = fileNamesAces;
                stagingFilesContents.FileNamesPies = fileNamesPies;


                pipelineStatus = CheckPipelineStatus();
                if (pipelineStatus.Active == false)
                {
                    if (!Helpers.ProcessFilePipelineTracker.InitiateFiles(fileNamesAces, fileNamesPies, true))
                    {
                        //if not successfull then we have no record and should not continue.
                        throw new Exception();
                    }
                }
                else if (pipelineStatus.ActiveStage != Models.Types.ProcessFilesStages.StagingInit)
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        ReasonPhrase = "Process pipeline is already in use."
                    });
                }
                //before returning, initiate record of a pipline process in db


                return Ok(stagingFilesContents);
                /*
                NetworkCredential NCredentials = GetNetworkCredential();
                using (new NetworkConnection(GetFileStoreHost(), NCredentials))
                {
                    //The Host Directroy
                    DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileStoreHost());

                    //Individual Aces and Pies Directory
                    DirectoryInfo directoryAces = stagingDirectoryInfo.CreateSubdirectory(_Aces);
                    DirectoryInfo directoryPies = stagingDirectoryInfo.CreateSubdirectory(_Pies);


                    if (AcesPiesIsEmptyCheck(true))
                    {
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            ReasonPhrase = "Pies and Aces in Staging are both empty"
                        });
                    }

                    //Get Name Files/Contents.
                    var fileNamesAces = directoryAces.EnumerateFiles().Select(x => x.Name).ToList();
                    var fileNamesPies = directoryPies.EnumerateFiles().Select(x => x.Name).ToList();

                    stagingFilesContents.FileNamesAces = fileNamesAces;
                    stagingFilesContents.FileNamesPies = fileNamesPies;


                    pipelineStatus = CheckPipelineStatus();
                    if(pipelineStatus.Active == false)
                    {
                        if (!Helpers.ProcessFilePipelineTracker.InitiateFiles(fileNamesAces, fileNamesPies, true))
                        {
                            //if not successfull then we have no record and should not continue.
                            throw new Exception();
                        }
                    }
                    else if(pipelineStatus.ActiveStage != Models.Types.ProcessFilesStages.StagingInit)
                    {
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            ReasonPhrase = "Process pipeline is already in use."
                        });
                    }
                    //before returning, initiate record of a pipline process in db
                    

                    return Ok(stagingFilesContents);
                }*/
            }
            catch(System.ComponentModel.Win32Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "> from AcesPiesContents: " + ex.Message
                });
            }
            catch (HttpResponseException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "> from AcesPiesContents: " + ex.Response.ReasonPhrase
                }); //throw;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "> from AcesPiesContents: " + ex.Message
                }); //throw;
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
                AssertStep(pipelineStatus, Models.Types.ProcessFilesStages.StagingComplete);
                NetworkCredential NCredentials = GetNetworkCredential();
                using (new NetworkConnection(GetFileData(), NCredentials))
                {
                    //The Host Directroy
                    DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileData());

                    //Individual Aces and Pies Directory
                    DirectoryInfo directoryComplete = stagingDirectoryInfo.CreateSubdirectory(_Complete);

                    //find files that are currently active
                    var namesOfActiveFiles = context.EPM_ProcessFilesDetail.Where(x => x.BatchId == pipelineStatus.BatchId).Select(x => x.FileName).ToList();


                    //Get Name Files/Contents.
                    var matchingNamesOfActiveFiles = directoryComplete.EnumerateFiles().Where(x => namesOfActiveFiles.Contains( x.Name)).Select(x => x.Name).ToList();
                    //var fileNamesPies = directoryPies.EnumerateFiles().Select(x => x.Name).ToList();

                    //stagingFilesContents.FileNamesAces = fileNamesAces;
                    //stagingFilesContents.FileNamesPies = fileNamesPies;
                    stagingFilesContents.FileNamesComplete = matchingNamesOfActiveFiles;

                    return Ok(stagingFilesContents);
                }
            }
            catch (HttpResponseException ex)
            {
                throw new Exception("From ccomplete contents"); 
            }
            catch (Exception ex)
            {
                throw new Exception("From ccomplete contents");
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
                AssertStep(pipelineStatus, Models.Types.ProcessFilesStages.StagingInit);
                NetworkCredential NCredentials = GetNetworkCredential();

                DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileData());
                //used for testing
                //var importAcesSuccess = Import(stagingDirectoryInfo, _ImportMotorcycleBat);

                var importAcesSuccess = Import(stagingDirectoryInfo, _ImportAcesBat);
                var importPiesSuccess = Import(stagingDirectoryInfo, _ImportPiesBat);
                //    //if (!importAcesSuccess)
                //    //{
                //    //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //    //    {
                //    //        ReasonPhrase = "Aces import process failed"
                //    //    });
                //    //}
                //    //if (!importPiesSuccess)
                //    //{
                //    //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //    //    {
                //    //        ReasonPhrase = "Pies import process failed"
                //    //    });
                //    //}
                //    Helpers.ProcessFilePipelineTracker.ImportComplete(pipelineStatus.BatchId, true);


                //var importPiesSuccess = Import(stagingDirectoryInfo, _ImportPiesBat);
                //DirectoryInfo debugFolder = Directory.CreateDirectory("D:\\Import\\Staging\\Scripts\\magnaflow_staging\\debug");
                //var path = debugFolder.FullName + "\\logErr.txt";
                //if (File.Exists(path))
                //{
                //    File.Delete(path);
                //}

                //// Create the file.
                //using (FileStream fs = File.Create(path))
                //{
                //    Byte[] info = new UTF8Encoding(true).GetBytes(importAcesSuccess);
                //    // Add some information to the file.
                //    fs.Write(info, 0, info.Length);
                //}


                //using (new NetworkConnection(GetFileStoreHostForImporting(), NCredentials))
                //{
                //    //The Host Directroy
                //    DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileStoreHostForImporting());

                //    //Individual Aces and Pies Directory
                //    //var impoartMotorcycleSuccess = Import(stagingDirectoryInfo, _ImportPiesBat);
                //    //var importAcesSuccess = Import(stagingDirectoryInfo, _ImportAcesBat );
                //    //var importPiesSuccess = Import(stagingDirectoryInfo, _ImportPiesBat);


                //    var importAcesSuccess = Import(stagingDirectoryInfo, _MagnaflowScriptsExecutableName, _ImportAcesArgs);
                //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //    {
                //        ReasonPhrase = importAcesSuccess
                //    });
                //    //if (!importAcesSuccess)
                //    //{
                //    //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //    //    {
                //    //        ReasonPhrase = "Aces import process failed"
                //    //    });
                //    //}
                //    //if (!importPiesSuccess)
                //    //{
                //    //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //    //    {
                //    //        ReasonPhrase = "Pies import process failed"
                //    //    });
                //    //}
                //    Helpers.ProcessFilePipelineTracker.ImportComplete(pipelineStatus.BatchId, true);

                //}
                return Ok(true);
            }
            catch (HttpResponseException ex)
            {
                throw ex;
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
                Models.ProcessFiles.Response.PipelineStatus pipelineStatus = CheckPipelineStatus();
                AssertStep(pipelineStatus, Models.Types.ProcessFilesStages.StagingComplete);
                NetworkCredential NCredentials = GetNetworkCredential();
                //once these checks are complete then send run batch files to send files to prod
                using (new NetworkConnection(GetFileData(), NCredentials))
                {
                    //The Host Directroy
                    DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileData());

                    //Individual Aces and Pies Directory
                    var sendToProduction = Import(stagingDirectoryInfo, _MoveToProduction);

                    //if (!sendToProduction)
                    //{
                    //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    //    {
                    //        ReasonPhrase = "Aces import process failed"
                    //    });
                    //}
                    //if (!importPiesSuccess)
                    //{
                    //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    //    {
                    //        ReasonPhrase = "Pies import process failed"
                    //    });
                    //}
                    Helpers.ProcessFilePipelineTracker.ImportComplete(pipelineStatus.BatchId, true);

                }

                //when that's done then move batch file step forward to prod init
                Helpers.ProcessFilePipelineTracker.ProductionInit(pipelineStatus.BatchId);
                return Ok(true);
                
            }catch(HttpResponseException ex)
            {
                throw;
            }catch(Exception ex)
            {
                throw;
            }
        }

        //private static string ImportOld(DirectoryInfo stagingDirectoryInfo, String batFileToRun)
        //{
        //    try
        //    {
        //        DirectoryInfo directoryBAT = stagingDirectoryInfo.CreateSubdirectory(_BAT);

        //        var importBatFile = directoryBAT.EnumerateFiles().Where(x => x.Name == batFileToRun).Select(x => x.FullName).FirstOrDefault();

        //        if (importBatFile == null)
        //        {
        //            if (batFileToRun == _ImportAcesBat)
        //            {
        //                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        //                {
        //                    ReasonPhrase = "Did not find .bat file to import Aces"
        //                });
        //            }
        //            else
        //            {
        //                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        //                {
        //                    ReasonPhrase = "Did not find .bat file to import Pies"
        //                });
        //            }

        //        }

        //        //launch process
        //        ProcessStartInfo info = new ProcessStartInfo(importBatFile);
        //        info.UseShellExecute = false;
        //        info.RedirectStandardOutput = true;
        //        info.RedirectStandardInput = false;
        //        info.CreateNoWindow = true;
        //        var proc = Process.Start(info);
        //        string log = "";
        //        while (!proc.StandardOutput.EndOfStream)
        //        {
        //            bool result;
        //            string line = proc.StandardOutput.ReadLine();
        //            log += line;
        //            //Boolean.TryParse(line, out result);
        //            //if (result)
        //            //{
        //            //    return "true";// return true;
        //            //}
        //            continue;
        //            // do something with line
        //        }
        //        return log;// return false;
        //    }catch(HttpResponseException ex)
        //    {
        //        throw ex;
        //    }
        //    catch (Exception)
        //    {

        //        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        //        {
        //            ReasonPhrase = "something went wrong in import file thing."
        //        });
        //    }
        //}
        private static bool Import(DirectoryInfo stagingDirectoryInfo, String batFileToRun)
        {
            try
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
                string log = "";
                int timeout = 10; //minutes
                string errorMessage = null;
                string outputMessage;

                ProcessStartInfo info = new ProcessStartInfo(importBatFile);
                //info.Arguments = args;

                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;

                Process proc = new Process { StartInfo = info };

                proc.Start();

                proc.WaitForExit
                (
                    (timeout <= 0)
                        ? int.MaxValue : (int)TimeSpan.FromMinutes(timeout).TotalMilliseconds
                );


                errorMessage = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                outputMessage = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                

                //var guat = outputMessage.Trim('\r', '\n');

                //if(!guat.Contains("true"))
                //{
                //    log += "errormessage: " + errorMessage + "outputmessage: " + outputMessage;
                //    WriteToAppDebugFile(log);
                //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                //    {
                //        ReasonPhrase = "Error running Bat file. Check log"
                //    });
                //}

                return true;
            }
            catch (HttpResponseException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "something went wrong in import file thing."
                });
            }
        }

        private static void WriteToAppDebugFile(String message)
        {
            DirectoryInfo debugFolder = Directory.CreateDirectory(GetDebugFile());
            var path = debugFolder.FullName + "\\AppLogErr.txt";

            // Create the file.
            using (FileStream fs = File.Create(path))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(DateTime.Now.ToString() + ": " + message);
                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }
        }

        private static string Import(DirectoryInfo stagingDirectoryInfo, String executableName, String args)
        {
            try
            {
                var executable = stagingDirectoryInfo.EnumerateFiles().Where(x => x.Name == executableName).Select(x => x.FullName).FirstOrDefault();
                var names = stagingDirectoryInfo.EnumerateFiles().Select(x => x.FullName).ToList();
                //var executable = stagingDirectoryInfo.EnumerateFiles().Where(x => x.Name == executableName).Select(x => x.FullName).FirstOrDefault();

                //var thing = "";
                //foreach(var name in names)
                //{
                //    thing += name + " ";
                //}
                if(executable == null)
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        ReasonPhrase = "Did not find .exe file to import >"
                    });

                }
                string log = "what follows:   >";
                int timeout = 10; //minutes
                string errorMessage;
                string outputMessage;

                ProcessStartInfo info = new ProcessStartInfo(executable);
                info.Arguments = args;

                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;

                Process proc = new Process { StartInfo = info };

                proc.Start();

                proc.WaitForExit
                (
                    (timeout <= 0)
                        ? int.MaxValue : (int)TimeSpan.FromMinutes(timeout).TotalMilliseconds
                );


                errorMessage = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                outputMessage = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                log += "errormessage: " + errorMessage + "outputmessage: " + outputMessage;
                ////launch process
                //ProcessStartInfo info = new ProcessStartInfo(executable);
                //info.UseShellExecute = false;
                ////info.RedirectStandardOutput = true;
                ////info.RedirectStandardInput = false;
                //info.CreateNoWindow = true;
                //info.Arguments = args;
                //info.WindowStyle = ProcessWindowStyle.Hidden;

                //string log = "what follows:   >";
                //try
                //{
                //    // Start the process with the info we specified.
                //    // Call WaitForExit and then the using-statement will close.
                //    using (Process exeProcess = Process.Start(info))
                //    {
                //        exeProcess.WaitForExit();
                //    }
                //    log += "passed";
                //}
                //catch
                //{
                //    log += "failed";
                //    // Log error.
                //}

                //var proc = Process.Start(info);
                //string log = "what follows:   >";
                //while (!proc.StandardOutput.EndOfStream)
                //{
                //    //bool result;
                //    string line = proc.StandardOutput.ReadLine();
                //    log += line;
                //    //Boolean.TryParse(line, out result);
                //    //if (result)
                //    //{
                //    //    return "true";// return true;
                //    //}
                //    continue;
                //    // do something with line
                //}
                return log;// return false;
            }
            catch (HttpResponseException ex)
            {
                throw ex;
            }
            catch (Exception)
            {

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "something went wrong in import file thing."
                });
            }
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
            catch( Exception ex)
            {
                throw new Exception("From accesPiesIsEmpty"); //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        /// <summary>
        /// Checks that the Aces and Pies files are empty
        /// </summary>
        /// <returns></returns>
        private bool AcesPiesIsEmptyCheck(bool networkConnectionIsOpen = false)
        {
            try
            {
                NetworkCredential NCredentials = GetNetworkCredential();
                if (!networkConnectionIsOpen)
                {
                    using (new NetworkConnection(GetFileData(), NCredentials))
                    {
                        DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileData());

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
                else
                {
                    DirectoryInfo stagingDirectoryInfo = Directory.CreateDirectory(GetFileData());

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
                throw new Exception("From accesPiesIsEmptyCheck"); //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }



        #region HELPERS
        private static string GetFileData()
        {
            // From here get ServerLogin from the webconfig file
            return ConfigurationManager.AppSettings.Get("StageWeb_Data");
        }
        private static string GetDebugFile()
        {
            // From here get ServerLogin from the webconfig file
            return ConfigurationManager.AppSettings.Get("StageWeb_AppDebug");
        }
        

        private static NetworkCredential GetNetworkCredential()
        {
            // From here get ServerLogin_blahblah from the webconfig file
            return new NetworkCredential(ConfigurationManager.AppSettings.Get("StageWeb_SQLDATA_UserName"), ConfigurationManager.AppSettings.Get("StageWeb_SQLDATA_PassWord"));
        }
        #endregion
    }
}
