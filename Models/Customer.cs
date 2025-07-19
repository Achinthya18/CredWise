using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CredWise.Models
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(15)]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        [StringLength(20)] 
        public string AccountNumber { get; set; }

        [Required] 
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<LoanApplication> LoanApplications { get; set; }
        public ICollection<KycApproval> kycApprovals { get; set; }
    }
}