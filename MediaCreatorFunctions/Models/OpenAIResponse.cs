using MediaCreatorFunctions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorFunctions.Models
{

    public class OpenAIResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string Model { get; set; }
        public OpenAIResponseUsageData Usage { get; set; }
        public OpenAIResponseChoice[] Choices { get; set; }
    }

    public class OpenAIResponseUsageData
    {
        public int Prompt_Tokens { get; set; }
        public int Completion_Tokens { get; set; }
        public int Total_Tokens { get; set; }
    }

    public class OpenAIResponseChoice
    {
        public OpenAIResponseMessage Message { get; set; }
        public string Finish_Reason { get; set; }
        public int Index { get; set; }
    }

    public class OpenAIResponseMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

}
