using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using GrantDisimbursement.Utilities;
using System.Data;
using GrantDisimbursement.Models;

namespace GrantDisimbursement.Controllers
{
    [RoutePrefix("api/GrantScheme")]
    public class GrantSchemeController : ApiController
    {
        const int SEB_INCOME_BELOW = 150000;
        const int SEB_AGE_BELOW = 16;
        const int FTS_AGE_BELOW = 18;
        const int EB_AGE_ABOVE = 50;
        const int BSG_AGE_BELOW = 5;
        const int YGG_INCOME_BELOW = 100000;
        const string SELECTORSNIPPET = @" H.[ObjectID], H.[HousingType], H.[IsDeleted],
                            M.[ObjectID], M.[Name], M.[Gender],
                            M.[MaritalStatus], M.[SpouseID], M.[OccupationType],
                            M.[AnnualIncome], M.[DateOfBirth], M.[HouseholdID], M.[IsDeleted] ";

        /// <summary>
        /// Endpoint for querying householdsand receipients of grant disbursement endpoint.
        /// </summary>
        /// <param name="grantscheme">grant schemes</param>
        /// <param name="householdsizegte">household size greater than or equals</param>
        /// <param name="householdsizelte">household size less  than or equals</param>
        /// <param name="totalincomegte">total income less than or equals</param>
        /// <param name="totalincomelte">total income more than or equals</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{grantScheme}")]
        public IHttpActionResult Query(string grantscheme, int? householdsizegte = null, int? householdsizelte = null, int? totalincomegte = null, int? totalincomelte = null)
        {
            try
            {
                string HOUSEHOLDSIZEGTE = householdsizegte == null ? "1=1" : $"COUNT(A.HouseholdID) >= {householdsizegte}";
                string HOUSEHOLDSIZELTE = householdsizegte == null ? "1=1" : $"COUNT(A.HouseholdID) <= {householdsizelte}";
                string TOTALINCOMEGTE = totalincomegte == null ? "1=1" : $"SUM(A.AnnualIncome) >= {totalincomegte}";
                string TOTALINCOMELTE = totalincomelte == null ? "1=1" : $"SUM(A.AnnualIncome) <= {totalincomelte}";

                string searchQuery;
                if (householdsizegte == null && householdsizelte == null && totalincomegte == null && totalincomelte == null)
                {
                    searchQuery = String.Empty;
                }
                else
                {
                    searchQuery = $@"AND H.ObjectID IN (SELECT A.HouseholdID FROM [Member] A
                                    WHERE A.IsDeleted = 0
                                    GROUP BY A.HouseholdID
                                    HAVING {TOTALINCOMEGTE} AND {TOTALINCOMELTE}
                                    AND {HOUSEHOLDSIZEGTE} AND {HOUSEHOLDSIZELTE})";
                }

                string sql = String.Empty;
                List<HouseholdEntityResponse> householdEntityResponses = new List<HouseholdEntityResponse>();
                HouseholdEntityResponse householdEntityResponse = new HouseholdEntityResponse();

                if (grantscheme.ToLower() == "studentencouragementbonus")
                {
                    //basic query
                    sql = $@"
                    SELECT {SELECTORSNIPPET} FROM [Household] H 
                    LEFT JOIN [Member] M ON H.ObjectID = M.HouseholdID
                    WHERE DATEDIFF(year, M.DateOfBirth, GETDATE()) < {SEB_AGE_BELOW} AND 
                    M.OccupationType = 'STUDENT' AND 
                    H.IsDeleted = 0 AND H.ObjectID IN 
                                   (SELECT M1.HouseholdID FROM [Member] M1 
                                    WHERE M1.HouseholdID is NOT NULL 
                                    AND M1.IsDeleted = 0
                                    GROUP BY M1.HouseholdID
                                    HAVING SUM(M1.AnnualIncome) < {SEB_INCOME_BELOW})
                    {searchQuery};
                    ";
                }
                else if (grantscheme.ToLower()  == "familytogethernessscheme")
                {
                    sql = $@"
                    SELECT {SELECTORSNIPPET} FROM [Household] H 
                    LEFT JOIN [Member] M ON H.ObjectID = M.HouseholdID
                    WHERE H.ObjectID IN (
                        SELECT M1.[HouseholdID] FROM [MEMBER] M1 WHERE 
                        M1.[SpouseID] IN (SELECT M2.[OBJECTID] FROM [MEMBER] M2 WHERE M2.[HouseholdID] = M1.[HouseholdID]) AND 
                        M1.[HouseholdID] IN (SELECT M3.[HouseholdID] FROM [MEMBER] M3 WHERE DATEDIFF(year, M3.DateOfBirth, GETDATE()) < {FTS_AGE_BELOW}))
                    {searchQuery};
                    ";
                }
                else if (grantscheme.ToLower() == "elderbonus")
                {
                    sql = $@"
                    SELECT {SELECTORSNIPPET} FROM [Household] H 
                    LEFT JOIN [Member] M ON H.ObjectID = M.HouseholdID
                    WHERE DATEDIFF(year, M.DateOfBirth, GETDATE()) > {EB_AGE_ABOVE}
                    {searchQuery};
                    ";
                }
                else if (grantscheme.ToLower() == "babysunshinegrant")
                {
                    sql = $@"
                    SELECT {SELECTORSNIPPET} FROM [Household] H 
                    LEFT JOIN [Member] M ON H.ObjectID = M.HouseholdID
                    WHERE DATEDIFF(year, M.DateOfBirth, GETDATE()) < {BSG_AGE_BELOW}
                    {searchQuery};
                    ";
                }
                else if (grantscheme.ToLower() == "yologstgrant")
                {
                    sql = $@"
                    SELECT {SELECTORSNIPPET} FROM [Household] H 
                    LEFT JOIN [Member] M ON H.ObjectID = M.HouseholdID
                    WHERE H.OBJECTID IN (
                        SELECT M1.HouseholdID FROM [Member] M1 
                        WHERE M1.IsDeleted = 0 
                        GROUP BY M1.HouseholdID
                        HAVING SUM(M1.AnnualIncome) < {YGG_INCOME_BELOW})
                    {searchQuery};
                    ";
                }
                else
                {
                    return BadRequest("url parameter grant scheme should be on of 'studentencouragementbonus', 'familytogethernessscheme', 'elderbonus', 'babysunshinegrant', 'yologstgrant'.");
                }

                SqlCommand command;
                SqlDataReader dataReader;
                Guid? tempID = null;
                // Query Runner
                using (SqlConnection c = new SqlConnection(ConfigurationManager.AppSettings["database"]))
                {
                    command = new SqlCommand(sql, c);
                    c.Open();
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        var household = Helpers.ReadHouseholdRow((IDataRecord)dataReader);
                        if (tempID != household.ObjectID)
                        {
                            if (tempID != null)
                            {
                                //add the householdentityResponse to list
                                householdEntityResponses.Add(householdEntityResponse);
                            }
                            householdEntityResponse = new HouseholdEntityResponse();
                            householdEntityResponse.Household = household;
                            householdEntityResponse.FamilyMembers = new List<Member>();
                            tempID = household.ObjectID.Value;
                        }
                        var familyMember = Helpers.ReadMemberRow((IDataRecord)dataReader, 3);
                        householdEntityResponse.FamilyMembers.Add(familyMember);
                    }
                    // end of query result, add last result into list.
                    householdEntityResponses.Add(householdEntityResponse);
                }
                return Ok(householdEntityResponses);
            }
            catch (Exception ex)
            {
                return BadRequest("Query failed.");
            }
        }
    }
}
