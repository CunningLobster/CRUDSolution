using Entities;

namespace RepositoryContracts
{
    /// <summary>
    /// Represents the access logic for managing Country entity
    /// </summary>
    public interface ICountriesRepository
    {
        /// <summary>
        /// Add a country object to the data store
        /// </summary>
        /// <param name="country">Country object to add</param>
        /// <returns>Returns the country object after adding it to the data store</returns>
        Task<Country> AddCountry(Country country);

        /// <summary>
        /// Returns all countries in the data store
        /// </summary>
        /// <returns>All countries from the table</returns>
        Task<List<Country>> GetAllCountries();

        /// <summary>
        /// Returns a country object based on the give countryId
        /// </summary>
        /// <param name="countryId">CountryId to search</param>
        /// <returns>Matchng country or null</returns>
        Task<Country?> GetCountryByCountryId(Guid countryId);

        /// <summary>
        /// Returns a country object based on the give countryName
        /// </summary>
        /// <param name="countryName">Country name to search</param>
        /// <returns>Matchng country or null</returns>
        Task<Country?> GetCountryByCountryName(string countryName);


    }
}