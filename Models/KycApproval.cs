using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CredWise.Models
{
    [Table("KYC_APPROVAL")]
    public class KycApproval
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KycID { get; set; }

        public int CustomerId { get; set; }

        public DateTime SubmissionDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; 

        public DateTime? ApprovalDate { get; set; } 

        [StringLength(255)] 
        public string DocumentPath { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
    }
}