using MediaCreatorSite.Utility.Exceptions;
using Newtonsoft.Json;

namespace MediaCreatorSite.Utility.Results
{
    public static class ResultStatus
    {
        public const int OK = 1;
        public const int ERROR = 2;
    }
    public interface IBaseResult {
        public string successResult { get; set; }
        public string errorResult { get; set; }
        public int status { get; set; }
        public int errorCode { get; set; }
        public Exception? exception { get; set; }
    }
    public class BaseResult : IBaseResult
    {
        public string successResult { get; set; } = "Success!";
        public string errorResult { get; set; } = "";
        public int status { get; set; }
        public int errorCode { get; set; }
        public Exception? exception { get; set; }
        public string CloseResult()
        {
            CompleteStatus();
            return JsonConvert.SerializeObject(this);
        }
        public void CompleteStatus()
        {
            this.status = this.exception == null && this.errorResult.Equals("") ? ResultStatus.OK : ResultStatus.ERROR;
            if (this.status == ResultStatus.ERROR && this.errorResult.Equals("") && this.exception != null)
            {
                this.errorCode = ExceptionCode.ExceptionCodes.ContainsKey(this.exception.GetType().Name) ? ExceptionCode.ExceptionCodes[this.exception.GetType().Name] : 404;
                this.errorResult = this.exception.Message;
            }
        }

        public bool isOk()
        {
            return this.errorResult.Equals("");
        }
    }
}
