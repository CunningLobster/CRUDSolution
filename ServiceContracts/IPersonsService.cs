using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonsService
    {
        /// <summary>
        /// Add a new person into existing list of persons
        /// </summary>
        /// <param name="personAddRequest">Person to add</param>
        /// <returns>Rerurns the same person details along with newly generated person Id</returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);

        /// <summary>
        /// Returns all persons
        /// </summary>
        /// <returns>Returns a list of objects of person response type</returns>
        Task<List<PersonResponse>> GetAllPersons();

        /// <summary>
        /// Returns the person object by the given person id
        /// </summary>
        /// <param name="personId">Person id to search</param>
        /// <returns>Returns matching person object</returns>
        Task<PersonResponse?> GetPersonByPersonId(Guid? personId);

        /// <summary>
        /// Returns all person objects that matches with the given search field and search string 
        /// </summary>
        /// <param name="searchBy">Search field to search</param>
        /// <param name="searchString">Search string to search</param>
        /// <returns>Returns all matching persons based on the given search string and search field</returns>
        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);

        /// <summary>
        /// Returns sorted list of persons
        /// </summary>
        /// <param name="allPersons">Represents list of perso to search</param>
        /// <param name="sortBy">Name of property based o which persons should be sorted</param>
        /// <param name="sortOrder">ASC or DESC</param>
        /// <returns>Returns sorted persons as PersonResponse list</returns>
        Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder);

        /// <summary>
        /// Update the specified Person details based on the given Person Id
        /// </summary>
        /// <param name="personUpdateRequest">Person details to update including Person Id</param>
        /// <returns>Returns the PersonResponse object after updation</returns>
        Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);

        /// <summary>
        /// Deletes a person based on the given person Id
        /// </summary>
        /// <param name="personId">PersonId to delete</param>
        /// <returns>Returns true if deletion is successful</returns>
        Task<bool> DeletePerson(Guid? personId);

        /// <summary>
        /// Returns persons as CSV
        /// </summary>
        /// <returns>Returns the memory stream with CSV persons data</returns>
        Task<MemoryStream> GetPersonsCSV();

        /// <summary>
        /// Returns persons as Excel
        /// </summary>
        /// <returns>Returns the memory stream with Excel persons data</returns>
        Task<MemoryStream> GetPersonsExcel();
    }
}
