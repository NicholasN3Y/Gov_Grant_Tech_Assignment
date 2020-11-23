using GrantDisimbursement.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.Data;

namespace GrantDisimbursement.Controllers
{
    [RoutePrefix("api/Household")]
    public class HouseholdController : ApiController
    {
        /// <summary>
        /// Creates the household (housing unit)
        /// </summary>
        /// <param name="housingType">possible options: Landed, Condominium, HDB</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateHousehold(string housingType)
        {
            try
            {
                string[] housingOptions = new string[] { "LANDED", "CONDOMINIUM", "HDB" };
                if (housingType == null ||
                    !(housingOptions.Contains(housingType.ToUpper())))
                    return BadRequest("Query parameter housing type must be one of these ('Landed', 'Condominium', 'HDB')");

                Household newHousehold = new Household();
                newHousehold.ObjectID = Guid.NewGuid();
                newHousehold.IsDeleted = 0;
                newHousehold.HousingType = housingType.ToUpper();

                String sql = @"INSERT INTO Household (ObjectID, IsDeleted, HousingType)
                           VALUES (@ObjectID, @IsDeleted, @HousingType);
                        ";

                int result = 0;

                using (SqlConnection c = new SqlConnection(ConfigurationManager.AppSettings["database"]))
                {
                    SqlCommand command;
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    command = new SqlCommand(sql, c);
                    command.Parameters.Add(new SqlParameter("@ObjectID", newHousehold.ObjectID));
                    command.Parameters.Add(new SqlParameter("@IsDeleted", newHousehold.IsDeleted));
                    command.Parameters.Add(new SqlParameter("@HousingType", newHousehold.HousingType));

                    adapter.InsertCommand = command;
                    c.Open();
                    result = adapter.InsertCommand.ExecuteNonQuery();
                }

                if (result == 1)
                {
                    return Created(newHousehold.ObjectID.ToString(), newHousehold);
                }
                else
                {
                    return BadRequest("Creation of new household failed");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all household listings
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetHouseHoldsListings()
        {
            try
            {
                // Return HouseholdListingResponse
                List<HouseholdEntityResponse> householdEntityResponses = new List<HouseholdEntityResponse>();

                HouseholdEntityResponse householdEntityResponse = new HouseholdEntityResponse();

                SqlCommand command;
                SqlDataReader dataReader;

                string sql = @"SELECT H.[ObjectID], H.[HousingType], H.[IsDeleted],
                            M.[ObjectID], M.[Name], M.[Gender],
                            M.[MaritalStatus], M.[SpouseID], M.[OccupationType],
                            M.[AnnualIncome], M.[DateOfBirth], M.[HouseholdID], M.[IsDeleted] 
                           FROM Household H             
                           LEFT JOIN Member M ON M.HouseholdID = H.ObjectID
                           WHERE M.IsDELETED = 0 AND H.IsDeleted = 0 ORDER BY H.[ObjectID], M.[Name];
                           ";

                Guid? tempID = null;

                using (SqlConnection c = new SqlConnection(ConfigurationManager.AppSettings["database"]))
                {
                    command = new SqlCommand(sql, c);
                    c.Open();
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        var household = ReadHouseholdRow((IDataRecord)dataReader);
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
                        var familyMember = ReadMemberRow((IDataRecord)dataReader, 3);
                        householdEntityResponse.FamilyMembers.Add(familyMember);
                    }
                    // end of query result, add last result into list.
                    householdEntityResponses.Add(householdEntityResponse);
                }
                return Ok(householdEntityResponses);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to get household listings.");
            }
        }


        /// <summary>
        /// Retrieves a specific  household
        /// </summary>
        /// <param name="householdID">the identifier of the household</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{householdID}")]
        public IHttpActionResult RetrieveHousehold(Guid? householdID)
        {
            try
            {
                if (householdID == null)
                    return BadRequest("householdID must be specified");

                // Return HouseholdListingResponse
                HouseholdEntityResponse householdEntityResponse = new HouseholdEntityResponse();

                SqlCommand command;
                SqlDataReader dataReader;

                string sql = @"SELECT ObjectID, HousingType, IsDeleted FROM Household WHERE IsDeleted = 0 and ObjectID = @ObjectID";

                using (SqlConnection c = new SqlConnection(ConfigurationManager.AppSettings["database"]))
                {
                    command = new SqlCommand(sql, c);
                    command.Parameters.Add(new SqlParameter("@ObjectID", householdID));
                    c.Open();
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        householdEntityResponse.Household = ReadHouseholdRow((IDataRecord)dataReader);
                    }

                    if (householdEntityResponse.Household == null)
                        return Ok("Household does not exist");
                }

                using (SqlConnection c = new SqlConnection(ConfigurationManager.AppSettings["database"]))
                {
                    sql = @"SELECT [ObjectID],
                    [Name],
                    [Gender],
                    [MaritalStatus],
                    [SpouseID],
                    [OccupationType],
                    [AnnualIncome],
                    [DateOfBirth],
                    [HouseholdID],
                    [IsDeleted] FROM [Member] 
                    WHERE [IsDeleted] = 0 AND HouseholdID = @HouseholdID";

                    c.Open();
                    command = new SqlCommand(sql, c);
                    command.Parameters.Add(new SqlParameter("@HouseholdID", householdID));

                    dataReader = command.ExecuteReader();
                    householdEntityResponse.FamilyMembers = new List<Member>();
                    while (dataReader.Read())
                    {
                        householdEntityResponse.FamilyMembers.Add(ReadMemberRow((IDataRecord)dataReader));
                    }
                }
                return Ok(householdEntityResponse);
            }
            catch(Exception ex)
            {
                return BadRequest("Failed to Retrieve single Household.");
            }
        }

        private Household ReadHouseholdRow(IDataRecord record)
        {
            return new Household()
            {
                ObjectID = (Guid?)record[0],
                HousingType = (string)record[1],
                IsDeleted = (int?)record[2]
            };
        }

        private Member ReadMemberRow(IDataRecord record, int offset = 0)
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
