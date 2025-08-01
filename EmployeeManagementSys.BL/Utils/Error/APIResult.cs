
namespace EmployeeManagementSys.BL
{
    public class APIResult
    {
        public bool Success { get; set; }
        public APIError[] Errors { get; set; } = [];
    }


    public class APIResult<T> : APIResult
    {
        public T? Data { get; set; }
    }
}
