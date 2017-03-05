using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Settings
{
    public class GeneralSettings
    {
        public BaseSettings ChronobankSettings { get; set; }
        public GeneralDb Db { get; set; }
    }

    public class GeneralDb
    {
        public string ChronoBankSrvConnString { get; set; }
        public string SharedStorageConnString { get; set; }
    }
}
