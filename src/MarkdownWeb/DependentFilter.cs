using System;

namespace MarkdownWeb
{
    /// <summary>
    ///     Tells that a certain filter is dependent of another one.
    /// </summary>
    public class DependentOnFilterAttribute : Attribute
    {
        public DependentOnFilterAttribute(params Type[] dependencyTypes)
        {
            DependencyTypes = dependencyTypes;
        }

        public Type[] DependencyTypes { get; private set; }
    }
}