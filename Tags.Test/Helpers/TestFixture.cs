using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFTagTest.Helpers;

public sealed class TestFixture : IDisposable
{
    public string ConnectionString { get; private set; }
    public ServiceCollection Services { get; private set; } = new ServiceCollection();
    public List<Book> SeededData { get; } = [
        new Book() { Id = Guid.NewGuid().ToString(), Title = "1984", Author = "George Orwell" },
        new Book() { Id = Guid.NewGuid().ToString(), Title = "To Kill a Mockingbird", Author = "Harper Lee" },
        new Book() { Id = Guid.NewGuid().ToString(), Title = "Brave New World", Author = "Aldous Huxley" },
        new Book() { Id = Guid.NewGuid().ToString(), Title = "The Great Gatsby", Author = "F. Scott Fitzgerald" },
        new Book() { Id = Guid.NewGuid().ToString(), Title = "Moby-Dick", Author = "Herman Melville" },
        new Book() { Id = Guid.NewGuid().ToString(), Title = "Pride and Prejudice", Author = "Jane Austen" },
        new Book() { Id = Guid.NewGuid().ToString(), Title = "The Catcher in the Rye", Author = "J.D. Salinger" },
        new Book() { Id = Guid.NewGuid().ToString(), Title = "The Lord of the Rings", Author = "J.R.R. Tolkien" },
        new Book() { Id = Guid.NewGuid().ToString(), Title = "Fahrenheit 451", Author = "Ray Bradbury" },
        new Book() { Id = Guid.NewGuid().ToString(), Title = "The Hobbit", Author = "J.R.R. Tolkien" }
    ];

    public TestFixture()
    {
        var dbName = $"test_db_{Guid.NewGuid()}.db";
        var dbPath = Path.Combine(Path.GetTempPath(), dbName);
        ConnectionString = $"Data Source={dbPath}";

        ConfigureDbContext();
    }

    private void ConfigureDbContext()
    {
        Services.AddDbContext<TestDbContext>(options =>
            options.UseSqlite(ConnectionString), ServiceLifetime.Scoped);

        // Initialize and seed the database if needed here
        using var provider = Services.BuildServiceProvider();
        using var context = provider.GetRequiredService<TestDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        SeedDatabase(context);
    }

    private void SeedDatabase(TestDbContext dbContext)
    {
        // Example seed data
        dbContext.Books.AddRange(SeededData);
        dbContext.SaveChanges();
    }

    public void Dispose()
    {
        //this.Services.Clear();
        if (ConnectionString != null)
        {
            var dbPath = ConnectionString.Split("=", StringSplitOptions.RemoveEmptyEntries)[1];
            try
            {
                using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "PRAGMA optimize;";
                    command.ExecuteNonQuery();
                    command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                    command.ExecuteNonQuery();
                    command.CommandText = "PRAGMA journal_mode = DELETE;";
                    command.ExecuteNonQuery();
                }
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as required
                Console.WriteLine($"Error disposing database: {ex.Message}");
            }
            ConnectionString = "";
        }
    }
}