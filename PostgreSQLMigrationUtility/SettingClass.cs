using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSQLMigrationUtility
{
    public class SettingClass
    {
        public string MigrationLocation { get; set; }
        public string SQLServer { get; set; }
        public string SQLUsername { get; set; }
        public string SQLPassword { get; set; }
        public string SQLPort { get; set; }
        public string Postgres { get; set; }
        public string PostgresUsername { get; set; }
        public string PostgresPassword { get; set; }
        public string PostgresPort { get; set; }
    }
}
