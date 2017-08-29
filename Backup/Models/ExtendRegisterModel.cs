using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;


namespace QML.Web.UI.Areas.Core.Models
{
    public class ExtendRegisterModel
    {

        
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [DataType(DataType.EmailAddress)]
        
        [Display(Name = "Email address")]
        public string Email { get; set; }
        [Display(Name = "Password")]
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Username")]
        [Required]
        public string UserName { get; set; }

        [Display(Name = "Họ và tên")]
        
        public string ActualName { get; set; }

        [Display(Name = "Nhóm người sử dụng")]
        
        public int? UserGroup { get; set; }

        [Display(Name = "Faculty")]
        
        public string Faculty { get; set; }

        [Display(Name = "StudentClass")]
        
        public string StudentClass { get; set; }

        [Display(Name = "StudentID")]
        
        public string StudentID { get; set; }

        [Display(Name = "Hạn sử dụng")]       
        public DateTime DueDate { get; set; }

        [Display(Name = "Khóa")]        
        public string YearLearnt { get; set; }
       
    }
}
