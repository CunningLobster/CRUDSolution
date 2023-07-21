using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Person> Persons { get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Person>().ToTable("Persons");

            //Seed to Countries
            string countriesJson = File.ReadAllText("countries.json");
            List<Country>? countries = System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countriesJson);

            foreach(Country country in countries)
                modelBuilder.Entity<Country>().HasData(country);

            //Seed to Persons
            string personsJson = File.ReadAllText("persons.json");
            List<Person>? persons = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personsJson);

            foreach (Person person in persons)
                modelBuilder.Entity<Person>().HasData(person);

            //Fluent API
            modelBuilder.Entity<Person>().Property(p => p.TIN).HasColumnName("TaxIdentificationNumber").HasColumnType("varchar(8)").HasDefaultValue("ABC12345");

            //modelBuilder.Entity<Person>().HasIndex(p => p.TIN).IsUnique();

            modelBuilder.Entity<Person>().HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8");

            //Table Relations
            //modelBuilder.Entity<Person>(entity => { 
            //    entity.HasOne<Country>(p => p.Country).
            //    WithMany(c => c.Persons).
            //    HasForeignKey(p => p.CountryId);
            //});

        }

        public List<Person> sp_GetAllPersons() 
        { 
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
        }

        public int sp_InsertPerson(Person person)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@PersonId", person.PersonId),
                new SqlParameter("@PersonName", person.PersonName),
                new SqlParameter("@Email", person.Email),
                new SqlParameter("@DateOfBirth", person.DateOfBirth),
                new SqlParameter("@Gender", person.Gender),
                new SqlParameter("@CountryId", person.CountryId),
                new SqlParameter("@Address", person.Address),
                new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters)
            };

            return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonId, @PersonName, @Email, @DateOfBirth, @Gender, @CountryId, @Address, @ReceiveNewsLetters", sqlParameters);
        }
    }
}
