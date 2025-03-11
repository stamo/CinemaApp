namespace CinemaApp.Data.Models
{
    public class ApplicationUserMovie
    {
        public Guid ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; } = null!;

        public Guid MovieId { get; set; }

        public Movie Movie { get; set; } = null!;
    }
}
