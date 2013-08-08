using Nancy;

namespace Pomona
{
    public interface IHasNancyContext
    {
        NancyContext Context { get; set; }
    }
}