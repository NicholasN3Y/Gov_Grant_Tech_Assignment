using GrantDisimbursement.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;

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

    }
}
