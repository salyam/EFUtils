# EFUtils.Tags

`Salyam.EFUtils.Tags` is a library designed to simplify the management of tags and their associations with entities in Entity Framework Core. It provides a flexible and efficient way to implement tagging functionality within your applications.

## Features

- **Tag Management:** Easily add, remove, and retrieve tags associated with entities.
- **Case-Insensitive Tagging:** Supports case-insensitive tag matching and management.
- **Entity Filtering:** Efficiently filter entities based on their associated tags.
- **EF Core Integration:** Seamlessly integrates with Entity Framework Core DbContext.
- **Simple usage:** minimal configuration needed.

## Usage

### Prerequisites

- .NET 8 or higher.
- Entity Framework Core 6 or higher.
- The project using the library must be configured to use EF core

### Installation

This library is intended to be used as a Git submodule, not as a NuGet package.

1.  **Add as a Submodule:**

    ```bash
    git submodule add https://github.com/salyam/EFUtils.git <path/to/submodule>
    ```

    Replace `<path/to/submodule>` with the desired location in your project (e.g., `./libs/EFUtils.Tags`).
2. Ensure that the `EFUtils.Tags` project is included in the solution (`.sln`) file.
3. Add a project reference to the `EFUtils.Tags` project from the project you want to use it in.

### Setting Up

1.  **Create a `DbContext`:**

    Your `DbContext` must implement the `ITagDbContext<TaggableType>` interface, where `TaggableType` is the type of your taggable entity.

    ```csharp
    using Microsoft.EntityFrameworkCore;
    using Salyam.EFUtils.Tags;

    public class MyDbContext : DbContext, ITagDbContext<MyTaggableEntity>
    {
        public DbSet<TagModel> Tags { get; set; }
        public DbSet<TaggedEntityModel> TaggedEntities { get; set; }

        // ... other DbSets and configurations

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("your_connection_string");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configures relationships if needed.
        }
    }
    ```

2.  **Register the Tag Service:**

    In your `Startup.cs` or `Program.cs`, register the tag service using the `AddEfCoreTags` extension method.

    ```csharp
    using Microsoft.EntityFrameworkCore;
    using Salyam.EFUtils.Tags;
    using Salyam.EFUtils.Tags.Impl; //for internal classes
    // ...

    var builder = WebApplication.CreateBuilder(args);
    // ...

    builder.Services.AddDbContext<MyDbContext>();
    builder.Services.AddEfCoreTags<MyDbContext, MyTaggableEntity>();
    // or with custom lifetime
    // builder.Services.AddEfCoreTags<MyDbContext, MyTaggableEntity>(ServiceLifetime.Transient); 

    // ...
    ```
    Make sure you also register your `DbContext`.

3.  **Use the Tag Service:**

    Inject the `ITagService<TaggableType>` into your services and use its methods to manage tags.

    ```csharp
    using Salyam.EFUtils.Tags;
    // ...

    public class MyService(ITagService<MyTaggableEntity> tagService)
    {
        public async Task ManageTags(MyTaggableEntity entity)
        {
            // Set tags for an entity
            await tagService.SetTagsAsync(entity, ["tag1", "Tag2", "TAG3"]);

            // Get tags for an entity
            var tags = await tagService.GetTagsAsync(entity);

            // Get entities with specific tags
            var taggedEntities = tagService.GetTaggedEntitiesAsync(["tag1", "tag2"]);
        }
    }
    ```

## API Reference

- **`ITagDbContext<TaggableType>`:** Interface to be implemented by your `DbContext`.
  - `DbSet<TagModel> Tags`: DbSet for `TagModel`.
  - `DbSet<TaggedEntityModel> TaggedEntities`: DbSet for `TaggedEntityModel`.
- **`ITagService<TaggableType>`:** Interface for managing tags.
  - `Task SetTagsAsync(TaggableType entity, IEnumerable<string> tags, CancellationToken cancellationToken = default)`: Sets the tags for a given entity.
  - `Task<List<string>> GetTagsAsync(TaggableType entity, CancellationToken cancellationToken = default)`: Retrieves the tags for a given entity.
  - `IQueryable<TaggableType> GetTaggedEntitiesAsync(IEnumerable<string> tagNames)`: Retrieves entities tagged with any of the given tag names.