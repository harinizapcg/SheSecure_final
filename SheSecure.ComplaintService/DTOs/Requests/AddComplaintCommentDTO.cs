namespace SheSecure.ComplaintService.DTOs.Requests
{
    public class AddComplaintCommentDTO
    {
        public int ComplaintId { get; set; }

        public int UserId { get; set; }

        public string Comment { get; set; }

        public bool IsInternal { get; set; }
    }
}