using GrantDisimbursement.Models;
using System;
using System.Collections.Generic;
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
        /// <param name="member"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult AddFamilyMemberToHousehold([FromBody] Member member)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
