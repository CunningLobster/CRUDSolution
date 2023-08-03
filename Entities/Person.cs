using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// Person domain model class
    /// </summary>
    public class Person
    {
        [Key]
        public Guid PersonId { get; set; }
        [StringLength(40)] //nvarchar(40)
        public string? PersonName { get; set; }
        [StringLength(40)]
        public string? Email{ get; set; }
        public DateTime? DateOfBirth{ get; set; }
        [StringLength(10)]
        public string? Gender{ get; set; }
        //uniqueidentifier
        public Guid? CountryId { get; set; } //Foreign key Property
        [StringLength(200)]
        public string? Address { get; set; }
        //bit
        public bool ReceiveNewsLetters{ get; set; }

        public string? TIN { get; set; }

        [ForeignKey("CountryId")]
        public Country? Country { get; set; } //Navigation Property

        public override string ToString()
        {
            return $"PersonId: {PersonId}, PersonName: {PersonName}, Email: {Email}, DateOfBirth: {DateOfBirth?.ToString("dd/mm/yyyy")}, Gender: {Gender}, CountryId: {CountryId}, Address: {Address}, ReceiveNewsLetters: {ReceiveNewsLetters}, TIN: {TIN}";
        }
    }
}
