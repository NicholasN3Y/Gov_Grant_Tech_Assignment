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
        /// Retrieves a specific  household
        /// </summary>
        /// <param name="householdID">the identifier of the household</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{householdID}")]
        public IHttpActionResult RetrieveHousehold(Guid? householdID)
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

        private Household ReadHouseholdRow(IDataRecord record)
        {
            return new Household()
            {
                ObjectID = (Guid?)record[0],
                HousingType = (string)record[1],
                IsDeleted = (int?)record[2]
            };
        }

        private Member ReadMemberRow(IDataRecord record)
        {
            Member m = new Member();
            m.ObjectID = record.GetGuid(0);
            m.Name = record.GetString(1);
            m.Gender = record.GetString(2);
            m.MaritalStatus = record.GetString(3);

            if (record.IsDBNull(4))
                m.SpouseID = null;
            else
                m.SpouseID = record.GetGuid(4);

            m.OccupationType = record.GetString(5);
            m.AnnualIncome = record.GetDecimal(6);
            m.DateOfBirth = record.GetDateTime(7);

            if (record.IsDBNull(8))
                m.HouseholdID = null;
            else
                m.HouseholdID = record.GetGuid(8);

            m.IsDeleted = record.GetInt32(9);
            return m;
        }
    }
}
