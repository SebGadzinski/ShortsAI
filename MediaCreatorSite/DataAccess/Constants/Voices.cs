using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorSite.DataAccess.Constants
{
    public class Voices
    {
        public static StaticVoice MALE = new StaticVoice() { Id = 1, Name = "Male"};
        public static StaticVoice FEMALE = new StaticVoice() { Id = 2, Name = "Female" };
        public static List<StaticVoice> ALL_VOICES = new List<StaticVoice>()
        {
            MALE, FEMALE
        };
    }
    public class StaticVoice
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
