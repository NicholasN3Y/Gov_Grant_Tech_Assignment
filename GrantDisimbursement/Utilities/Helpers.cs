using GrantDisimbursement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace GrantDisimbursement.Utilities
{
    public static class Helpers
    {
        public static Household ReadHouseholdRow(IDataRecord record)
        {
            return new Household()
            {
                ObjectID = (Guid?)record[0],
                HousingType = (string)record[1],
                IsDeleted = (int?)record[2]
            };
        }

        public static Member ReadMemberRow(IDataRecord record, int offset = 0)
        {
            Member m = new Member();
            m.ObjectID = record.GetGuid(0 + offset);
            m.Name = record.GetString(1 + offset);
            m.Gender = record.GetString(2 + offset);
            m.MaritalStatus = record.GetString(3 + offset);

            if (record.IsDBNull(4 + offset))
                m.SpouseID = null;
            else
                m.SpouseID = record.GetGuid(4 + offset);

            m.OccupationType = record.GetString(5 + offset);
            m.AnnualIncome = record.GetDecimal(6 + offset);
            m.DateOfBirth = record.GetDateTime(7 + offset);

            if (record.IsDBNull(8 + offset))
                m.HouseholdID = null;
            else
                m.HouseholdID = record.GetGuid(8 + offset);

            m.IsDeleted = record.GetInt32(9 + offset);
            return m;
        }
    }
}