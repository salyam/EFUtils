using System;

namespace Salyam.EFUtils.Comments.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommentableAttribute : Attribute
    {
        public CommentableAttribute(Type commenterType)
        {
            this.CommenterType = commenterType;
        }

        public Type CommenterType { get; set; }
    }
}
