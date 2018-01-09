using System;
using MarkdownWeb.PostFilters;
using MarkdownWeb.PreFilters;

namespace MarkdownWeb
{
    /// <summary>
    ///     Tells that a certain filter is dependent of another one.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Used to control in which order filters are executed.
    ///     </para>
    /// </remarks>
    public class DependentOnFilterAttribute : Attribute
    {
        /// <summary>
        ///     Creates a new instance of <see cref="DependentOnFilterAttribute" />.
        /// </summary>
        /// <param name="dependencyTypes">
        ///     Types of <see cref="IPostFilter" /> or <see cref="IPreFilter" /> that this filter is
        ///     dependent upon.
        /// </param>
        public DependentOnFilterAttribute(params Type[] dependencyTypes)
        {
            DependencyTypes = dependencyTypes;
        }

        /// <summary>
        ///      Types of <see cref="IPostFilter" /> or <see cref="IPreFilter" /> that this filter is
        ///     dependent upon.
        /// </summary>
        public Type[] DependencyTypes { get; private set; }
    }
}