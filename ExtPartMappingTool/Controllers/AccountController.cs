using System.Web.Mvc;
using System.Web.Security;

using ExtPartMappingTool.Models;
using System;
using System.Web;
using System.DirectoryServices;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
namespace ExtPartMappingTool.Controllers
{
    [Authorize(Roles = "DataTeam_APPS_RW, DataTeam_APPS_RO, APP_ADMINS")]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            return this.View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }
            Models.Response.Response response = null;

            if (Membership.ValidateUser(model.UserName, model.Password))
            {

                //connect to directory and axe it to look for user name memberOf properties
                DirectoryEntry de = new DirectoryEntry(System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString);// "LDAP"+"://magnaflow.com:389/DC=magnaflow,DC=com");
                var searcher =
                    new DirectorySearcher(de)
                    {
                        Filter = String.Format("(&(objectClass=user)(samaccountname={0}))", model.UserName)
                    };
                searcher.PropertiesToLoad.Add("MemberOf");

                //get directory entries of username. Should only be one?
                var directoryEntriesFound = searcher.FindAll()
                    .Cast<SearchResult>()
                    .Select(result => result.GetDirectoryEntry());
                List<String> rolesList = new List<string>(); 

                //extract roles for this user
                foreach (DirectoryEntry entry in directoryEntriesFound)
                {
                    var info = entry.Properties["MemberOf"].Value.GetType();
                    if(entry.Properties["MemberOf"].Value is string)
                    {
                        rolesList.Add(Regex.Replace(entry.Properties["MemberOf"].Value.ToString(), @"^CN=(.*?)(?<!\\),.*", "$1"));
                    }else
                    {
                        foreach(var objs in (Object[])entry.Properties["MemberOf"].Value)
                        {
                            rolesList.Add(Regex.Replace((string)objs, @"^CN=(.*?)(?<!\\),.*", "$1"));
                        }
                    }
                }

                //plase them in a string as FormsAuthenticationTicket expects data this way. Or more like that's how we want to recieve the data. 
                //Really could be anything but we're expecting list of roles
                string roles = "";
                var firstTime = true;
                foreach (var role in rolesList)
                {
                    if (firstTime)
                    {
                        roles = role;
                        firstTime = false;
                    }
                    else
                    {
                        roles += "," + role;
                    }
                }

                //This may or may not be necessary
                var expectedRolesList = Constants.GetRoles();
                var containsExpectedRole = false;
                foreach(var role in expectedRolesList)
                {
                    if (roles.Contains(role))
                    {
                        containsExpectedRole = true;
                    }
                }

                if(containsExpectedRole)
                {
                    //setup ticket
                    var authTicket = new FormsAuthenticationTicket(1, model.UserName, DateTime.Now, DateTime.Now.AddMinutes(20), false, roles);
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    HttpContext.Response.Cookies.Add(authCookie);

                    response = new Models.Response.Response()
                    {
                        Success = true,
                        JSON_RESPONSE_DATA = null
                    };
                }else
                {
                    response = new Models.Response.Response()
                    {
                        Success = false,
                        JSON_RESPONSE_DATA = null,
                        Message = "Login error PM-2034. Contact IT."
                    };
                }
                

                return this.Json(response, JsonRequestBehavior.AllowGet);
            }

            response = new Models.Response.Response()
            {
                Success = false,
                JSON_RESPONSE_DATA = null,
                Message = "The user name or password provided is incorrect."
            };
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            Models.Response.Response response = new Models.Response.Response()
            {
                Success = true,
                JSON_RESPONSE_DATA = null // JsonConvert.SerializeObject(commissionDataResponseObject)// JsonConvert.SerializeObject(commissionDataResponseObject, Formatting.None, settings)
            };
            return this.Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}