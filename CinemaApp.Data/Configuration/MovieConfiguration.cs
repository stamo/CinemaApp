using CinemaApp.Data.Models;
using static CinemaApp.Common.Constants.EntityConstants.Movie;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaApp.Data.Configuration
{
    internal class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> entity)
        {
            // Define the primary key of the Movie entity
            entity
                .HasKey(m => m.Id);

            // Define constraints for the Title column
            entity
                .Property(m => m.Title)
                .IsRequired()
                .HasMaxLength(TitleMaxLength);

            // Define constraints for the Genre column
            entity
                .Property(m => m.Genre)
                .IsRequired()
                .HasMaxLength(GenreMaxLength);

            // Define constraints for the ReleaseDate column
            entity
                .Property(m => m.ReleaseDate)
                .IsRequired();

            // Define constraints for the Director column
            entity
                .Property(m => m.Director)
                .IsRequired()
                .HasMaxLength(DirectorMaxLength);

            // Define constraints for the Duration column
            entity
                .Property(m => m.Duration)
                .IsRequired();

            // Define constraints for the Description column
            entity
                .Property(m => m.Description)
                .IsRequired()
                .HasMaxLength(DescriptionMaxLength);

            // Define constraints for the ImageUrl column
            entity
                .Property(m => m.ImageUrl)
                .IsRequired(false)
                .HasMaxLength(ImageUrlMaxLength);

            // Define constraints for the IsDeleted column
            entity
                .Property(m => m.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Ensure that only existing records are used in the business logic
            entity
                .HasQueryFilter(m => m.IsDeleted == false);
        }
    }
}
