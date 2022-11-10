using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidFormatGenerator
{
    /// <summary>
    /// Summary description for Handler1
    /// </summary>
    public class GetGuid : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string guid = new DBFunctions().GetGuid();
            context.Response.ContentType = "text/plain";
            context.Response.Write(guid);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}