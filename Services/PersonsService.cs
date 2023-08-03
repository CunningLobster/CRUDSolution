using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using Services.Helpers;
using ServiceContracts.Enums;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.ExternalReferences;
using Microsoft.AspNetCore.Http;
using RepositoryContracts;
using Microsoft.Extensions.Logging;
using Serilog;
using SerilogTimings;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        private readonly IPersonsRepository _personRepository;
        private readonly ILogger<PersonsService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public PersonsService(IPersonsRepository personRepository, ILogger<PersonsService> logger, IDiagnosticContext diagnosticContext)
        {
            _personRepository = personRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            if (personAddRequest == null)
                throw new ArgumentNullException(nameof(personAddRequest));

            //Model validations
            ValidationHelper.ModelValidation(personAddRequest);

            Person person = personAddRequest.ToPerson();
            person.PersonId = Guid.NewGuid();
            await _personRepository.AddPerson(person);
            //_db.sp_InsertPerson(person);

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons from PersonsService");

            var persons = await _personRepository.GetAllPersons();

            return persons.Select(p => p.ToPersonResponse()).ToList();
            //return _db.sp_GetAllPersons().Select(p => p.ToPersonResponse()).ToList();
        }

        public async Task<PersonResponse?> GetPersonByPersonId(Guid? personId)
        {
            if (personId == null)
                return null;

            Person? person = await _personRepository.GetPersonByPersonId(personId.Value);
            if (person == null)
                return null;

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            _logger.LogInformation("GetFilteredPersons from PersonsService");

            List<Person> persons = new List<Person>();
            using (Operation.Time("Time for GetFilteredPersons from PersonsService"))
            {
                persons = searchBy switch
                {
                    nameof(PersonResponse.PersonName) =>
                        await _personRepository.GetFilteredPersons(p => p.PersonName.Contains(searchString)),

                    nameof(PersonResponse.Email) =>
                        await _personRepository.GetFilteredPersons(p => p.Email.Contains(searchString)),

                    nameof(PersonResponse.DateOfBirth) =>
                        await _personRepository.GetFilteredPersons(p => p.DateOfBirth.Value.ToString("dd MMMM yyyy").Contains(searchString)),

                    nameof(PersonResponse.Gender) =>
                        await _personRepository.GetFilteredPersons(p => p.Gender.Contains(searchString)),

                    nameof(PersonResponse.CountryId) =>
                        await _personRepository.GetFilteredPersons(p => p.Country.CountryName.Contains(searchString)),

                    nameof(PersonResponse.Address) =>
                        await _personRepository.GetFilteredPersons(p => p.Address.Contains(searchString)),

                    _ =>
                        await _personRepository.GetAllPersons()
                }; 
            }
               
            _diagnosticContext.Set("Persons", persons);

            return persons.Select(p => p.ToPersonResponse()).ToList();

        }

        public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
        {
            _logger.LogInformation("GetSortedPersons from PersonsService");

            if (string.IsNullOrEmpty(sortBy))
                return allPersons;

            List<PersonResponse> sortedPersons = (sortBy, sortOrder)
            switch
            {
                (nameof(PersonResponse.PersonName), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.PersonName), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Email, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Email), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.DateOfBirth).ToList(),
                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.DateOfBirth).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Age).ToList(),
                (nameof(PersonResponse.Age), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Age).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Gender, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Gender), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Country, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Country), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.Address, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Address), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASC) => allPersons.OrderBy(p => p.ReceiveNewsLetters).ToList(),
                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESC) => allPersons.OrderByDescending(p => p.ReceiveNewsLetters).ToList(),

                _ => allPersons
            };

            return sortedPersons;
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(Person));

            //Validation
            ValidationHelper.ModelValidation(personUpdateRequest);

            //Get matching person object
            Person? matchingPerson = await _personRepository.GetPersonByPersonId(personUpdateRequest.PersonId);
            if (matchingPerson == null)
                throw new ArgumentException("Given person Id doesn't exist");

            //Update all details
            matchingPerson.PersonName = personUpdateRequest.PersonName;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;
            matchingPerson.CountryId = personUpdateRequest.CountryId;

            await _personRepository.UpdatePerson(matchingPerson);

            return matchingPerson.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personId)
        {
            if (personId == null)
                throw new ArgumentNullException(nameof(personId));

            Person? personToDelete = await _personRepository.GetPersonByPersonId(personId.Value);
            if (personToDelete == null)
                return false;

            await _personRepository.DeletePersonByPesonId(personId.Value);

            return true;
        }

        public async Task<MemoryStream> GetPersonsCSV()
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);

            csvWriter.WriteField(nameof(PersonResponse.PersonName));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            csvWriter.WriteField(nameof(PersonResponse.Gender));
            csvWriter.WriteField(nameof(PersonResponse.Country));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));
            csvWriter.NextRecord();

            List<PersonResponse> persons = await GetAllPersons();
            foreach (var person in persons)
            {
                csvWriter.WriteField(person.PersonName);
                csvWriter.WriteField(person.Email);
                if (person.DateOfBirth != null)
                    csvWriter.WriteField(person.DateOfBirth.Value.ToString("dd-mm-yyyy"));
                else
                    csvWriter.WriteField("");
                csvWriter.WriteField(person.Age);
                csvWriter.WriteField(person.Gender);
                csvWriter.WriteField(person.Country);
                csvWriter.WriteField(person.Address);
                csvWriter.WriteField(person.ReceiveNewsLetters);
                csvWriter.NextRecord();
                csvWriter.Flush();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Page 1");
                worksheet.Cells["A1"].Value = "Person Name";
                worksheet.Cells["B1"].Value = "Email";
                worksheet.Cells["C1"].Value = "Date Of Birth";
                worksheet.Cells["D1"].Value = "Age";
                worksheet.Cells["E1"].Value = "Gender";
                worksheet.Cells["F1"].Value = "Country";
                worksheet.Cells["G1"].Value = "Address";
                worksheet.Cells["H1"].Value = "Receive News Letters";

                using (ExcelRange headerCells = worksheet.Cells["A1:H1"] )
                {
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerCells.Style.Font.Bold = true;
                }

                int row = 2;
                List<PersonResponse> persons = await GetAllPersons();
                foreach (var person in persons)
                {
                    worksheet.Cells[row, 1].Value = person.PersonName;
                    worksheet.Cells[row, 2].Value = person.Email;
                    if(person.DateOfBirth.HasValue)
                        worksheet.Cells[row, 3].Value = person.DateOfBirth.Value.ToString("dd-mm-yyyy");
                    worksheet.Cells[row, 4].Value = person.Age;
                    worksheet.Cells[row, 5].Value = person.Gender;
                    worksheet.Cells[row, 6].Value = person.Country;
                    worksheet.Cells[row, 7].Value = person.Address;
                    worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;

                    row++;
                }

                worksheet.Cells[$"A1:H{row}"].AutoFitColumns();
                await excelPackage.SaveAsync();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
