using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrantDisimbursement.Models
{
    public class Household
    {
        public Guid? ObjectID { get; set; }
        public string HousingType { get; set; }
        public int? IsDeleted { get; set; }
    }
}