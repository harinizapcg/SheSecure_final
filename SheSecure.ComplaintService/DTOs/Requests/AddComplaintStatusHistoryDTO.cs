namespace SheSecure.ComplaintService.DTOs.Requests
{
    public class AddComplaintStatusHistoryDTO
    {
        public int ComplaintId { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public int ChangedBy { get; set; }

        public string Remarks { get; set; }
    }
}