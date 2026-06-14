namespace SheSecure.ComplaintService.DTOs.Responses
{
    public class ComplaintResponseDTO
    {
        public int Id { get; set; }

        public string ComplaintNumber { get; set; }

        public string Subject { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}