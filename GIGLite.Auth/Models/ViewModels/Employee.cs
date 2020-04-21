using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GIGLite.Auth.Models.ViewModels
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }
        public string EmployeeCode { get; set; }
        public DateTime DateJoined { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        //public string FullName => FirstName + " " + LastName;
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Otp { get; set; }
        public bool OtpIsUsed { get; set; }
        public string TicketRemovalOtp { get; set; }
        public bool TicketRemovalOtpIsUsed { get; set; }
        public DateTime? OTPLastUsedDate { get; set; }
        public int? OtpNoOfTimeUsed { get; set; }
        public string EmployeePhoto { get; set; }
        public string NextOfKin { get; set; }
        public string NextOfKinPhone { get; set; }


        // FK
        public int? WalletId { get; set; }
        public string WalletNumber { get; set; }

        //public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        //public int? PartnerId { get; set; }
        public string PartnerName { get; set; }

        //public int? PositionId { get; set; }
        public string PositionName { get; set; }

        //public int? TerminalId { get; set; }
        public string TerminalName { get; set; }

        public double TotalSales { get; set; }
        public double TotalCashSales { get; set; }
        public double TotalExpenseSales { get; set; }
        public double TotalCashRemittance { get; set; }


        public string ReferralCode { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

    }

}
