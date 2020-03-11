namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    /// <summary>
    /// A set of commonly used constants.
    /// </summary>
    public static class Scopes
    {
        /// <summary>
        /// Represents the scope of a function trigger request.
        /// </summary>
        public const string RootLifetimeScopeTag = "AutofacFunctionsScope";
    }
}
