using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PersonsRepository> _logger;

        public PersonsRepository(ApplicationDbContext db, ILogger<PersonsRepository> logger) 
        {  
            _db = db; 
            _logger = logger;
        }

        public async Task<Person> AddPerson(Person person)
        {
            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            return person;
        }

        public async Task<bool> DeletePersonByPesonId(Guid PersonId)
        {
            _db.Persons.RemoveRange(_db.Persons.Where(p => p.PersonId == PersonId));
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<Person>> GetAllPersons()
        {
            return await _db.Persons.Include("Country").ToListAsync();
        }

        public async Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
        {
            _logger.LogInformation("GetFilteredPersons from PersonsRepository");

            return await _db.Persons.Include("Country").Where(predicate).ToListAsync();
        }

        public async Task<Person?> GetPersonByPersonId(Guid personId)
        {
            return await _db.Persons.Include("Country").FirstOrDefaultAsync(p => p.PersonId == personId);
        }

        public async Task<Person> UpdatePerson(Person person)
        {
            Person? matchngPerson = await _db.Persons.FirstOrDefaultAsync(p => p.PersonId == person.PersonId);

            if (matchngPerson == null) { return person; }

            matchngPerson.PersonName = person.PersonName;
            matchngPerson.Email = person.Email;
            matchngPerson.DateOfBirth = person.DateOfBirth;
            matchngPerson.Gender = person.Gender;
            matchngPerson.CountryId = person.CountryId;
            matchngPerson.Address = person.Address;
            matchngPerson.ReceiveNewsLetters = person.ReceiveNewsLetters;

            await _db.SaveChangesAsync();

            return matchngPerson;
        }
    }
}
