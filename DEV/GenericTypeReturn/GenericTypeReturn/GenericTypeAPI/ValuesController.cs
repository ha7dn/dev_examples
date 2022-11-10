using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GenericTypeReturn;

namespace GenericTypeAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        [HttpGet]
        public ActionResult Get()
        {
            var getVal = new GenericType<MainType>();
            getVal.Data = new MainType();
            getVal.Data.Name = "Samhain";
            getVal.Data.ID = 1;
            getVal.Data.Path = Environment.ProcessPath;
            return Ok(getVal);



            //var getVal2 = new GenericType<string, MainType>();
            //getVal2.Key = "ClassTypeOfValue";
            //getVal2.Value = new MainType();
            //getVal2.Value.Name = "Samhain";
            //getVal2.Value.ID = 2;
            //getVal2.Value.Path = Environment.ProcessPath;
            //return Ok(getVal2);
        }


    }
}
