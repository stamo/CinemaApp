using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Data.Models
{
    public class Movie
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string Genre { get; set; } = null!;

        [Required]
        public DateTime ReleaseDate { get; set; }

        [Required]
        [MaxLength(150)]
        public string Director { get; set; } = null!;

        [Required]
        public int Duration { get; set; }

        [Required]
        [MaxLength(1024)]
        public string Description { get; set; } = null!;

        [MaxLength(256)]
        public string? ImageUrl { get; set; }

        public bool IsDeleted { get; set; }

        public List<CinemaMovie> MovieCinemas { get; set; } = new List<CinemaMovie>();

        public List<ApplicationUserMovie> MovieApplicationUsers { get; set; } = new List<ApplicationUserMovie>();

        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
