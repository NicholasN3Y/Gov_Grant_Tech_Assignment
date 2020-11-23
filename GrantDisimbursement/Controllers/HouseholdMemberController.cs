using GrantDisimbursement.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GrantDisimbursement.Controllers
{
    [RoutePrefix("api/HouseholdMember")]
    public class HouseholdMemberController : ApiController
    {
        /// <summary>
        /// Addes a Family Member to the Household
        /// </summary>
        /// <param name="familyMember"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult AddFamilyMemberToHousehold([FromBody] Member familyMember)
        {
            Member member = familyMember;
            try
            {
                //validate Member
                if (member == null)
                {
                    return BadRequest("Member data insufficiently specified");
                }

                if (member.ObjectID != null)
                {
                    return BadRequest("Incorrect API endpoint. This API is for creation of new member to the household. ObjectID should be null.");
                }

                if (string.IsNullOrWhiteSpace(member.Name))
                {
                    return BadRequest("Property 'Name' is mandatory");
                }

                string[] genderOptions = new string[] { "MALE", "FEMALE" };

                if (string.IsNullOrWhiteSpace(member.Gender) || !genderOptions.Contains(member.Gender.ToUpper()))
                {
                    return BadRequest("Property 'Gender' must be one of these ('MALE', 'FEMALE')");
                }

                string[] maritalStatusOptions = new string[] { "SINGLE", "MARRIED", "WIDOWED", "SEPARATED", "DIVORCED" };

                if (member.MaritalStatus == null || !(maritalStatusOptions.Contains(member.MaritalStatus.ToUpper())))
                {
                    return BadRequest("Property 'Marital Status' must be one of these ('SINGLE', 'MARRIED', 'WIDOWED', 'SEPARATED', 'DIVORCED')");
                }

                string[] occupationTypeOptions = new string[] { "UNEMPLOYED", "STUDENT", "EMPLOYED" };

                if (member.OccupationType == null || !(occupationTypeOptions.Contains(member.OccupationType.ToUpper())))
                {
                    return BadRequest("Property 'OccupationType' must be one of these ('UNEMPLOYED', 'STUDENT', 'EMPLOYED')");
                }

                if (member.AnnualIncome == null)
                    member.AnnualIncome = 0;
                
                if (!member.DateOfBirth.HasValue)
                {
                    return BadRequest("Property 'DateOfBirth' is mandatory.");
                }

                if (!member.HouseholdID.HasValue)
                {
                    return BadRequest("Property 'HouseholdID' is mandatory.");
                }

                if (member.MaritalStatus.ToUpper() == "SINGLE" && member.SpouseID.HasValue)
                {
                    return BadRequest("Single Person shouldn't have Spouse.");
                }

                member.IsDeleted = 0;
                member.ObjectID = Guid.NewGuid();

                String sql = @"
                    INSERT INTO Member ([ObjectID],
                    [Name],
                    [Gender],
                    [MaritalStatus],
                    [SpouseID],
                    [OccupationType],
                    [AnnualIncome],
                    [DateOfBirth],
                    [HouseholdID],
                    [IsDeleted]) 
                    VALUES (@ObjectID, @Name, @Gender, @MaritalStatus, @SpouseID, @OccupationType, 
                            @AnnualIncome, @DateOfBirth, @HouseholdID, @IsDeleted);";

                int result = 0;

                using (SqlConnection c = new SqlConnection(ConfigurationManager.AppSettings["database"]))
                {
                    SqlCommand command;
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    command = new SqlCommand(sql, c);
                    command.Parameters.Add(new SqlParameter("@ObjectID", member.ObjectID));
                    command.Parameters.Add(new SqlParameter("@Name", member.Name.ToUpper()));
                    command.Parameters.Add(new SqlParameter("@Gender", member.Gender.ToUpper()));
                    command.Parameters.Add(new SqlParameter("@MaritalStatus", member.MaritalStatus.ToUpper()));
                    if (member.SpouseID.HasValue)
                        command.Parameters.Add(new SqlParameter("@SpouseID", member.SpouseID));
                    else
                        command.Parameters.Add(new SqlParameter("@SpouseID", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@OccupationType", member.OccupationType.ToUpper()));
                    command.Parameters.Add(new SqlParameter("@AnnualIncome", member.AnnualIncome));
                    command.Parameters.Add(new SqlParameter("@DateOfBirth", member.DateOfBirth.Value.Date));
                    command.Parameters.Add(new SqlParameter("@HouseholdID", member.HouseholdID));
                    command.Parameters.Add(new SqlParameter("@IsDeleted", member.IsDeleted));

                    adapter.InsertCommand = command;
                    c.Open();
                    result = adapter.InsertCommand.ExecuteNonQuery();
                }

                if (result == 1)
                {
                    return Created(member.ObjectID.ToString(), member);
                }
                else
                {
                    return BadRequest("Creation of new household failed");
                }
            }
            catch (Exception ex)
            {
                if ((ex as SqlException).Errors[0].Message.Contains($"conflict occurred in database \"GRANTDB\", table \"dbo.Household\", column \'ObjectID\'"))
                    return BadRequest("Member must be added to an existing Household.");
                
                if ((ex as SqlException).Errors[0].Message.Contains($"FK_SpousalRelation"))
                    return BadRequest("Spouse must be someone already added to the Household");

                return BadRequest(ex.Message);
            }
        }
    }
}
