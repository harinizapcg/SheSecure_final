using Microsoft.EntityFrameworkCore;
using SheSecure.ComplaintService.Entities;

namespace SheSecure.ComplaintService.Data
{
    public class ComplaintDbContext : DbContext
    {
        public ComplaintDbContext(
            DbContextOptions<ComplaintDbContext> options)
            : base(options)
        {
        }

        public DbSet<Complaint> Complaints { get; set; }

        public DbSet<ComplaintFile> ComplaintFiles { get; set; }

        public DbSet<ComplaintComment> ComplaintComments { get; set; }

        public DbSet<ComplaintStatusHistory> ComplaintStatusHistories { get; set; }

        public DbSet<ComplaintAssignment> ComplaintAssignments { get; set; }
    }
}