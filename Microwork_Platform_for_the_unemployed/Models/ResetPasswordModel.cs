using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Microwork_Platform_for_the_unemployed.Models
{
    public class ResetPasswordModel
    {
        [MinLength(6, ErrorMessage = "Mininum 6 characters required")]
        [DataType(DataType.Password)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "New password is required")]
        public string NewPassword { get; set; }
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirm password and password do not match")]
        public string ConfirmPassword { get; set; }
        public string ResetCode { get; set; }

    }
}