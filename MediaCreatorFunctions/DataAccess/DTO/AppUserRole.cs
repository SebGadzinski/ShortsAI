using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.DataAccess.Dto
{
    public class AppUserRole
    {
        public Guid id { get; set; }
        public Guid role_id { get; set; }
        public Guid user_id { get; set; }
        public DateTime created_date { get; set; }
        public DateTime modified_date { get; set; }
        public string modified_by { get; set; } = "";
    }
}
