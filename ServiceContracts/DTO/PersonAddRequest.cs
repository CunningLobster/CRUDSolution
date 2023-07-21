using ServiceContracts.Enums;
using Entities;
using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO
{
    public class PersonAddRequest
    {
        [Required(ErrorMessage = "Person name can't be blank")]
        public string? PersonName { get; set; }
        [Required(ErrorMessage = "Email can't be blank")]
        [EmailAddress(ErrorMessage = "Email value should be a propper email")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [Required(ErrorMessage ="Please select a gender")]
        public GenderOptions? Gender { get; set; }
        public Guid? CountryId { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }

        public Person ToPerson()
        { 
            return new Person 
            { 
                PersonName = PersonName, 
                Email = Email, 
                DateOfBirth = DateOfBirth, 
                Gender = Gender.ToString(), 
                CountryId = CountryId, 
                Address = Address, 
                ReceiveNewsLetters = ReceiveNewsLetters 
            };
        }
    }
}
