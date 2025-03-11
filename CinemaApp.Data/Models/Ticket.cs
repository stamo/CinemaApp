using CinemaApp.Common.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaApp.Data.Models
{
    public class Ticket
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = EntityConstants.MoneyType)]
        public decimal Price { get; set; }

        [Required]
        public Guid CinemaId { get; set; }

        public Cinema? Cinema { get; set; }

        [Required]
        public Guid MovieId { get; set; }

        public Movie? Movie { get; set; }

        [Required]
        public Guid ApplicationUserId { get; set; }

        public ApplicationUser? ApplicationUser { get; set; }
    }
}