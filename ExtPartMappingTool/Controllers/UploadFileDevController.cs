using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using OfficeOpenXml;
using Newtonsoft.Json;

namespace ExtPartMappingTool.Controllers
{
    [Authorize(Roles = "DataTeam_APPS_RW, DataTeam_APPS_RO, APP_ADMINS")]
    public class UploadFileDevController : Controller
    {
        // GET: UploadFile
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// upoloads but doesn' commit
        /// LOGS UPLOAD
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "DataTeam_APPS_RW, APP_ADMINS")]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            var context = new Magnaflow_WebEntities();
            var instantiateLogEntry = true;
            int? stagingActivityId = null;
            Models.Response.Response response = null;
            List<ExtPartMappingToolUploadStaging> uploadStagingList = new List<ExtPartMappingToolUploadStaging>();
            try
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    // get the first worksheet in the workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    for (int row = 1; worksheet.Cells[row, 1].Value != null; row++)
                    {
                        var oldPartId = worksheet.Cells[row, 1].Value;
                        var newPartId = worksheet.Cells[row, 2].Value;
                        #region log begin upload
                        if (instantiateLogEntry)
                        {
                            var logBeginUpload = new ExtPartMappingToolStagingActivity()
                            {
                                Time = DateTime.UtcNow,
                                UserId = User.Identity.Name,
                                ActionType = Types.ActionType.BEGIN_UPLOAD
                            };

                            context.ExtPartMappingToolStagingActivities.Add(logBeginUpload);
                            context.SaveChanges();
                            stagingActivityId = logBeginUpload.StagingActivityId;
                            instantiateLogEntry = false;
                        }
                        #endregion

                        /* add to upload staging */
                        uploadStagingList.Add(new ExtPartMappingToolUploadStaging()
                        {
                            StagingActiviyId = (int)stagingActivityId,
                            OldPartId = oldPartId.ToString(),
                            NewPartId = newPartId.ToString()
                        });
                    }
                    context.ExtPartMappingToolUploadStagings.AddRange(uploadStagingList);
                    /*foreach(var record in uploadStagingList)
                    {
                        context.ExtPartMappingToolUploadStagings.Add(record);
                    }*/

                    context.SaveChanges();
                } // the using 

                response = new Models.Response.Response()
                {
                    Success = true,
                    JSON_RESPONSE_DATA = JsonConvert.SerializeObject(uploadStagingList)
                };
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                response = new Models.Response.Response()
                {
                    Success = false,
                    JSON_RESPONSE_DATA = JsonConvert.SerializeObject("Internal_Error")
                };
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }
            
        }

        /// <summary>
        /// Commits
        /// LOGS COMMIT so like it's as good as an agreement
        /// </summary>
        /// <param name="stagingList"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "DataTeam_APPS_RW, APP_ADMINS")]
        public ActionResult Commit(List<ExtPartMappingToolUploadStaging> stagingList)
        {
            var context = new Magnaflow_WebEntities();
            Models.Response.Response response = null;
            try
            {
                foreach(var stagingRecord in stagingList)
                {
                    context.ExtPartMappings.Add(new ExtPartMapping()
                    {
                        OldPartId = stagingRecord.OldPartId,
                        NewPartId = stagingRecord.NewPartId
                    });
                }
                var logBeginUpload = new ExtPartMappingToolStagingActivity()
                {
                    Time = DateTime.UtcNow,
                    UserId = User.Identity.Name,
                    ActionType = Types.ActionType.COMMIT_UPLOAD
                };
                context.ExtPartMappingToolStagingActivities.Add(logBeginUpload);

                context.SaveChanges();

                response = new Models.Response.Response()
                {
                    Success = true,
                    JSON_RESPONSE_DATA = null// JsonConvert.SerializeObject(stagingList)
                };
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                response = new Models.Response.Response()
                {
                    Success = false,
                    JSON_RESPONSE_DATA = JsonConvert.SerializeObject("Internal_Error")
                };
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }

        }
    }
}