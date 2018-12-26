using AOFL.KrakenIoc.Core.V1.Interfaces;

namespace AOFL.KrakenIoc.Core.V1
{
    public abstract class Factory<T> : IFactory<T>
    {
        public abstract T Create(IInjectContext injectionContext);

        object IFactory.Create(IInjectContext injectionContext)
        {
            return Create(injectionContext);
        }
    }
}
