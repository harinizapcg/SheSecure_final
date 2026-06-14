namespace SheSecure.ComplaintService.DTOs.Responses
{
    public class ComplaintCommentResponseDTO
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public int UserId { get; set; }

        public string Comment { get; set; }

        public bool IsInternal { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}