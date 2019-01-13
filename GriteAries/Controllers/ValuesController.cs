using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GriteAries.Models;


namespace GriteAries.Controllers
{
    
    public class ValuesController : ApiController
    {
        Job _job; 
        // GET api/values
        public List<DataPrint> Get()
        {
            _job = new Job();
            var list = _job.GetDataPrintFootball();

            if (list.Count == 0)
            {
                return null;
            }

            return list;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
