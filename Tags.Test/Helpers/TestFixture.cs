using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFTagTest.Helpers;

public sealed class TestFixture : IDisposable
{
    public string ConnectionString { get; private set; }
    public ServiceCollection Services { get; private set; } = new ServiceCollection();
    public readonly List<Book> SeededBooks =
    [
        new Book() { Id = 1, Title = "1984", Author = "George Orwell" },
        new Book() { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee" },
        new Book() { Id = 3, Title = "Pride and Prejudice", Author = "Jane Austen" },
        new Book() { Id = 4, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald" },
        new Book() { Id = 5, Title = "Moby-Dick", Author = "Herman Melville" },
        new Book() { Id = 6, Title = "The Catcher in the Rye", Author = "J.D. Salinger" },
        new Book() { Id = 7, Title = "The Hobbit", Author = "J.R.R. Tolkien" },
        new Book() { Id = 8, Title = "Brave New World", Author = "Aldous Huxley" },
        new Book() { Id = 9, Title = "Fahrenheit 451", Author = "Ray Bradbury" },
        new Book() { Id = 10, Title = "Animal Farm", Author = "George Orwell" },
        new Book() { Id = 11, Title = "The Lord of the Rings", Author = "J.R.R. Tolkien" },
        new Book() { Id = 12, Title = "Harry Potter and the Sorcerer's Stone", Author = "J.K. Rowling" },
        new Book() { Id = 13, Title = "Crime and Punishment", Author = "Fyodor Dostoevsky" },
        new Book() { Id = 14, Title = "The Brothers Karamazov", Author = "Fyodor Dostoevsky" },
        new Book() { Id = 15, Title = "Don Quixote", Author = "Miguel de Cervantes" },
        new Book() { Id = 16, Title = "Jane Eyre", Author = "Charlotte Brontë" },
        new Book() { Id = 17, Title = "Meditations", Author = "Marcus Aurelius" },
        new Book() { Id = 18, Title = "Frankenstein", Author = "Mary Shelley" },
        new Book() { Id = 19, Title = "Wuthering Heights", Author = "Emily Brontë" },
        new Book() { Id = 20, Title = "The Adventures of Huckleberry Finn", Author = "Mark Twain" }
    ];
    public readonly List<Article> SeededArticles =
    [
        new Article() { Id = 1, Title = "The Rise of Artificial Intelligence", Content = "Artificial Intelligence (AI) is transforming industries and creating new opportunities. Learn about its impact and potential.", Author = "Jane Smith" },
        new Article() { Id = 2, Title = "The Benefits of Mindfulness", Content = "Discover how practicing mindfulness can improve your mental health and overall well-being.", Author = "John Doe" },
        new Article() { Id = 3, Title = "Exploring the Universe", Content = "An overview of humanity's exploration of space and the discovery of exoplanets.", Author = "Dr. Emily Carter" },
        new Article() { Id = 4, Title = "The Future of Renewable Energy", Content = "A closer look at how solar, wind, and other renewable energy sources are shaping the future of sustainability.", Author = "Mark Taylor" },
        new Article() { Id = 5, Title = "Healthy Eating: A Beginner's Guide", Content = "Tips and recommendations for starting a healthy eating journey to improve your quality of life.", Author = "Sarah Lee" },
        new Article() { Id = 6, Title = "The History of the Internet", Content = "Tracing the origins of the internet and how it has revolutionized communication.", Author = "Michael Brown" },
        new Article() { Id = 7, Title = "The Impact of Climate Change", Content = "An analysis of the effects of climate change and what steps can be taken to mitigate its impacts.", Author = "Dr. Rachel Green" },
        new Article() { Id = 8, Title = "Understanding Blockchain Technology", Content = "A beginner's guide to blockchain technology and its applications beyond cryptocurrencies.", Author = "James Wilson" },
        new Article() { Id = 9, Title = "The Art of Public Speaking", Content = "Learn how to conquer your fear of public speaking and deliver impactful presentations.", Author = "Amanda Clark" },
        new Article() { Id = 10, Title = "The Psychology of Happiness", Content = "Explore the science behind happiness and how to create a more fulfilling life.", Author = "Dr. Karen Foster" },
        new Article() { Id = 11, Title = "The Evolution of Smartphones", Content = "A journey through the history and rapid evolution of smartphones over the last two decades.", Author = "Chris Johnson" },
        new Article() { Id = 12, Title = "Time Management Tips for Success", Content = "Expert strategies for managing your time effectively and boosting productivity.", Author = "Elizabeth Bennett" },
        new Article() { Id = 13, Title = "The Mysteries of the Deep Ocean", Content = "Uncovering the secrets of Earth's final frontier: the deep ocean.", Author = "Dr. Robert Lang" },
        new Article() { Id = 14, Title = "Cultural Influences on Modern Fashion", Content = "How culture and history influence modern fashion trends around the globe.", Author = "Emily Sanders" },
        new Article() { Id = 15, Title = "The Basics of Personal Finance", Content = "A beginner-friendly guide to budgeting, saving, and building financial stability.", Author = "David Kim" },
    ];
    public readonly List<IdentityUser> SeededUsers = 
    [
        new IdentityUser() { Id = "8f1c1de5-a9c0-47da-8ba3-ef2bbbd62251", UserName = "Alice", Email = "alice@example.com" },
        new IdentityUser() { Id = "35b0eeb8-4885-4800-9b3b-d74bcbf1d905", UserName = "Bob", Email = "bob@example.com" },
        new IdentityUser() { Id = "e81d92ec-315d-49f0-a766-9d3a1554e712", UserName = "Cecile", Email = "cecile@example.com" },
    ];

    public TestFixture()
    {
        var dbName = $"test_db_{Guid.NewGuid()}.db";
        var folder = Path.Combine(Path.GetTempPath(), "efutils");
        Directory.CreateDirectory(folder);
        var dbPath = Path.Combine(folder, dbName);
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
        dbContext.Books.AddRange(SeededBooks);
        dbContext.Articles.AddRange(SeededArticles);
        dbContext.Users.AddRange(SeededUsers);
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