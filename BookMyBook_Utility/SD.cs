using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookMyBook_Utility
{
    public static class SD
    {
        public const string Role_User_Indi = "Individual";
        public const string Role_User_Company = "Company";
        public const string Role_User_Admin = "Admin";
        public const string Role_User_Employee = "Employee";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "InProcess";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public const string PayementPending = "Pending";
        public const string PayementStatusApproved = "Approved";
        public const string PayementDelayedPayement = "ApprovedForDelayedPayement";        
        public const string PayementRejected = "Rejected";

    }
}
