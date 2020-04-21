using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GIGLite.Auth.Models.ViewModels
{
    public class RegisterViewModel
    {
        public RegisterViewModel()
        {
            Roles = new List<string>();
        }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        //public string ConfirmPassword { get; set; }
        [Required]
        public List<string> Roles { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
       
        //public int? DepartmentId { get; set; }
       
        public string TerminalName { get; set; }
        public string PositionName { get; set; }
        public string DepartmentName { get; set; }
        public string PartnerName { get;  set; }
        
        //public int? PositionId { get; set; }
        //public int? PartnerId { get; set; }
        public DateTime DateJoined { get; set; }
        public string EmployeeCode { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public string NextOfKin { get; set; }
        public string NextOfKinPhone { get; set; }
        public UserType UserType { get; set; }
        public bool IsActive { get; set; }
      
    }

    public class SignoutModel
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}
