using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Microwork_Platform_for_the_unemployed.Models
{
    [MetadataType(typeof(EmployerMetaData))]
    public partial class Employer
    {
        public string ConfirmPassword { get; set; }

    }

    public class EmployerMetaData
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Display(Name = "Company Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Company name required")]
        public string CompanyName { get; set; }
        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email address required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public System.DateTime RegisterDate { get; set; }
        public int JobPosts { get; set; }
        public bool IsEmailVerified { get; set; }

        [Display(Name = "Password")]
        [MinLength(6, ErrorMessage = "Mininum 6 characters required")]
        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password and password do not match")]
        public string ConfirmPassword { get; set; }
        public System.Guid ActivationCode { get; set; }


    }
}