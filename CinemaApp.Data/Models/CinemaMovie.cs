using CinemaApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Data.Models
{
    public class CinemaMovie
    {
        public Guid MovieId { get; set; }

        public Movie Movie { get; set; } = null!;

        public Guid CinemaId { get; set; }

        public Cinema Cinema { get; set; } = null!;

        public int AvailableTickets { get; set; }

        public bool IsDeleted { get; set; }

        [Unicode(false)]
        [MaxLength(5)]
        public string Showtimes { get; set; } = "00000";

    }
}
