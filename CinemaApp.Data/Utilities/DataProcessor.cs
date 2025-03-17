using System.Text;

using CinemaApp.Data.Dtos;
using CinemaApp.Data.Models;
using CinemaApp.Data.Utilities.Interfaces;
using static CinemaApp.Common.OutputMessages.ErrorMessages;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CinemaApp.Data.Utilities
{
    // TODO: Refactor this class into separate seeders to improve single responsibility
    // This class is a fatty, we should avoid such classes
    public class DataProcessor
    {
        private readonly IValidator entityValidator;
        private readonly IXmlHelper xmlHelper;
        private readonly ILogger<DataProcessor> logger;

        public DataProcessor(IValidator entityValidator, IXmlHelper xmlHelper, 
            ILogger<DataProcessor> logger)
        {
            this.entityValidator = entityValidator;
            this.xmlHelper = xmlHelper;
            this.logger = logger;
        }

        public async Task ImportMoviesFromJson(CinemaDbContext context)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Files", "movies.json");
            string moviesStr = await File.ReadAllTextAsync(path);
            var movies = JsonSerializer.Deserialize<List<Movie>>(moviesStr);

            if (movies != null && movies.Count > 0)
            {
                List<Guid> moviesIds = movies.Select(m => m.Id).ToList();
                if (await context.Movies.AnyAsync(m => moviesIds.Contains(m.Id)) == false)
                {
                    await context.Movies.AddRangeAsync(movies);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task ImportCinemasMoviesFromJson(CinemaDbContext context)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Files", "cinemasMovies.json");
            string cinemasMoviesStr = await File.ReadAllTextAsync(path);

            try
            {
                CinemaMovieDto[]? cinemaMovieDtos =
                JsonSerializer.Deserialize<CinemaMovieDto[]>(cinemasMoviesStr);
                if (cinemaMovieDtos != null && cinemaMovieDtos.Length > 0)
                {
                    ICollection<CinemaMovie> validCinemaMovies = new List<CinemaMovie>();
                    foreach (CinemaMovieDto cinemaMovieDto in cinemaMovieDtos)
                    {
                        if (!this.entityValidator.IsValid(cinemaMovieDto))
                        {
                            // Log the message
                            this.logger
                                .LogWarning(this.BuildEntityValidatorWarningMessage(nameof(CinemaMovie)));

                            // Skip the current DTO instance
                            continue;
                        }

                        string[] cinemaInfo = cinemaMovieDto
                            .Cinema
                            .Split(" - ", StringSplitOptions.RemoveEmptyEntries);
                        string cinemaName = cinemaInfo[0];
                        string? cinemaLocation = cinemaInfo.Length > 1 ?
                            cinemaInfo[1] : null;

                        // Build the query for extracting Cinema using Query Tree
                        IQueryable<Cinema> cinemaQuery = context
                            .Cinemas
                            .Where(c => c.Name == cinemaName);
                        if (cinemaLocation != null)
                        {
                            cinemaQuery = cinemaQuery
                                .Where(c => c.Location == cinemaLocation);
                        }

                        Cinema? cinema = await cinemaQuery
                            .SingleOrDefaultAsync();
                        Movie? movie = await context
                            .Movies
                            .FirstOrDefaultAsync(m => m.Title == cinemaMovieDto.Movie);
                        if (cinema == null || movie == null)
                        {
                            // Non-existing movie or cinema => cannot import the MovieCinema DTO!
                            string logMessage = string.Format(EntityImportError, nameof(CinemaMovie)) +
                                                ReferencedEntityMissing;

                            // Log warning message
                            this.logger.LogWarning(logMessage);

                            // Skip the current DTO instance
                            continue;
                        }

                        CinemaMovie? existingProjection = await context
                            .CinemaMovies
                            .FirstOrDefaultAsync(cm => cm.CinemaId == cinema.Id &&
                                                       cm.MovieId == movie.Id);
                        if (existingProjection != null &&
                            existingProjection.Showtimes == cinemaMovieDto.Showtimes)
                        {
                            // Log warning message
                            this.logger.LogWarning(EntityInstanceAlreadyExist);

                            // Skip the current DTO instance
                            continue;
                        }

                        CinemaMovie newCinemaMovie = new CinemaMovie()
                        {
                            CinemaId = cinema.Id,
                            MovieId = movie.Id,
                            AvailableTickets = cinemaMovieDto.AvailableTickets,
                            IsDeleted = cinemaMovieDto.IsDeleted,
                            Showtimes = cinemaMovieDto.Showtimes
                        };
                        validCinemaMovies.Add(newCinemaMovie);
                    }

                    await context.CinemaMovies.AddRangeAsync(validCinemaMovies);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                throw;
            }
        }

        public async Task ImportTicketsFromXml(CinemaDbContext context)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Files", "tickets.xml");
            string ticketsStr = await File.ReadAllTextAsync(path);

            try
            {
                TicketDto[]? ticketDtos = this.xmlHelper
                    .Deserialize<TicketDto[]>(ticketsStr, "Tickets");
                if (ticketDtos != null && ticketDtos.Length > 0)
                {
                    ICollection<Ticket> validTickets = new List<Ticket>();
                    foreach (TicketDto ticketDto in ticketDtos)
                    {
                        if (!this.entityValidator.IsValid(ticketDto))
                        {
                            // Log warning message
                            this.logger.LogWarning(this.BuildEntityValidatorWarningMessage(nameof(Ticket)));

                            // Skip current DTO instance
                            continue;
                        }

                        bool isPriceValid = decimal
                            .TryParse(ticketDto.Price, out decimal ticketPrice);
                        bool isMovieIdValid = Guid
                            .TryParse(ticketDto.MovieId, out Guid ticketMovieId);
                        bool isCinemaIdValid = Guid
                            .TryParse(ticketDto.CinemaId, out Guid ticketCinemaId);
                        bool isUserIdValid = Guid
                            .TryParse(ticketDto.UserId, out Guid ticketUserId);
                        if ((!isPriceValid) || (!isMovieIdValid) ||
                            (!isCinemaIdValid) || (!isUserIdValid))
                        {
                            string logMessage = string.Format(EntityImportError, nameof(Ticket)) + EntityDataParseError;

                            this.logger.LogWarning(logMessage);

                            continue;
                        }

                        CinemaMovie? ticketCinemaMovie = await context
                            .CinemaMovies
                            .SingleOrDefaultAsync(cm => cm.CinemaId == ticketCinemaId &&
                                                        cm.MovieId == ticketMovieId);
                        ApplicationUser? ticketUser = await context
                            .Users
                            .SingleOrDefaultAsync(u => u.Id == ticketUserId);
                        if (ticketUser == null || ticketCinemaMovie == null)
                        {
                            // Non-existing movie or cinema => cannot import the MovieCinema DTO!
                            string logMessage = string.Format(EntityImportError, nameof(Ticket)) +
                                                ReferencedEntityMissing;

                            // Log warning message
                            this.logger.LogWarning(logMessage);

                            // Skip the current DTO instance
                            continue;
                        }

                        Ticket newTicket = new Ticket()
                        {
                            Price = ticketPrice,
                            ApplicationUserId = ticketUserId,
                            CinemaMovieId = ticketCinemaMovie.Id
                        };
                        validTickets.Add(newTicket);
                    }

                    await context.Tickets.AddRangeAsync(validTickets);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                throw;
            }
        }

        public void SeedUsers(IServiceProvider serviceProvider)
        {
            UserManager<ApplicationUser> userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            this.SeedUser(userManager, "admin@example.com", "Admin@123", "Admin");
            this.SeedUser(userManager, "appManager@example.com", "123asd", "Manager");
            this.SeedUser(userManager, "appUser@example.com", "123asd", "User");
        }

        public void SeedRoles(IServiceProvider serviceProvider)
        {
            RoleManager<IdentityRole<Guid>> roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            string[] roles = { "Admin", "Manager", "User" };

            foreach (string role in roles)
            {
                bool roleExists = roleManager
                    .RoleExistsAsync(role)
                    .GetAwaiter()
                    .GetResult();
                if (!roleExists)
                {
                    IdentityResult result = roleManager
                        .CreateAsync(new IdentityRole<Guid>(role))
                        .GetAwaiter()
                        .GetResult();
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create role: {role}");
                    }
                }
            }
        }

        private void SeedUser(UserManager<ApplicationUser> userManager, string email, string password, string role)
        {
            ApplicationUser? user = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email
                };
                IdentityResult createUserResult = userManager
                    .CreateAsync(user, password)
                    .GetAwaiter()
                    .GetResult();
                if (!createUserResult.Succeeded)
                {
                    throw new Exception($"Failed to create user: {email}");
                }
            }

            bool isInRole = userManager
                .IsInRoleAsync(user, role)
                .GetAwaiter()
                .GetResult();
            if (!isInRole)
            {
                IdentityResult addRoleResult = userManager
                    .AddToRoleAsync(user, role)
                    .GetAwaiter()
                    .GetResult();
                if (!addRoleResult.Succeeded)
                {
                    throw new Exception($"Failed to assign {role} role to user: {email}");
                }
            }
        }

        private string BuildEntityValidatorWarningMessage(string entityName)
        {
            // Prepare log message with error messages from the validation
            StringBuilder logMessage = new StringBuilder();
            logMessage
                .AppendLine(string.Format(EntityImportError, entityName))
                .AppendLine(string.Join(Environment.NewLine, this.entityValidator.ErrorMessages));

            // Log the message
            return logMessage.ToString().TrimEnd();
        }
    }
}
