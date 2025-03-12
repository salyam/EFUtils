using Salyam.EFUtils.Comments.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Salyam.EFUtils.Comments;

public static class Extensions
{
    public static IServiceCollection AddEfCoreComments<DbContextType, CommentableType, CommenterType>(
        this IServiceCollection services)
        where DbContextType : DbContext, ICommentDbContext<CommentableType, CommenterType>
        where CommentableType : class
        where CommenterType : class
    {
        services.AddScoped<ICommentService<CommentableType, CommenterType>, CommentService<DbContextType, CommentableType, CommenterType>>();
        return services;
    }
}
