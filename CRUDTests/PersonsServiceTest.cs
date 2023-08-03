using Services;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using ServiceContracts;
using Xunit.Abstractions;
using Entities;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCoreMock;
using AutoFixture;
using FluentAssertions;
using RepositoryContracts;
using Moq;
using System.Linq.Expressions;
using Serilog.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Logging;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personsService;

        private readonly IPersonsRepository _personsRepository;
        private readonly Mock<IPersonsRepository> _personRepositoryMock;

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();

            _personRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personRepositoryMock.Object;
            var diagnosticContextMock = new Mock<IDiagnosticContext>();
            var loggerMock = new Mock<ILogger<PersonsService>>();

            var dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            _personsService = new PersonsService(_personsRepository, loggerMock.Object, diagnosticContextMock.Object);

            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson

        //When we supply a null value it returns a NullArgumentException
        [Fact]
        public async Task AddPerson_ToBeArgumentNullException()
        { 
            //Arrange
            PersonAddRequest? personAddRequest = null;

            //Act
            Func<Task> action = async () => await _personsService.AddPerson(personAddRequest);

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        //When we supply null value as PersonName, it should throw ArgumentException
        [Fact]
        public async Task AddPerson_PersonNameIsNull_ToBeArgumentException() 
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.PersonName, null as string).Create();

            Person person = personAddRequest.ToPerson();

            //When personRepository.AddPerson() has called, it has to return the same person object
            _personRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

            //Act
            Func<Task> action = async () => await _personsService.AddPerson(personAddRequest);

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When we supply proper persons details, it should insert the person in the persons list; and it should return an object of PersonResponse, which include with the newly generated person id
        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.Email, "someone@mail.com").Create();

            Person person = personAddRequest.ToPerson();
            PersonResponse person_response_expected = person.ToPersonResponse();

            //If we supply any argument value to the AddPerson method, it should return the same return value
            _personRepositoryMock.Setup(p => p.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

            //Act
            PersonResponse person_response_from_add = await _personsService.AddPerson(personAddRequest);
            person_response_expected.PersonId = person_response_from_add.PersonId;

            //Assert
            person_response_from_add.PersonId.Should().NotBe(Guid.Empty);
            person_response_from_add.Should().Be(person_response_expected);
        }


        #endregion

        #region GetPersonByPersonId

        //If we supply null as Person id, it dhould return null as PersonResponse
        [Fact]
        public async Task GetPersonByPersonId_PersonIdIsNull_ToBeNull()
        { 
            //Arrange
            Guid? personId = null;

            //Act
            PersonResponse? person_response_from_get = await _personsService.GetPersonByPersonId(personId);

            //Assert
            person_response_from_get.Should().BeNull();
        }

        //If we supply a valid person id, it shoul return a valid person details as PersonResponse object
        [Fact]
        public async Task GetPersonByPersonId_WithPersonId_ToBeSuccessful()
        {
            //Arrange
            Person person = _fixture.Build<Person>().With(p => p.Email, "someone@mail.com").With(p => p.Country, null as Country).Create();
            PersonResponse person_response_expected = person.ToPersonResponse();

            _personRepositoryMock.Setup(p => p.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            PersonResponse? person_response_from_get = await _personsService.GetPersonByPersonId(person.PersonId);

            //Assert
            person_response_from_get.Should().BeEquivalentTo(person_response_expected);
        }

        #endregion

        #region GetAllPersons

        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            //Arrange
            List<Person> persons = new List<Person>();
            _personRepositoryMock.Setup(p => p.GetAllPersons()).ReturnsAsync(persons);

            //Act
            List<PersonResponse> persons_from_get = await _personsService.GetAllPersons();

            //Assert
            persons_from_get.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllPersons_FewPersons_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = GetPersonsListFromAdd();
            List<PersonResponse> person_response_list_from_expected = persons.Select(p => p.ToPersonResponse()).ToList();

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (var person_response_from_add in person_response_list_from_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personRepositoryMock.Setup(p => p.GetAllPersons()).ReturnsAsync(persons);

            //Act
            List<PersonResponse> person_response_list_from_get = await _personsService.GetAllPersons();

            //print person_response_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (var person_response_from_get in person_response_list_from_get)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert
            person_response_list_from_get.Should().BeEquivalentTo(person_response_list_from_expected);
        }

        #endregion

        #region GetFilteredPersons

        //If the search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = GetPersonsListFromAdd();
            List<PersonResponse> person_response_list_expected = persons.Select(p => p.ToPersonResponse()).ToList();


            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (var person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personRepositoryMock.Setup(r => r.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

            //Act
            List<PersonResponse> person_response_list_from_search = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print person_response_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (var person_response_from_get in person_response_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert
            person_response_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
        }

        //Search by person name with some search string. It should return the matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = GetPersonsListFromAdd();
            List<PersonResponse> person_response_list_expected = persons.Select(p => p.ToPersonResponse()).ToList();

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (var person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personRepositoryMock.Setup(p => p.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

            //Act
            List<PersonResponse> person_response_list_from_search = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "ma");

            //print person_response_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (var person_response_from_get in person_response_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert
            person_response_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
        }

        #endregion

        #region GetSortedPersons

        //When we sort persons in DESC order, it should return pursons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = GetPersonsListFromAdd();
            List<PersonResponse> person_response_list_expected = persons.Select(p => p.ToPersonResponse()).ToList();

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (var person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personRepositoryMock.Setup(p => p.GetAllPersons()).ReturnsAsync(persons);

            List<PersonResponse> allPersons = await _personsService.GetAllPersons();

            //Act
            List<PersonResponse> person_response_list_from_sort = await _personsService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);

            //print person_response_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (var person_response_from_get in person_response_list_from_sort)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert
            person_response_list_from_sort.Should().BeInDescendingOrder(p => p.PersonName);
        }

        #endregion

        private List<Person> GetPersonsListFromAdd()
        {
            Person person1 = _fixture.Build<Person>()
                .With(p => p.Email, "someone_1@mail.com")
                .With(p => p.Country, null as Country)
                .Create();
            Person person2 = _fixture.Build<Person>()
                .With(p => p.Email, "someone_2@mail.com")
                .With(p => p.Country, null as Country)
                .Create();
            Person person3 = _fixture.Build<Person>()
                .With(p => p.Email, "someone_3@mail.com")
                .With(p => p.Country, null as Country)
                .Create();

            List<Person> persons = new List<Person> { person1, person2, person3 };

            return persons;
        }

        #region UpdatePerson

        //When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest person_update_request = null;

            //Act
            Func<Task> action = async () => await _personsService.UpdatePerson(person_update_request);

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        //When we supply invalid Person Id, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonId_ToBeArgumentException()
        {
            //Arrange
            PersonUpdateRequest person_update_request = _fixture.Create<PersonUpdateRequest>();

            //Act
            Func<Task> action = async () => await _personsService.UpdatePerson(person_update_request);

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When the PersonName is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException() 
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(p => p.PersonName, null as string)
                .With(p => p.Email, "someone@mail.com")
                .With(p => p.Country, null as Country)
                .With(p => p.Gender, "Male")
                .Create();
            PersonResponse person_response_from_add = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();

            //Act
            Func<Task> action = async () => await _personsService.UpdatePerson(person_update_request);

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDatails_ToBeSuccessful()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(p => p.Email, "someone@mail.com")
                .With(p => p.Country, null as Country)
                .With(p => p.Gender, "Male")
                .Create();
            PersonResponse person_response_from_expected = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_from_expected.ToPersonUpdateRequest();

            _personRepositoryMock.Setup(p => p.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
            _personRepositoryMock.Setup(p => p.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            PersonResponse person_response_from_update = await _personsService.UpdatePerson(person_update_request);

            //Assert
            person_response_from_update.Should().Be(person_response_from_expected);
        }

        #endregion

        #region DeletePerson

        //If you supply a valid PersonId, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonId_ShouldBeSuccessful()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(p => p.Email, "someone@mail.com")
                .With(p => p.Country, null as Country)
                .With(p => p.Gender, "Female")
                .Create();

            _personRepositoryMock.Setup(p => p.DeletePersonByPesonId(It.IsAny<Guid>())).ReturnsAsync(true);
            _personRepositoryMock.Setup(p => p.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            bool isDeleted = await _personsService.DeletePerson(person.PersonId);

            //Assert
            isDeleted.Should().BeTrue();
        }

        //If you supply an invalid PersonId, it should return true
        [Fact]
        public async Task DeletePerson_InvalidPersonId()
        {
            //Act
            bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());

            //Assert
            isDeleted.Should().BeFalse();
        }

        #endregion

    }
}
