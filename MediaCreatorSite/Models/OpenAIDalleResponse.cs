using MediaCreatorSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorSite.Models
{

    public class OpenAIPictureResponse
    {
        public List<OpenAIPictureResponseData> data = new List<OpenAIPictureResponseData> { };
    }

    public class OpenAIPictureResponseData
{
        public string url { get; set; }
    }

}
