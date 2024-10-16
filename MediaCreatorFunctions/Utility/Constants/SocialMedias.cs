using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Utility.Constants
{
    public class SocialMedias
    {
        public static StaticSocialMedias YOUTUBE = new StaticSocialMedias() { Name = "Youtube", Website = "https://youtube.com" };
        public static List<StaticSocialMedias> ALL_STORES = new List<StaticSocialMedias>()
        {
            YOUTUBE
        };
    }
    public class StaticSocialMedias
    {
        public string Name { get; set; }
        public string Website { get; set; }
    }
}
