﻿using AutoFixture;
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

namespace CRUDTests
{
    public class PersonsControllerTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;

        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly Mock<IPersonsService> _personsServiceMock;

        private readonly Fixture _fixture;

        public PersonsControllerTest()
        { 
            _fixture = new Fixture();

            _countriesServiceMock = new Mock<ICountriesService>();
            _personsServiceMock = new Mock<IPersonsService>();

            _personsService = _personsServiceMock.Object;
            _countriesService = _countriesServiceMock.Object;
        }

        #region Index

        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonList()
        {
            //Arrange
            List<PersonResponse> persons_response_list = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

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
            
            PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();//WITH VALID VALUES FOR PROPERTIES
            PersonResponse person_response = _fixture.Create<PersonResponse>();//WITH VALID VALUES FOR PROPERTIES
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();//WITH VALID VALUES FOR PROPERTIES

            _countriesServiceMock.Setup(c => c.GetAllCountries()).ReturnsAsync(countries);
            _personsServiceMock.Setup(p => p.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(person_response);

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            //Act
            personsController.ModelState.AddModelError("PersonName", "PersonName can't be blank");//ERROR ADDED MANUALLY

            IActionResult result = await personsController.Create(person_add_request);//SO MODEL STATE IS NO LONGER VALID

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);//IN THIS CASE EVERYTHING WORKS FINE   

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

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

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

            _personsServiceMock.Setup(p => p.UpdatePerson(It.IsAny<PersonUpdateRequest>())).ReturnsAsync(person_response);

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

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

            _personsServiceMock.Setup(p => p.UpdatePerson(It.IsAny<PersonUpdateRequest>())).ReturnsAsync(person_response);

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

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
            PersonUpdateRequest person_update_request = _fixture.Build<PersonUpdateRequest>().Create<PersonUpdateRequest>();
            PersonResponse person_response = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            _personsServiceMock.Setup(p => p.UpdatePerson(It.IsAny<PersonUpdateRequest>())).ReturnsAsync(person_response);
            _countriesServiceMock.Setup(c => c.GetAllCountries()).ReturnsAsync(countries);

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            //Act
            personsController.ModelState.AddModelError("PersonName", "PersonName can't be blank");

            IActionResult result = await personsController.Edit(person_update_request);

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            viewResult.Model.Should().BeAssignableTo<PersonUpdateRequest>();
            viewResult.Model.Should().Be(person_update_request);
        }

        #endregion
    }
}