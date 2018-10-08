using Microsoft.Extensions.DependencyInjection;
using System;

namespace DHaven.Faux.DependencyInjection
{
    internal static class FauxCollectionExtensions
    {
        /// <summary>
        /// Only for use with FauxCollection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        internal static IServiceProvider BuildFauxServiceProvider(this IServiceCollection collection)
        {
            return new FauxServiceProvider(collection);
        }
    }
}
