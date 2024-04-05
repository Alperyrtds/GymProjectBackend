using FluentValidation.Results;

namespace GymProject.Models
{
    public class ApiResponse
    {
        public string status { get; set; }
        public string errorMessage { get; set; }
        public object data { get; set; }
        public List<ValidationFailure> errorMessageList { get; set; }
        public ApiResponse(string _status, string _errorMessage , List<ValidationFailure> _errorMessageList, object _data)
        {
            status = _status;
            errorMessage = _errorMessage;
            errorMessageList = _errorMessageList;
            data = _data;
        }
        public ApiResponse(string _status, string _errorMessage, object _data)
        {
            status = _status;
            errorMessage = _errorMessage;
            data = _data;
        }
        public ApiResponse()
        {

        }
    }
}
