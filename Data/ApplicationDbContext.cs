using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Theatre> Theatres => Set<Theatre>();
    public DbSet<Screen> Screens => Set<Screen>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Show> Shows => Set<Show>();
    public DbSet<ShowSeat> ShowSeats => Set<ShowSeat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>(e =>
        {
            e.HasKey(m => m.MovieId);
            e.Property(m => m.Title).IsRequired().HasMaxLength(150);
            e.Property(m => m.Description).HasMaxLength(500);
            e.Property(m => m.DurationMinutes).IsRequired();
            e.Property(m => m.ReleaseDate).HasColumnType("date").IsRequired();
            e.Property(m => m.Language).IsRequired().HasMaxLength(50);
            e.Property(m => m.Genre).IsRequired().HasMaxLength(50);
            e.ToTable(t => t.HasCheckConstraint("CK_Movie_DurationMinutes", "[DurationMinutes] > 0"));
        });

        modelBuilder.Entity<Theatre>(e =>
        {
            e.HasKey(t => t.TheatreId);
            e.Property(t => t.Name).IsRequired().HasMaxLength(150);
            e.Property(t => t.Location).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Screen>(e =>
        {
            e.HasKey(s => s.ScreenId);
            e.Property(s => s.ScreenName).IsRequired().HasMaxLength(50);
            e.HasOne(s => s.Theatre).WithMany(t => t.Screens)
                .HasForeignKey(s => s.TheatreId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => new { s.TheatreId, s.ScreenName }).IsUnique();
        });

        modelBuilder.Entity<Seat>(e =>
        {
            e.HasKey(s => s.SeatId);
            e.Property(s => s.RowName).IsRequired().HasMaxLength(5);
            e.Property(s => s.SeatNumber).IsRequired();
            e.Property(s => s.SeatType).IsRequired().HasMaxLength(30);
            e.HasOne(s => s.Screen).WithMany(sc => sc.Seats)
                .HasForeignKey(s => s.ScreenId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => new { s.ScreenId, s.RowName, s.SeatNumber }).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_Seat_SeatNumber", "[SeatNumber] > 0"));
        });

        modelBuilder.Entity<Show>(e =>
        {
            e.HasKey(s => s.ShowId);
            e.Property(s => s.StartTime).IsRequired();
            e.Property(s => s.EndTime).IsRequired();
            e.Property(s => s.TicketPrice).HasColumnType("decimal(10,2)").IsRequired();
            e.HasOne(s => s.Movie).WithMany(m => m.Shows)
                .HasForeignKey(s => s.MovieId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Screen).WithMany(sc => sc.Shows)
                .HasForeignKey(s => s.ScreenId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Show_EndTime", "[EndTime] > [StartTime]");
                t.HasCheckConstraint("CK_Show_TicketPrice", "[TicketPrice] >= 0");
            });
        });

        modelBuilder.Entity<ShowSeat>(e =>
        {
            e.HasKey(ss => ss.ShowSeatId);
            e.Property(ss => ss.Status).IsRequired().HasMaxLength(20);
            e.HasOne(ss => ss.Show).WithMany(s => s.ShowSeats)
                .HasForeignKey(ss => ss.ShowId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ss => ss.Seat).WithMany(s => s.ShowSeats)
                .HasForeignKey(ss => ss.SeatId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(ss => new { ss.ShowId, ss.SeatId }).IsUnique();
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.BookingId);
            e.Property(b => b.BookingReference).IsRequired().HasMaxLength(30);
            e.Property(b => b.CustomerName).IsRequired().HasMaxLength(100);
            e.Property(b => b.CustomerEmail).IsRequired().HasMaxLength(150);
            e.Property(b => b.CustomerPhone).IsRequired().HasMaxLength(15);
            e.Property(b => b.BookingDate).IsRequired();
            e.Property(b => b.TotalAmount).HasColumnType("decimal(10,2)").IsRequired();
            e.Property(b => b.Status).IsRequired().HasMaxLength(20);
            e.HasIndex(b => b.BookingReference).IsUnique();
            e.HasOne(b => b.Show).WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ShowId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_Booking_TotalAmount", "[TotalAmount] >= 0"));
        });

        modelBuilder.Entity<BookingSeat>(e =>
        {
            e.HasKey(bs => bs.BookingSeatId);
            e.Property(bs => bs.Price).HasColumnType("decimal(10,2)").IsRequired();
            e.HasOne(bs => bs.Booking).WithMany(b => b.BookingSeats)
                .HasForeignKey(bs => bs.BookingId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(bs => bs.Seat).WithMany(s => s.BookingSeats)
                .HasForeignKey(bs => bs.SeatId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_BookingSeat_Price", "[Price] >= 0"));
        });
    }
}
