using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrantDisimbursement.Models
{
    public class GrantScheme
    {
        public string GrantSchemeName { get; set; }
        public List<HouseholdEntityResponse> HouseholdEntity { get; set; }
    }
}