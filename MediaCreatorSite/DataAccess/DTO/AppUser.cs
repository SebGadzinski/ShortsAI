using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorSite.DataAccess.Dto
{
    public class AppUser
    {
        public Guid id { get; set; }
        public string email { get; set; } = "";
        public string password { get; set; } = "";
        public string user_name { get; set; } = "";
        public bool email_confirmed { get; set; }
        public string? phone_number { get; set; }
        public bool phone_number_confirmed { get; set; }
        public bool two_factor_enabled { get; set; }
        public DateTime created_date { get; set; }
        public DateTime modified_date { get; set; }
        public string modified_by { get; set; } = "";
    }
}
