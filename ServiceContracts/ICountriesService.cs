using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Country entity
    /// </summary>
    public interface ICountriesService
    {
        /// <summary>
        /// Add a country object to the list of countries
        /// </summary>
        /// <param name="countryAddRequest">Country object to add</param>
        /// <returns>Returns the country object after adding it (including newly generated country id)</returns>
        Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);

        /// <summary>
        /// Returns all countries from the List of Countries
        /// </summary>
        /// <returns>All countries from the list of Countries as CountryResponse list</returns>
        Task<List<CountryResponse>> GetAllCountries();

        /// <summary>
        /// Returns a country object by the giving country Id
        /// </summary>
        /// <param name="countryId">CountryId to search</param>
        /// <returns>Matching country as CountryResponse object</returns>
        Task<CountryResponse?> GetCountryByCountryId(Guid? countryId);

        /// <summary>
        /// Uploads countries from Excel file into database
        /// </summary>
        /// <param name="formFile">Excel file with list of countries</param>
        /// <returns>Number of countries added</returns>
        Task<int> UploadCountryFromExcelFile(IFormFile formFile);
    }
}