using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorSite.DataAccess.DTO
{
    public class VideoCategory
    {
        public int id { get; set; }
        public int video_id { get; set; }
        public int category_id { get; set; }
        public DateTime created_date { get; set; }
    }
}
