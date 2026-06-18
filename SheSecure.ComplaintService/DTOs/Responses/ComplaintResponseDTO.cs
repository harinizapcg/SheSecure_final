namespace SheSecure.ComplaintService.DTOs.Responses
{
    public class ComplaintResponseDTO
    {
        public int Id { get; set; }

        public string ComplaintNumber { get; set; }

        public string Subject { get; set; }

        public string Status { get; set; }

        public string Category { get; set; }

        public string Priority { get; set; }

        public string? AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}