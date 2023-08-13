using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters
{
    public class PersonsListActionFilters : IActionFilter
    {
        private readonly ILogger<PersonsListActionFilters> _logger;

        public PersonsListActionFilters(ILogger<PersonsListActionFilters> logger) 
        {
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("{FilterName}.{MethodName} method", nameof(PersonsListActionFilters), nameof(OnActionExecuted));

            PersonsController personsController = (PersonsController)context.Controller;

            Dictionary<string, object?>? parameters = (Dictionary<string, object?>?)context.HttpContext.Items["arguments"];

            if (parameters != null) 
            {
                if(parameters.ContainsKey("searchBy"))
                    personsController.ViewData["CurrentSearchBy"] = Convert.ToString(parameters["searchBy"]);
                if(parameters.ContainsKey("searchString"))
                    personsController.ViewData["CurrentSearchString"] = Convert.ToString(parameters["searchString"]);
                if(parameters.ContainsKey("sortBy"))
                    personsController.ViewData["CurrentSortBy"] = Convert.ToString(parameters["sortBy"]);
                if(parameters.ContainsKey("sortOrder"))
                    personsController.ViewData["CurrentSortOrder"] = Convert.ToString(parameters["sortOrder"]);
            }

            //Searching
            personsController.ViewBag.SearchFields = new Dictionary<string, string>()
            {
                { nameof(PersonResponse.PersonName), "Person Name" },
                { nameof(PersonResponse.Email), "Email" },
                { nameof(PersonResponse.DateOfBirth), "Date Of Birth" },
                { nameof(PersonResponse.Age), "Age" },
                { nameof(PersonResponse.Gender), "Gender" },
                { nameof(PersonResponse.Country), "Country" },
                { nameof(PersonResponse.Address), "Address" },
                { nameof(PersonResponse.ReceiveNewsLetters), "Receive News Letters" },
            };

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items["arguments"] = context.ActionArguments;

            _logger.LogInformation("{FilterName}.{MethodName} method", nameof(PersonsListActionFilters), nameof(OnActionExecuting));

            if (context.ActionArguments.ContainsKey("searchBy"))
            {
                string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);

                if (!String.IsNullOrEmpty(searchBy))
                {
                    var searchByOptions = new List<string>()
                    {
                        nameof(PersonResponse.PersonName),
                        nameof(PersonResponse.Email),
                        nameof(PersonResponse.DateOfBirth),
                        nameof(PersonResponse.Gender),
                        nameof(PersonResponse.CountryId),
                        nameof(PersonResponse.Address)
                    };

                    //Reset the searchBy parameter value
                    if (searchByOptions.Any(s => s == searchBy) == false)
                    {
                        _logger.LogInformation("searchBy actual value is: {searchBy}", searchBy);
                        context.ActionArguments["searchBy"] = searchBy = nameof(PersonResponse.PersonName);
                        _logger.LogInformation("searchBy updated value is: {searchBy}", searchBy);
                    }
                }
            }

        }
    }
}
