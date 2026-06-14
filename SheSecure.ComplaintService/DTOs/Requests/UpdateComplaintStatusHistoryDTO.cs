namespace SheSecure.ComplaintService.DTOs.Requests
{
    public class UpdateComplaintStatusDTO
    {
        public int ComplaintId { get; set; }

        public string Status { get; set; }

        public string? ResolutionNotes { get; set; }
    }
}