using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Simpleloginsystem.Models
{
    [Table("tblEmployee")]
    public class User
    {

        [Key]
        public int EmployeeID
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Employee Name is required")]
        [Display(Name = "Employee Name")]
        public string EmployeeName
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Email ID is required")]
        [System.Web.Mvc.Remote("doesEmailExist", "User", ErrorMessage = "EmailId already exists. Please enter a different EmailID.")]
        [EmailAddress]
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


        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string EncryptedPassword
        {
            get;
            set;
        }
        public string EmployeeRole
        {
            get;
            set;
        }
    }

    public class Login {
        [Key]
        public int EmployeeID
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Email ID is required")]

        [EmailAddress]
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
}