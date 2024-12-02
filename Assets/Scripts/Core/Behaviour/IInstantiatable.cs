namespace Core.Behaviour {
    /// <summary>
    /// Interface for components that require initialization post-instantiation.
    /// </summary>
    public interface IInstantiatable {
        /// <summary>
        /// Initializes the component.
        /// </summary>
        void Init();
    }
}