using Microsoft.AspNetCore.Http;
using SheSecure.ComplaintService.DTOs.Responses;

namespace SheSecure.ComplaintService.Interfaces
{
    public interface IComplaintFileService
    {
        Task<ComplaintFileResponseDTO> UploadFileAsync(
            int complaintId,
            IFormFile file);
    }
}