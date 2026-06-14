namespace SheSecure.ComplaintService.DTOs.Requests
{
    public class CreateComplaintDTO
    {
        public string Category { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public bool IsAnonymous { get; set; }
    }
}