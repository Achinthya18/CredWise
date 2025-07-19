using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CredWise.Models
{
    [Table("LOAN_PAYMENTS")]
    public class LoanPayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }
        public int LoanId { get; set; }
        public int CustomerId { get; set; } 

        [Required]
        [Column(TypeName = "decimal(18, 2)")] 
        public decimal PaidAmount { get; set; }

        public DateTime? PaymentDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string PaymentMethod { get; set; } 

        [StringLength(255)]
        public string TransactionId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Success"; 

        [ForeignKey("LoanId")]
        public LoanApplication LoanApplication { get; set; }
    }
} 