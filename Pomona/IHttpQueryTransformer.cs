using System;
using Nancy;

namespace Pomona
{
    /// <summary>
    /// This is the interface for the layer that transforms the query part
    /// of a HTTP request to a form that is sent to the data source.
    /// </summary>
    public interface IHttpQueryTransformer
    {
        IPomonaQuery TransformRequest(Request request, NancyContext nancyContext);
    }
}
