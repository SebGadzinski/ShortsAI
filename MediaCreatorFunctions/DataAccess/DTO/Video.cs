using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.DataAccess.DTO
{
    public class Video
    {
        public int id { get; set; }
        public Guid user_id { get; set; }
        public int status_type_id { get; set; }
        public string title { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int picture_store_id { get; set; }
        public int voice_id { get; set; }
        public DateTime created_date { get; set; }
        public DateTime modified_date { get; set; }
        public string modified_by { get; set; }
    }
}
