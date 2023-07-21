using Entities;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ICountriesRepository _countriesRepository;

        public CountriesService(ICountriesRepository countriesRepository)
        { 
            _countriesRepository = countriesRepository;
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            if (countryAddRequest == null)
            {
                throw new ArgumentNullException(nameof(countryAddRequest));
            }
            if (countryAddRequest.CountryName == null)
            { 
                throw new ArgumentException(nameof(countryAddRequest.CountryName));
            }
            if (await _countriesRepository.GetCountryByCountryName(countryAddRequest.CountryName) != null)
            {
                throw new ArgumentException("Country with the given name is already exists");
            }

            //Convert CountryRequest to country
            Country country = countryAddRequest.ToCountry();
            country.CountryId = Guid.NewGuid();

            await _countriesRepository.AddCountry(country);

            //Convert country to response
            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            return (await _countriesRepository.GetAllCountries()).Select(c => c.ToCountryResponse()).ToList();
        }

        public async Task<CountryResponse?> GetCountryByCountryId(Guid? countryId)
        {
            if (countryId == null)
                return null;

            Country? country_from_list = await _countriesRepository.GetCountryByCountryId(countryId.Value);

            if (country_from_list == null)
                return null;

            return country_from_list.ToCountryResponse();
        }

        public async Task<int> UploadCountryFromExcelFile(IFormFile formFile)
        {
            MemoryStream memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);

            int countriesInserted = 0;
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["Countries"];

                int rowCount = worksheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++)
                {
                    string? cellValue = Convert.ToString(worksheet.Cells[row, 1].Value);
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        string? countryName = cellValue;
                        if (_countriesRepository.GetCountryByCountryName(countryName) == null)
                        {
                            Country country = new Country { CountryName = countryName };
                            await _countriesRepository.AddCountry(country);
                            countriesInserted++;
                        }
                    }
                }
            }
            return countriesInserted;
        }
    }
}