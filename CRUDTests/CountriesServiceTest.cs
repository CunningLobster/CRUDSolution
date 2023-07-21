using ServiceContracts;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;
using ServiceContracts.DTO;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCoreMock;
using Moq;
using RepositoryContracts;
using AutoFixture;
using FluentAssertions;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        private readonly ICountriesRepository _countriesRepository;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;

        private readonly IFixture _fixture;


        public CountriesServiceTest()
        {
            _fixture = new Fixture();

            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;

            var dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            _countriesService = new CountriesService(_countriesRepository);
        }

        #region AddCountry
        //When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry_ToBeArgumentNullException()
        { 
            //Arrange
            CountryAddRequest? request = null;

            //Act
            Func<Task> action = async () => await _countriesService.AddCountry(request);

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull_ToBeArgumentException()
        {
            //Arrange
            CountryAddRequest? country_add_request = _fixture.Build<CountryAddRequest>().With(c => c.CountryName, null as string).Create();

            Country country = country_add_request.ToCountry();
            _countriesRepositoryMock.Setup(c => c.AddCountry(It.IsAny<Country>())).ReturnsAsync(country);

            //Act
            Func<Task> action = async () => await _countriesService.AddCountry(country_add_request);

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }


        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsDuplicate_ToBeArgumentException()
        {
            //Arrange
            CountryAddRequest? request1 = _fixture.Build<CountryAddRequest>().With(c => c.CountryName, "USA").Create();
            CountryAddRequest? request2 = _fixture.Build<CountryAddRequest>().With(c => c.CountryName, "USA").Create();

            Country country1 = request1.ToCountry();
            Country country2 = request2.ToCountry();

            _countriesRepositoryMock.Setup(c => c.AddCountry(It.IsAny<Country>())).ReturnsAsync(country1);
            _countriesRepositoryMock.Setup(c => c.GetCountryByCountryName(It.IsAny<string>())).ReturnsAsync(country2);

            //Act
            Func<Task> action = async () => await _countriesService.AddCountry(request1);

            await action.Should().ThrowAsync<ArgumentException>();
        }


        //When the proper CountryName supplied, it should add the country to the existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryDetails_ToBeSuccessful()
        {
            //Arrange
            CountryAddRequest? country_add_request = _fixture.Create<CountryAddRequest>();

            Country country = country_add_request.ToCountry();
            CountryResponse country_response_expected = country.ToCountryResponse();

            _countriesRepositoryMock.Setup(c => c.AddCountry(It.IsAny<Country>())).ReturnsAsync(country);

            //Act
            CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);
            country_response_expected.CountryId = country_response_from_add.CountryId;

            //Assert
            country_response_expected.CountryId.Should().NotBe(Guid.Empty);
            country_response_from_add.Should().Be(country_response_expected);
        }
        #endregion

        #region GetAllCounries

        //When counry list is empty
        [Fact]
        public async Task GetAllCountries_EmptyList_ShouldReturnEmptyList()
        {
            //Arrange
            List<Country> countries = new List<Country>();
            _countriesRepositoryMock.Setup(c => c.GetAllCountries()).ReturnsAsync(countries);

            //Act
            List<CountryResponse> country_response_list = await _countriesService.GetAllCountries();

            //Assert
            country_response_list.Should().BeEmpty();
        }


        //When add few countries
        [Fact]
        public async Task GetAllCountries_FewCountries_ToBeSuccessful()
        {
            //Arrange
            List<Country> countries = _fixture.Build<Country>().With(c => c.Persons, null as ICollection<Person>).CreateMany(3).ToList();

            List<CountryResponse> county_response_list_expected = countries.Select(c => c.ToCountryResponse()).ToList();

            _countriesRepositoryMock.Setup(c => c.GetAllCountries()).ReturnsAsync(countries);

            //Act
            List<CountryResponse> country_response_list_actual = await _countriesService.GetAllCountries();

            //Assert
            country_response_list_actual.Should().BeEquivalentTo(county_response_list_expected);
        }

        #endregion

        #region GetCountryByCountryId

        //If we supply null as countryId
        [Fact]
        public async Task GetCountryByCountryId_NullCountryId_ShouldReturnNull()
        { 
            //Arrange
            Guid? countryId = null;

            //Act
            CountryResponse? country_response_from_get_method =  await _countriesService.GetCountryByCountryId(countryId);

            //Assert
            Assert.Null(country_response_from_get_method);
        }

        //If we supply a valid counryId, it should return the maching country details as CountryResponse object
        [Fact]
        public async Task GetCountryByCountryId_ValidCountryId_ShouldBeSuccessfull()
        {
            //Arrange
            Country country = _fixture.Build<Country>().With(c => c.Persons, null as ICollection<Person>).Create();
            CountryResponse country_response_expected = country.ToCountryResponse();

            _countriesRepositoryMock.Setup(c => c.GetCountryByCountryId(It.IsAny<Guid>())).ReturnsAsync(country);

            //Act
            CountryResponse? country_response_from_get = await _countriesService.GetCountryByCountryId(country_response_expected.CountryId);

            //Assert
            country_response_from_get.Should().Be(country_response_expected);
        }

            
        #endregion
    }
}
