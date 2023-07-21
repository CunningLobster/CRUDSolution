using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContracts
{
    /// <summary>
    /// Represents the access logic for managing Person entity
    /// </summary>
    public interface IPersonsRepository
    {
        /// <summary>
        /// Adds person object to the data store
        /// </summary>
        /// <param name="person">Person object to add</param>
        /// <returns>Returns the person object after adding it to the table</returns>
        Task<Person> AddPerson(Person person);

        /// <summary>
        /// Returns all persons in the data store
        /// </summary>
        /// <returns>List of persons from the table</returns>
        Task<List<Person>> GetAllPersons();

        /// <summary>
        /// Retuens a person object on the given person Id
        /// </summary>
        /// <param name="personId">PersonId to search</param>
        /// <returns>A person object or null</returns>
        Task<Person?> GetPersonByPersonId(Guid personId);

        /// <summary>
        /// Returns all person objects based on the given expression
        /// </summary>
        /// <param name="predicate">LINQ expression to check</param>
        /// <returns>All matching persons with the given condition</returns>
        Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate);

        /// <summary>
        /// Deletes a person object on the person id
        /// </summary>
        /// <param name="PersonId">Person Id to search</param>
        /// <returns>true, if the deletion is successful</returns>
        Task<bool> DeletePersonByPesonId(Guid PersonId);

        /// <summary>
        /// Updates a person object based on the given person Id
        /// </summary>
        /// <param name="person">Person object to update</param>
        /// <returns>Returns the updated person object</returns>
        Task<Person> UpdatePerson(Person person);
    }
}
