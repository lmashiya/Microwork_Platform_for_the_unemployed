using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Microwork_Platform_for_the_unemployed.Models
{
    [MetadataType(typeof(UserMetaData))]
    public partial class User
    {
        public string ConfirmPassword { get; set; }
    }

    public class UserMetaData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false,ErrorMessage = "First name required")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name required")]
        public string LastName { get; set; }
        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email address required")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        //[Display(Name = "Resume")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Required required")]
        public byte[] Resume { get; set; }
        [Display(Name = "Mobile Number")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Mobile number required")]
        public string MobileNumber { get; set; }

        [Display(Name = "Gender")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Display(Name = "Age")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Age is required")]
        public string Age { get; set; }
        [Display(Name = "Province")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Province is required")]
        public string Province { get; set; }

        [Display(Name = "City")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "City is required")]
        public string City { get; set; }

        [Display(Name = "Experience")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Experience is required")]
        public string Experience { get; set; }

        [Display(Name = "Key Skill")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Key Skill is required")]
        public string KeySkills { get; set; }

        [Display(Name = "Higher Qualification")]
        [Required(AllowEmptyStrings = true)]
        public string Higher { get; set; }

        [Display(Name = "Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public System.DateTime RegistrationDate { get; set; }
        public string DesiredProvince { get; set; }
        public string DesiredCity { get; set; }
        public bool IsEmailVerified { get; set; }
        public System.Guid ActivationCode { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString =  "{0:yyyy/MM/dd}")]

        public System.DateTime DateOfBirth { get; set; }
    }
}