using Microsoft.AspNetCore.Http;
using SheSecure.ComplaintService.DTOs.Responses;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Services
{
    public class ComplaintFileService : IComplaintFileService
    {
        private readonly IComplaintFileRepository _repository;

        public ComplaintFileService(
            IComplaintFileRepository repository)
        {
            _repository = repository;
        }

        public async Task<ComplaintFileResponseDTO> UploadFileAsync(
            int complaintId,
            IFormFile file)
        {
            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads",
                "ComplaintEvidence");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = Guid.NewGuid() + "_" + file.FileName;

            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var complaintFile = new ComplaintFile
            {
                ComplaintId = complaintId,
                FileName = file.FileName,
                FilePath = fullPath
            };

            var savedFile =
                await _repository.AddFileAsync(complaintFile);

            return new ComplaintFileResponseDTO
            {
                Id = savedFile.Id,
                FileName = savedFile.FileName,
                FilePath = savedFile.FilePath,
                UploadedAt = savedFile.UploadedAt
            };
        }
    }
}