using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExtPartMappingTool
{
    public static class Constants
    {
        public const string DataTeam_APPS_RO = "DataTeam_APPS_RO";
        public const string DataTeam_APPS_RW = "DataTeam_APPS_RW";
        public const string APP_ADMINS = "APP_ADMINS";


        public static List<string> GetRoles()
        {
            List<string> returnList = new List<string>();
            returnList.Add(DataTeam_APPS_RO);
            returnList.Add(DataTeam_APPS_RW);
            returnList.Add(APP_ADMINS);
            return returnList;
        }

    }
}