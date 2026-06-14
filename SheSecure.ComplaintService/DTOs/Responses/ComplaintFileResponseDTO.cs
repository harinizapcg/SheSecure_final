namespace SheSecure.ComplaintService.DTOs.Responses
{
    public class ComplaintFileResponseDTO
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}