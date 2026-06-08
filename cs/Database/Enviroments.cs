using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDWorks_Shop_Designer.Database
{
    public static class DbTABLE_NAMES
    {
        public static string SD_INSTANCEGROUP = "SHOPDRAWING_CONNECTIONAB";
        public static string SD_INSTANCEGROUP_MEMBERS = "SHOPDRAWING_CONNECTIONAB_MEMBERS";
        public static string SD_OBJECT = "SHOPDRAWING_DRAWOBJECT";
        public static string SD_TEXT = "SHOPDRAWING_TEXT";

    }
    public static class DbCOLUMN_ID_NAMES
    {
        public static string INSTANCE_GROUP_ID = "INSTANCE_GROUP_ID";
        public static string INSTANCE_GROUP_MEMBER = "INSTANCE_ID";
        public static string INSTANCE_ID= "INSTANCE_ID";
        public static string INSTANCE_PARENTGROUP_ID = "INSTANCE_GROUP_PARENT_ID";
        public static string INSTANCE_GROUP_NAME = "INSTANCE_GROUP_NM";
        public static string INSTANCE_GROUP_MEMBER_GUID = "MAPNG_ID";
        public static string INSTANCE_VERSION_SEQUENCE = "VERSION_SEQUENCE";

        public static string TEXT_ID = "ID";
        public static string MAPOWNER_ID = "MapOwner";
        public static string DRAWOBJECT_ID = "ID";

    }

}
