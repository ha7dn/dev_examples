using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;


namespace Filtering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {

        IEnumerable<Person> persons = new List<Person>() {
            new Person() { Name = "Nancy Davolio", DOB = DateTime.Parse("1948-12-08"), Email = "nancy.davolio@test.com" , MyHomeID = 1, MyHome = new Home(){Address = "Street 1", Rental = true, Cost = 1234.55f, Inhabitants = 3}},
            new Person() { Name = "Andrew Fuller", DOB = DateTime.Parse("1952-02-19"), Email = "andrew.fuller@test.com" , MyHomeID = 2, MyHome = new Home(){Address = "Street 1", Rental = true, Cost = 1234.55f, Inhabitants = 3}},
            new Person() { Name = "Janet Leverling", DOB = DateTime.Parse("1963-08-30"), Email = "janet.leverling@test.com" , MyHomeID = 3, MyHome = new Home(){Address = "Street 1", Rental = true, Cost = 1234.55f, Inhabitants = 3}},
            new Person() { Name = "Margaret Peacock", DOB = DateTime.Parse("1937-09-19"), Email = "margaret.peacock@test.com" , MyHomeID = 4, MyHome = new Home(){Address = "Street 1", Rental = true, Cost = 1234.55f, Inhabitants = 3}},
            new Person() { Name = "Steven Buchanan", DOB = DateTime.Parse("1955-03-04"), Email = "steven.buchanan@test.com" , MyHomeID = 5, MyHome = new Home(){Address = "Street 1", Rental = true, Cost = 1234.55f, Inhabitants = 3}},
            new Person() { Name = "Michael Suyama", DOB = DateTime.Parse("1963-07-02"), Email = "michael.suyama@test.com" , MyHomeID = 6, MyHome = new Home(){Address = "Street 1", Rental = true, Cost = 1234.55f, Inhabitants = 3}},
            new Person() { Name = "Robert King", DOB = DateTime.Parse("1960-05-29"), Email = "robert.king@test.com" , MyHomeID = 7, MyHome = new Home(){Address = "Street 1", Rental = true, Cost = 1234.55f, Inhabitants = 3}},
            new Person() { Name = "Laura Callahan", DOB = DateTime.Parse("1958-01-09"), Email = "laura.callahan@test.com" , MyHomeID = 8, MyHome = new Home(){Address = "Street 1", Rental = true, Cost = 1234.55f, Inhabitants = 3}},
            new Person() { Name = "Anne Dodsworth", DOB = DateTime.Parse("1966-01-27"), Email = "anne.dodsworth@test.com", MyHomeID = 9, MyHome = new Home(){ Address = "Street 1", Rental = true, Cost = 1234.55f, Inhabitants = 3} }
            };

        // GET api/values
        [HttpGet]
        public ActionResult<PagedCollectionResponse<Person>> Get([FromQuery] FilterModel filter)
        {

            //return Ok();

            Func<FilterModel, IEnumerable<Person>> filterData = (filterModel) =>
            {
                if (String.IsNullOrEmpty(filterModel.Term))
                {
                    return persons.Where(p => p.Email.Contains(filterModel.Term ?? String.Empty, StringComparison.InvariantCultureIgnoreCase))
                                  .Skip((filterModel.Page - 1) * filter.Limit)
                                  .Take(filterModel.Limit);
                }
                else
                {
                    return persons.Skip((filterModel.Page - 1) * filter.Limit).Take(filterModel.Limit);
                }
            };

            //Get the data for the current page  
            var result = new PagedCollectionResponse<Person>();
            result.Items = filterData(filter);

            //Get next page URL string  
            FilterModel nextFilter = filter.Clone() as FilterModel;

            var val = filterData(nextFilter).Count();
            var foo = Request.Scheme;
            var url = Url.Action("Get", null, nextFilter, Request.Scheme);

            nextFilter.Page += 1;
            String nextUrl = filterData(nextFilter).Count() <= 0 ? null : this.Url.Action("Get", null, nextFilter, Request.Scheme);

            //Get previous page URL string  
            FilterModel previousFilter = filter.Clone() as FilterModel;
            previousFilter.Page -= 1;
            String previousUrl = previousFilter.Page <= 0 ? null : this.Url.Action("Get", null, previousFilter, Request.Scheme);

            result.NextPage = !String.IsNullOrWhiteSpace(nextUrl) ? new Uri(nextUrl) : null;
            result.PreviousPage = !String.IsNullOrWhiteSpace(previousUrl) ? new Uri(previousUrl) : null;

            return result;

        }
    }
}
