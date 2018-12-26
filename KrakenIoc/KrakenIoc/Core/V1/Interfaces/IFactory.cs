namespace AOFL.KrakenIoc.Core.V1.Interfaces
{
    public interface IFactory
    {
        object Create(IInjectContext injectionContext);
    }

    public interface IFactory<T> : IFactory
    {
        new T Create(IInjectContext injectionContext);
    }
}
