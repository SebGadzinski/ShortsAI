using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorSite.DataAccess.Constants
{
    public class Stores
    {
        public static StaticStore CHAT_GPT = new StaticStore() { Id = 1, Name = "ChatGPT", Website = "https://chat.openai.com/" };
        public static StaticStore DEEP_AI = new StaticStore() { Id = 2, Name = "DeepAI", Website = "https://deepai.org/" };
        public static StaticStore GOOGLE = new StaticStore() { Id = 3, Name = "Google", Website = "fucking google"};
        public static List<StaticStore> ALL_STORES = new List<StaticStore>()
        {
            CHAT_GPT, DEEP_AI, GOOGLE
        };
    }
    public class StaticStore
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Website { get; set; }
    }
}
