using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorSite.DataAccess.QueryModels
{
    public class ScriptAudio
    {
        public int script_id { get; set; }
        public int script_component_id { get; set; }
        public int audio_id { get; set; }
        public string url { get; set; }
    }
}
