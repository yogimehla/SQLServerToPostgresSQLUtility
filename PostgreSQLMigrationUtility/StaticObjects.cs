using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSQLMigrationUtility
{
    public static class StaticObjects
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static MenuForm RefMenu { get; set; }
        static StaticObjects()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
        public static string GetSettings()
        {
            if (File.Exists(Environment.ExpandEnvironmentVariables("%allusersprofile%") + "/PostgreSQLMigrationUtility/Configuration.config"))
            {
                return File.ReadAllText(Environment.ExpandEnvironmentVariables("%allusersprofile%") + "/PostgreSQLMigrationUtility/Configuration.config");
            }
            else {
                return "";
            }
       }
    }
}
