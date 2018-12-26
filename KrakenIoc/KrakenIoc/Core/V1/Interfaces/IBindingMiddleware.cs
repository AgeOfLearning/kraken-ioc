using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOFL.KrakenIoc.Core.V1.Interfaces
{
    public interface IBindingMiddleware
    {
        object Resolve(IBinding binding, object target = null);
        object Resolve(IBinding binding, IInjectContext injectContext, object target = null);
    }
}
