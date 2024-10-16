using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.DataAccess.DTO
{
    public class StoreReceipt
    {
        public int id { get; set; }
        public Guid user_id { get; set; }
        public int store_id { get; set; }
        public double cost { get; set; }
        public string purpose { get; set; }
        public DateTime created_date { get; set; }
    }
}
