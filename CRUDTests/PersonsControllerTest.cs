using AutoFixture;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRUDExample.Controllers;
using ServiceContracts.Enums;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;
using Services;
using Microsoft.Extensions.Logging;

namespace CRUDTests
{
    public class PersonsControllerTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;

        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly Mock<IPersonsService> _personsServiceMock;
        private readonly Mock<ILogger<PersonsController>> _loggerMock;

        private readonly Fixture _fixture;

        public PersonsControllerTest()
        { 
            _fixture = new Fixture();

            _countriesServiceMock = new Mock<ICountriesService>();
            _personsServiceMock = new Mock<IPersonsService>();
            _loggerMock = new Mock<ILogger<PersonsController>>();

            _personsService = _personsServiceMock.Object;
            _countriesService = _countriesServiceMock.Object;
            _logger = _loggerMock.Object;
        }

        #region Index

        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonList()
        {
            //Arrange
            List<PersonResponse> persons_response_list = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            _personsServiceMock.Setup(p => p.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(persons_response_list);
            _personsServiceMock.Setup(p => p.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>())).ReturnsAsync(persons_response_list);

            //Act
            IActionResult result = await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<SortOrderOptions>());

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();
            viewResult.ViewData.Model.Should().Be(persons_response_list);
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_IfModelErrors_ToReturnCreateView()
        {
            //Arrange
            
            PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();
            PersonResponse person_response = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            _countriesServiceMock.Setup(c => c.GetAllCountries()).ReturnsAsync(countries);
            _personsServiceMock.Setup(p => p.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(person_response);

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            //Act
            personsController.ModelState.AddModelError("PersonName", "PersonName can't be blank");

            IActionResult result = await personsController.Create(person_add_request);

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result); 

            viewResult.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();
            viewResult.ViewData.Model.Should().Be(person_add_request);
        }

        [Fact]
        public async Task Create_IfNoModelErrors_RedirectToIndex()
        {
            //Arrange
            PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();
            PersonResponse person_response = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            _countriesServiceMock.Setup(c => c.GetAllCountries()).ReturnsAsync(countries);
            _personsServiceMock.Setup(p => p.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(person_response);

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            //Act
            IActionResult result = await personsController.Create(person_add_request);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);

            redirectResult.ActionName.Should().Be("Index");
        }

        #endregion

        #region Edit

        [Fact]
        public async Task Edit_PersonResponseNull_RedirectToIndex()
        {
            //Arrange
            PersonUpdateRequest person_update_request = _fixture.Create<PersonUpdateRequest>();
            PersonResponse person_response = null;

            _personsServiceMock.Setup(p => p.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person_response);

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);
            //Act
            IActionResult result = await personsController.Edit(person_update_request);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);

            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_IfNoModelErrors_RedirectToIndex()
        {
            //Arrange
            PersonUpdateRequest person_update_request = _fixture.Create<PersonUpdateRequest>();
            PersonResponse person_response = _fixture.Create<PersonResponse>();

            _personsServiceMock.Setup(p => p.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person_response);
            _personsServiceMock.Setup(p => p.UpdatePerson(It.IsAny<PersonUpdateRequest>())).ReturnsAsync(person_response);

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            //Act
            IActionResult result = await personsController.Edit(person_update_request);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);

            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_IfModelErrors_ToReturnEditView()
        {
            //Arrange
            PersonUpdateRequest person_update_request = _fixture.Create<PersonUpdateRequest>();
            PersonResponse person_response = _fixture.Build<PersonResponse>().With(p => p.Gender, "Male").Create();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>().ToList();

            _personsServiceMock.Setup(p => p.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person_response);
            _countriesServiceMock.Setup(c => c.GetAllCountries()).ReturnsAsync(countries);

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            //Act
            personsController.ModelState.AddModelError("PersonName", "PersonName can't be blank");

            IActionResult result = await personsController.Edit(person_update_request);

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            viewResult.Model.Should().BeAssignableTo<PersonUpdateRequest>();
            viewResult.Model.Should().BeEquivalentTo(person_response.ToPersonUpdateRequest());
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_PersonResponseIsNull_ToIndexView()
        {
            //Arrange
            PersonUpdateRequest person_update_result = _fixture.Create<PersonUpdateRequest>();
            PersonResponse? person_response = null;

            _personsServiceMock.Setup(p => p.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person_response);

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            //Act
            IActionResult result = await personsController.Delete(person_update_result);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_ProperPersonDetails_ToIndexView()
        {
            //Arrange
            PersonUpdateRequest person_update_result = _fixture.Create<PersonUpdateRequest>();
            PersonResponse? person_response = _fixture.Build<PersonResponse>().With(p => p.Gender, "Other").Create();

            _personsServiceMock.Setup(p => p.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person_response);
            _personsServiceMock.Setup(p => p.DeletePerson(It.IsAny<Guid>())).ReturnsAsync(true);

            PersonsController personsController = new PersonsController(_personsService, _countriesService, _logger);

            //Act
            IActionResult result = await personsController.Delete(person_update_result);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }


        #endregion
    }
}
