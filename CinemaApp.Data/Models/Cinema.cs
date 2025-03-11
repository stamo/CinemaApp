using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Data.Models
{
    [Comment("Cinemas in the system")]
    public class Cinema
    {
        [Key]
        [Comment("Cinema identifier")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(256)]
        [Comment("Cinema name")]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(256)]
        [Comment("Cinema location")]
        public string Location { get; set; } = null!;

        [Comment("Shows if cinema is deleted")]
        public bool IsDeleted { get; set; }

        public List<CinemaMovie> CinemaMovies { get; set; } = new List<CinemaMovie>();

        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
