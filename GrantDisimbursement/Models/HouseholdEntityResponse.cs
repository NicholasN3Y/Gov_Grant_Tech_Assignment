using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrantDisimbursement.Models
{
    public class HouseholdEntityResponse
    {
        public Household Household { get; set; }
        public List<Member> FamilyMembers { get; set; }
    }
}