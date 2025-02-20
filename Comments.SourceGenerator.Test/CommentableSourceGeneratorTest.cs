using CodeVerifier = Salyam.EFUtils.Comments.SourceGenerator.Test.CSharpSourceGeneratorVerifier<Salyam.EFUtils.Comments.SourceGenerator.CommentableSourceGenerator>;

namespace Salyam.EFUtils.Comments.SourceGenerator.Test;

public class CommentableSourceGeneratorTest
{
    private const string BookCode = 
        """
        namespace Utest.Example;

        public class User
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Password { get; set; }
        }

        [Salyam.EFUtils.Comments.Attributes.Commentable(typeof(User))]
        public partial class Book
        {
            public int Id { get; set; }

            public string Title { get; set; }

            public string Author { get; set; }
        }
        """;

    private const string DbContextCode = 
        """
        using Microsoft.EntityFrameworkCore;
        using System;

        namespace Utest.Example;

        public partial class TestDbContext : DbContext
        {
            public DbSet<Book> Books { get; set; }
            public DbSet<User> Users { get; set; }
        }
        """;

    [Fact]
    public async Task Given_ACompilationUnit_When_Generated_Should_GenerateAttribute()
    {
        // Arrange: set the expected generated code
        
        // Act & Assert: Run the code generator, check the generated source file's name, the generated source code an the number of error messages
        await new CodeVerifier.Test
        {
            TestState = 
            {
                Sources = { BookCode, DbContextCode },
                GeneratedSources =
                {
                    (
                        sourceGeneratorType: typeof(CommentableSourceGenerator), 
                        filename: "Book.CommentModel.Generated.cs", 
                        content: 
                        """
                        namespace Salyam.EFUtils.Comments._Models
                        {
                            [System.ComponentModel.DataAnnotations.Schema.Table("salyam.efutils.comments.comments")]
                            public class Comment_Book
                            {
                                public int Id { get; set; }
                        
                                [System.ComponentModel.DataAnnotations.MaxLength(65535)]
                                public string Text { get; set; }
                        
                                public Utest.Example.Book Entity { get; set; }
                                public System.Int32 EntityId { get; set; }
                        
                                public Utest.Example.User Commenter { get; set; }
                                public System.Nullable<System.Int32> CommenterId { get; set; }
                            }
                        }
                        """
                    ),
                    (
                        sourceGeneratorType: typeof(CommentableSourceGenerator), 
                        filename: "Book.CommentServiceInterface.Generated.cs",
                        content:
                        """
                        using System.Collections.Generic;
                        using System.Linq;
                        using System.Threading;
                        using System.Threading.Tasks;
                        
                        namespace Salyam.EFUtils.Comments
                        {
                            /// <summary>
                            /// Defines a service interface for managing comments associated with entities in the database.
                            /// </summary>
                            /// <typeparam name="EntityType">The type of the entity to which the comments will be applied.</typeparam>
                            public interface IBookCommentService
                            {
                                /// <summary>
                                /// Asynchronously adds a tag to a specified entity.
                                /// </summary>
                                /// <param name="entity">The entity to add the comment to.</param>
                                /// <param name="commenter">The commenter of the newly added comment.</param>
                                /// <param name="text">The text of the comment to be added.</param>
                                /// <param name="cancellationToken">A token to cancel the operation.</param>
                                /// <returns>A task representing the asynchronous operation.</returns>
                                Task AddCommentAsync(
                                    Utest.Example.Book entity,
                                    Utest.Example.User commenter,
                                    string text,
                                    CancellationToken cancellationToken = default
                                    );
                        
                                /// <summary>
                                /// Asynchronously removes a comment from a specified entity.
                                /// </summary>
                                /// <param name="comment">The comment to be removed.</param>
                                /// <param name="cancellationToken">A token to cancel the operation.</param>
                                /// <returns>A task representing the asynchronous operation.</returns>
                                Task RemoveCommentAsync(
                                    Salyam.EFUtils.Comments._Models.Comment_Book comment,
                                    CancellationToken cancellationToken = default
                                    );
                        
                                /// <summary>
                                /// Retrieves the tags associated with the specified entity.
                                /// </summary>
                                /// <param name="entity">The entity whose comments are to be retrieved.</param>
                                /// <param name="cancellationToken">A token to cancel the operation.</param>
                                /// <returns>A query object representing the list of comments.</returns>
                                IQueryable<Salyam.EFUtils.Comments._Models.Comment_Book> GetCommentsAsync(
                                    Utest.Example.Book entity,
                                    CancellationToken cancellationToken = default
                                    );
                            }
                        }
                        """
                    ),(
                        sourceGeneratorType: typeof(CommentableSourceGenerator), 
                        filename: "Book.CommentServiceImplementation.Generated.cs",
                        content:
                        """
                        using System.Collections.Generic;
                        using System.Linq;
                        using System.Threading;
                        using System.Threading.Tasks;
                        
                        namespace Salyam.EFUtils.Comments
                        {
                            public class BookCommentService(Utest.Example.TestDbContext db) : IBookCommentService
                            {
                                /// <inheritdoc />
                                public async Task AddCommentAsync(
                                    Utest.Example.Book entity,
                                    Utest.Example.User commenter,
                                    string text,
                                    CancellationToken cancellationToken
                                    )
                                {
                                    await db.Comments_Book.AddAsync(new () 
                                        {
                                            Text = text,
                                            EntityId = entity.Id,
                                            CommenterId = commenter?.Id
                                        },
                                        cancellationToken
                                    );
                                    await db.SaveChangesAsync(cancellationToken);
                                }
                        
                                /// <inheritdoc />
                                public async Task RemoveCommentAsync(
                                    Salyam.EFUtils.Comments._Models.Comment_Book comment,
                                    CancellationToken cancellationToken
                                    )
                                {
                                    db.Comments_Book.Remove(comment);
                                    await db.SaveChangesAsync(cancellationToken);
                                }
                        
                                /// <inheritdoc />
                                public IQueryable<Salyam.EFUtils.Comments._Models.Comment_Book> GetCommentsAsync(
                                    Utest.Example.Book entity,
                                    CancellationToken cancellationToken
                                    )
                                {
                                    return db.Comments_Book.Where(c => c.Entity.Id == entity.Id);
                                }
                            }
                        }
                        """
                    ),
                    (
                        sourceGeneratorType: typeof(CommentableSourceGenerator),
                        filename: "TestDbContext.CommentFieldExtensions.Generated.cs",
                        content:
                        """
                        using Microsoft.EntityFrameworkCore;
                        using Salyam.EFUtils.Comments._Models;

                        namespace Utest.Example
                        {
                            public partial class TestDbContext
                            {
                                public DbSet<Comment_Book> Comments_Book { get; set; }
                            }
                        }
                        """
                    ),
                    (
                        sourceGeneratorType: typeof(CommentableSourceGenerator),
                        filename: "ServiceCollection.CommentableExtension.Generated.cs",
                        content:
                        """
                        using Microsoft.Extensions.DependencyInjection;
                        
                        namespace Salyam.EFUtils.Comments
                        {
                            public static class ServiceCollectionExtension
                            {
                                public static IServiceCollection AddEfCoreComments(this IServiceCollection services)
                                {
                                    services.AddTransient<IBookCommentService, BookCommentService>();
                                    return services;
                                }
                            }
                        }
                        
                        """
                    )
                },
            },
        }.RunAsync();
    }
}
