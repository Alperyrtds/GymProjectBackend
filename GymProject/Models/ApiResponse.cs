namespace GymProject.Models
{
    public class ApiResponse
    {
        public string status { get; set; }
        public string errorMessage { get; set; }
        public object data { get; set; }
        public ApiResponse()
        {

        }
        public ApiResponse(string _status, string _errorMessage, object _data)
        {
            status = _status;
            errorMessage = _errorMessage;
            data = _data;
        }
    }
}
