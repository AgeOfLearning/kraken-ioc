namespace AOFL.KrakenIoc.Core.V1.Interfaces
{
    /// <summary>
    /// Represents a functional section of an application or feature.
    /// </summary>
    public interface IContext
    {
        IContainer Container { get; }

        void Inherit<T>() where T : IContext;
    }
}
