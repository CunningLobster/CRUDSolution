using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    public class PersonsController : Controller
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;

        public PersonsController(IPersonsService personsService, ICountriesService countriesService, ILogger<PersonsController> logger)
        { 
            _personsService = personsService;
            _countriesService = countriesService;
            _logger = logger;
        }

        [Route("[action]")]
        [Route("/")]
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("Index method of Persons controller");
            _logger.LogDebug($"SearchBy: {searchBy}, SearchString: {searchString}, SortBy: {sortBy}, SortOrder: {sortOrder}");

            //Searching
            ViewBag.SearchFields = new Dictionary<string, string>()
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

            List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            //Sorting
            List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(persons, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;

            return View(sortedPersons);
        }


        //Executes when user clicks "Create Person" hyperlink
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Create()
        { 
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryId.ToString() });

            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = await _countriesService.GetAllCountries();
                ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryId.ToString() });
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                
                return View(personAddRequest);
            }

            PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);

            return RedirectToAction("Index", "Persons");
        }

        [HttpGet]
        [Route("[action]/{PersonId}")]
        public async Task<IActionResult> Edit(Guid PersonId)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonId(PersonId);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }
            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryId.ToString() });

            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{PersonId}")]
        public async Task <IActionResult> Edit(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonId(personUpdateRequest.PersonId);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = await _countriesService.GetAllCountries();
                ViewBag.Countries = countries.Select(c => new SelectListItem { Text = c.CountryName, Value = c.CountryId.ToString() });
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                return View(personResponse.ToPersonUpdateRequest());
            }
            else
            {
                PersonResponse updatedPerson = await _personsService.UpdatePerson(personUpdateRequest);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Route("[action]/{PersonId}")]
        public async Task <IActionResult> Delete(Guid? PersonId)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonId(PersonId);
            if (personResponse == null) 
                return RedirectToAction("Index");
            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{PersonId}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateResult)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByPersonId(personUpdateResult.PersonId);
            if(personResponse == null)
                return RedirectToAction("Index");

            await _personsService.DeletePerson(personUpdateResult.PersonId);
            return RedirectToAction("Index");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsPDF()
        { 
            List<PersonResponse> persons = await _personsService.GetAllPersons();

            return new ViewAsPdf("PersonsPDF", persons, ViewData) { 
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 20, 20, 20),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsCSV()
        { 
            MemoryStream memoryStream = await _personsService.GetPersonsCSV();
            return File(memoryStream, "application/octet-stream", "persons.csv");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsExcel()
        {
            MemoryStream memoryStream = await _personsService.GetPersonsExcel();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }


    }
}
