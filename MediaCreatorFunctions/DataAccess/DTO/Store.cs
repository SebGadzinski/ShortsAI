using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.DataAccess.DTO
{
    public class Store
    {
        public int id { get; set; }
        public string name { get; set; }
        public string website { get; set; }
        public DateTime created_date { get; set; }
    }
}
