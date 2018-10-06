namespace DHaven.Faux
{
    /// <summary>
    /// Used to register all the Faux interfaces for your application.
    /// </summary>
    public interface IFauxRegistrar
    {
        /// <summary>
        /// Register a Faux service.
        /// </summary>
        /// <typeparam name="TService">the service type to register</typeparam>
        /// <returns>the registrar to chain registrations</returns>
        IFauxRegistrar Register<TService>()
            where TService : class;
    }
}
