using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GIGLite.Auth.Models.ViewModels
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserType UserType { get; set; }

        //public string Department { get; set; }
        //public string Terminal { get; set; }
        //public string DateJoined { get; set; }
        //public string Position { get; set; }
        //public bool IsActive { get; set; } = true;
        //public bool IsDeleted { get; set; } = false;
        //public int UserType { get; set; } = 0;

    }

    public enum UserType
    {
        Administrator,
        Employee,
        Partner,
        Customer,
        Captain,
        CampusAmbassador
    }
}
