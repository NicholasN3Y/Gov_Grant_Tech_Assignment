using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrantDisimbursement.Models
{
    public class Member
    {
        public Guid? ObjectID { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public Guid? SpouseID { get; set; }

        /// <summary>
        /// either of Unemployed, Student, Employed
        /// </summary>
        public string OccupationType { get; set; }
        public Decimal? AnnualIncome { get; set; }
        public DateTime? DateOfBirth { get; set; }

        //Foreign Key to that of Household table
        public Guid? HouseholdID { get; set; }
    }
}