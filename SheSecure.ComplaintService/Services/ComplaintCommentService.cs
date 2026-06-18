using Microsoft.AspNetCore.Http;
using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.DTOs.Responses;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Services
{
    public class ComplaintCommentService
        : IComplaintCommentService
    {
        private readonly IComplaintCommentRepository _repository;
        private readonly IComplaintRepository _complaintRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ComplaintCommentService(
            IComplaintCommentRepository repository,
            IComplaintRepository complaintRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _complaintRepository = complaintRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ComplaintCommentResponseDTO>
            AddCommentAsync(AddComplaintCommentDTO dto)
        {
            var comment = new ComplaintComment
            {
                ComplaintId = dto.ComplaintId,
                UserId = dto.UserId,
                Comment = dto.Comment,
                IsInternal = dto.IsInternal
            };

            var savedComment =
                await _repository.AddCommentAsync(comment);

            return new ComplaintCommentResponseDTO
            {
                Id = savedComment.Id,
                ComplaintId = savedComment.ComplaintId,
                UserId = savedComment.UserId,
                Comment = savedComment.Comment,
                IsInternal = savedComment.IsInternal,
                CreatedAt = savedComment.CreatedAt
            };
        }

        public async Task<List<ComplaintCommentResponseDTO>>
            GetCommentsByComplaintIdAsync(int complaintId)
        {
            var complaint = await _complaintRepository.GetComplaintByIdAsync(complaintId);
            if (complaint == null)
                throw new Exception("Complaint not found");

            var user = _httpContextAccessor.HttpContext?.User;
            if (user != null)
            {
                var employeeId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? user.FindFirst("nameid")?.Value
                    ?? "1";

                var email = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                    ?? user.FindFirst("name")?.Value
                    ?? "";

                var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                    ?? user.FindFirst("role")?.Value
                    ?? "Employee";

                if (role != "HR" && role != "Admin" && role != "Manager" && complaint.EmployeeId != employeeId && complaint.EmployeeId != email)
                {
                    throw new UnauthorizedAccessException("You are not authorized to view comments for this complaint.");
                }
            }

            var comments =
                await _repository
                    .GetCommentsByComplaintIdAsync(
                        complaintId);

            var userRole = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value 
                        ?? _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value 
                        ?? "Employee";

            if (userRole != "HR" && userRole != "Admin" && userRole != "Manager")
            {
                comments = comments.Where(c => !c.IsInternal).ToList();
            }

            return comments.Select(x =>
                new ComplaintCommentResponseDTO
                {
                    Id = x.Id,
                    ComplaintId = x.ComplaintId,
                    UserId = x.UserId,
                    Comment = x.Comment,
                    IsInternal = x.IsInternal,
                    CreatedAt = x.CreatedAt
                }).ToList();
        }
    }
}