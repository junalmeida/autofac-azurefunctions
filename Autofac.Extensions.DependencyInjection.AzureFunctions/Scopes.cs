using Autofac.Builder;
using System;
using System.Linq;

namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    /// <summary>
    /// A set of commonly used constants and extensions.
    /// </summary>
    public static class Scopes
    {
        /// <summary>
        /// Represents the scope of a function trigger request.
        /// </summary>
        public const string RootLifetimeScopeTag = "AutofacFunctionsScope";

        /// <summary>
        /// Share one instance of the component within the context of a single
        /// azure function trigger request.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="lifetimeScopeTags">Additional tags applied for matching lifetime scopes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> InstancePerTriggerRequest<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, params object[] lifetimeScopeTags)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            var tags = new[] { Scopes.RootLifetimeScopeTag }.Concat(lifetimeScopeTags).ToArray();
            return registration.InstancePerMatchingLifetimeScope(tags);
        }

    }
}
