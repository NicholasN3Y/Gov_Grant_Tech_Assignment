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
    /// <summary>
    /// To Test Connection To DB
    /// </summary>
    public class SampleController : ApiController
    {
        [HttpGet]
        [Route("api/testConnection")]
        public HttpResponseMessage TestConnection()
        {
            SqlConnection cnn;
            cnn = new SqlConnection(ConfigurationManager.AppSettings["database"]);
            cnn.Open();
            cnn.Close();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
