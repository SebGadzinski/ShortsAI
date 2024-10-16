using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.DataAccess.Dto
{
    public class AppUserSession
    {
        public int id { get; set; }
        public Guid user_id { get; set; }
        public string device_name { get; set; } = "Device Not Found";
        public DateTime created_date { get; set; }
        public DateTime modified_date { get; set; }
        public string modified_by { get; set; } = "";
    }
}
