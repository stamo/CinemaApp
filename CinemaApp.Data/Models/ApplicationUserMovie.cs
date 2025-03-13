using Microsoft.EntityFrameworkCore;

namespace CinemaApp.Data.Models
{
    [Comment("Movie watchlist for system user")]
    public class ApplicationUserMovie
    {
        [Comment("Foreign key to the user")]
        public Guid ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; } = null!;

        [Comment("Foreign key to the movie")]
        public Guid MovieId { get; set; }

        public Movie Movie { get; set; } = null!;
    }
}
