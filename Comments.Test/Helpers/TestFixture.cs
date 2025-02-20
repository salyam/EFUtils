using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Salyam.EFUtils.Comments.Test.Helpers;

public sealed class TestFixture : IDisposable
{
    public string ConnectionString { get; private set; }
    public ServiceCollection Services { get; private set; } = new ServiceCollection();
    public List<Book> SeededData { get; } = [
        new Book() { Id = 1, Title = "1984",  Author = "George Orwell"},
        new Book() { Id = 2, Title = "To Kill a Mockingbird", Author =  "Harper Lee"}
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