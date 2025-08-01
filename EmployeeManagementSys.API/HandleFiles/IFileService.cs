namespace EmployeeManagementSys.API.HandleFiles
{
    public interface IFileService
    {
        Task<FileUploadResult> UploadFileAsync(IFormFile file);
    }
}
