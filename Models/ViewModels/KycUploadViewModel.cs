using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CredWise.ViewModels
{
    public class KycUploadViewModel
    {
        // Identity Proof
        [Required(ErrorMessage = "Please select a document type for identity proof.")]
        [Display(Name = "Identity Document Type")]
        public string IdentityDocumentType { get; set; }

        [Required(ErrorMessage = "Please upload your identity proof.")]
        [Display(Name = "Identity Proof File")]
        public IFormFile IdentityProofFile { get; set; }

        [Required(ErrorMessage = "You must accept the declaration.")]
        [Display(Name = "Declaration")]
        public bool DeclarationAccepted { get; set; }
    }
}