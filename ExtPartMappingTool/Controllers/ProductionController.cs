using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExtPartMappingTool.Controllers
{
    //[Authorize(Roles = "DataTeam_APPS_RW, DataTeam_APPS_RO, APP_ADMINS")]
    public class ProductionController : Controller
    {
        [HttpGet]
        public ActionResult ExtPartMappings()
        {
            var context = new Magnaflow_WebEntitiesProd();
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var extPartMappings = context.ExtPartMappings.ToList();
            Models.Response.Response response = null;

            if (extPartMappings == null || extPartMappings.Count() == 0)
            {

                //Used to make property name as camel case
                /*Microsoft.Data.OData.ODataError error = new Microsoft.Data.OData.ODataError()
                {
                    ErrorCode = "404",
                    Message = "Customer Not Fount"
                }
                return JsonConvert.SerializeObject(error, Formatting.None, settings);*/ //Returns students list as JSON
                response = new Models.Response.Response()
                {
                    Success = false,
                    JSON_RESPONSE_DATA = null,
                    Message = "Data set is empty"
                };
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }

            response = new Models.Response.Response()
            {
                Success = true,
                JSON_RESPONSE_DATA = JsonConvert.SerializeObject(extPartMappings)// JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings)
            };
            return this.Json(response, JsonRequestBehavior.AllowGet);
            //return JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings); //Returns students list as JSON
        }

        [HttpPost]
        [Authorize(Roles = "DataTeam_APPS_RW, APP_ADMINS")]
        public ActionResult Delete(ExtPartMapping entry)
        {
            var context = new Magnaflow_WebEntitiesProd();
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            Models.Response.Response response = null;

            /*
             * Do checks here making sure input is as expectied
             */

            try
            {
                var deleteEntry = context.ExtPartMappings.Where(x => x.Id == entry.Id).FirstOrDefault();

                if (deleteEntry != null)
                {
                    context.ExtPartMappings.Remove(deleteEntry);
                    context.SaveChanges();
                    response = new Models.Response.Response()
                    {
                        Success = true,
                        JSON_RESPONSE_DATA = null // JsonConvert.SerializeObject(commissionDataResponseObject)// JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings)
                    };
                    #region log activity
                    var logDelete = new ExtPartMappingToolStagingActivity()
                    {
                        Time = DateTime.UtcNow,
                        UserId = User.Identity.Name,
                        ActionType = Types.ActionType.DELETE
                    };
                    #endregion
                    context.ExtPartMappingToolStagingActivities.Add(logDelete);
                    context.SaveChanges();
                }
                else
                {
                    response = new Models.Response.Response()
                    {
                        Success = false,
                        JSON_RESPONSE_DATA = null, // JsonConvert.SerializeObject(commissionDataResponseObject)// JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings)
                        Message = "Data submitted to delete does not exist."
                    };
                }
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;//  new HttpException(404, "Internal error");
            }

            //return JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings); //Returns students list as JSON
        }

        /// <summary>
        /// Saves
        /// 
        /// </summary>
        /// <param name="saveModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "DataTeam_APPS_RW, APP_ADMINS")]
        public ActionResult Save(Models.Request.NestedViews.Save saveModel)
        {
            var context = new Magnaflow_WebEntitiesProd();
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            Models.Response.Response response = null;
            ExtPartMapping extPartEntry = null; //To do: need to change this with 'Mine' once we decide it's ok to change databases
            //Models.CommissionData commissionDataResponseObject = null;

            /*
             * Do checks here making sure input is as expectied
             */
            if (saveModel.Id != null)
            {
                extPartEntry = context.ExtPartMappings.Where(x => x.Id == saveModel.Id).FirstOrDefault();
                if (extPartEntry == null)
                {
                    response = new Models.Response.Response()
                    {
                        Success = false,
                        JSON_RESPONSE_DATA = null, //JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings)
                        Message = "Data to edit does not exist."
                    };
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    extPartEntry.OldPartId = saveModel.OldPartId;
                    extPartEntry.NewPartId = saveModel.NewPartId;
                    context.SaveChanges();
                    #region log save
                    var logUpdate = new ExtPartMappingToolStagingActivity()
                    {
                        Time = DateTime.UtcNow,
                        UserId = User.Identity.Name,
                        ActionType = Types.ActionType.UPDATE
                    };
                    context.ExtPartMappingToolStagingActivities.Add(logUpdate);
                    context.SaveChanges();
                    #endregion

                    response = new Models.Response.Response()
                    {
                        Success = true,
                        JSON_RESPONSE_DATA = JsonConvert.SerializeObject(extPartEntry)// JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings)
                    };
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }

            }
            else //if it's null we're adding
            {
                extPartEntry = new ExtPartMapping()
                {
                    OldPartId = saveModel.OldPartId,
                    NewPartId = saveModel.NewPartId
                };

                context.ExtPartMappings.Add(extPartEntry);
                context.SaveChanges();

                /* Here needs to be an updatae model of the extpartmapping added object */
                #region log save
                var logSave = new ExtPartMappingToolStagingActivity()
                {
                    Time = DateTime.UtcNow,
                    UserId = User.Identity.Name,
                    ActionType = Types.ActionType.SAVE
                };
                context.ExtPartMappingToolStagingActivities.Add(logSave);
                context.SaveChanges();
                #endregion

                response = new Models.Response.Response()
                {
                    Success = true,
                    JSON_RESPONSE_DATA = JsonConvert.SerializeObject(extPartEntry)// JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings)
                };
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }


            //return JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings); //Returns students list as JSON
        }

        [HttpGet]
        public ActionResult Search(string oldPartId = null, string newPartId = null)
        {
            var context = new Magnaflow_WebEntitiesProd();
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            List<ExtPartMapping> parts = null;
            Models.Response.Response response = null;

            var oldPID = String.IsNullOrWhiteSpace(oldPartId);
            var newPID = String.IsNullOrWhiteSpace(newPartId);

            try
            {
                if (!oldPID && !newPID)
                {
                    parts = context.ExtPartMappings.Where(x => x.OldPartId == oldPartId.Trim() && x.NewPartId == newPartId.Trim()).ToList();
                }
                else if (!oldPID)
                {
                    parts = context.ExtPartMappings.Where(x => x.OldPartId == oldPartId.Trim()).ToList();
                }
                else if (!newPID)
                {
                    parts = context.ExtPartMappings.Where(x => x.NewPartId == newPartId.Trim()).ToList();
                }
                else
                {
                    response = new Models.Response.Response()
                    {
                        Success = false,
                        JSON_RESPONSE_DATA = null,
                        Message = "Try again. This time, try searching for a part_Id"
                    };
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }

                if (parts.Count() == 0)
                {
                    response = new Models.Response.Response()
                    {
                        Success = false,
                        JSON_RESPONSE_DATA = null,
                        Message = "Search Yielded no results."
                    };
                    return this.Json(response, JsonRequestBehavior.AllowGet);
                }


                response = new Models.Response.Response()
                {
                    Success = true,
                    JSON_RESPONSE_DATA = JsonConvert.SerializeObject(parts)// JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings)
                };
                return this.Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
            //return JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings); //Returns students list as JSON
        }
    }
}