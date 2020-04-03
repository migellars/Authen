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

        }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public string Terminal { get; set; }
        public string DateJoined { get; set; }
        public string Position { get; set; }

    }
}
