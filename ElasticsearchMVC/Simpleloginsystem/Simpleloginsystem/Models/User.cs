using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Simpleloginsystem.Models
{
    [Table("tblUser")]
    public class Register
    {

        [Key]
        public int UserID
        {
            get;
            set;
        }
        [Required(ErrorMessage = "User Name is required")]
        [Display(Name = "User Name")]
        public string UserName
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Email ID is required")]
        [System.Web.Mvc.Remote("doesEmailExistRegister", "User", ErrorMessage = "EmailId already exists. Please enter a different EmailID.")]
        [EmailAddress]
        [Display(Name = "Email address")]
        [StringLength(150)]
        [ElasticProperty(OmitNorms = true, Index = FieldIndexOption.NotAnalyzed)]
        public string EmailID
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(150, MinimumLength = 6)]
        [Display(Name = "Password")]
        public string Password
        {
            get;
            set;
        }


        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string EncryptedPassword
        {
            get;
            set;
        }
        public string UserRole
        {
            get;
            set;
        }

        public string DecryptedPassword
        {
            get;
            set;
        }
        public int RecentPasswordReset
        {

            get;
            set;
        }

        public int VerifiedEmail
        {
            get;
            set;
        }

    }

    public class Login {
        [Key]
        public int UserID
        {
            get;
            set;
        }
        [EmailAddress]
        [Required(ErrorMessage = "Email ID is required")]
        [System.Web.Mvc.Remote("doesEmailExist", "User", ErrorMessage = "EmailId not exists. Please enter a different EmailID.")]
       // [System.Web.Mvc.Remote("doesEmailVerified", "User", ErrorMessage = "EmailId not verified. Please verify EmailID.")]
        [Display(Name = "Email address")]
        [StringLength(150)]
        public string EmailID
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(150, MinimumLength = 6)]
        [Display(Name = "Password")]
        public string Password
        {
            get;
            set;
        }
    }

    public class ForgotPassword
    {

        [Key]
        public int UserID
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Email ID is required")]
        [EmailAddress]
        [System.Web.Mvc.Remote("doesEmailExist", "User", ErrorMessage = "EmailId not exists. Please enter a different EmailID.")]
        [Display(Name = "Email address")]
        [StringLength(150)]
        public string EmailID
        {
            get;
            set;
        }
    }
    public class ChangeRole
    {

        [Key]
        public int UserID
        {
            get;
            set;
        }
        [Required(ErrorMessage = "User Name is required")]
        [Display(Name = "User Name")]
        public string UserName
        {
            get;
            set;
        }
     
    
        public string UserRole
        {
            get;
            set;
        }
    }

    public class ChangePassword
    {

        [Key]
        public int UserID
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(150, MinimumLength = 6)]
        [Display(Name = "New Password")]
        public string Password
        {
            get;
            set;
        }


        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string EncryptedPassword
        {
            get;
            set;
        }
    }
}