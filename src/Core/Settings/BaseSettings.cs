using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Settings
{
    public class BaseSettings
    {
        public Db Db { get; set; }
    }

    public class Db
    {
        public string DataConnString { get; set; }
        public string LogsConnString { get; set; }
        public string SharedConnString { get; set; }
    }
}
