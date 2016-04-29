#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Nancy;

using Pomona.Example.Models;

namespace Pomona.Example
{
    public class AsyncHandledThingsHandler
    {
        private readonly CritterRepository repository;


        public AsyncHandledThingsHandler(CritterRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            this.repository = repository;
        }


        public Task Delete(HandledThing handledThing)
        {
            return Task.Run(() => this.repository.Delete(handledThing));
        }


        public Task<HandledThing> Get(int id)
        {
            return Task.Run(() =>
            {
                var thing = this.repository.Query<HandledThing>().First(x => x.Id == id);
                thing.FetchedCounter++;
                return thing;
            });
        }


        public Task<HandledThing> Patch(HandledThing handledThing)
        {
            if (handledThing == null)
                throw new ArgumentNullException(nameof(handledThing));
            return Task.Run(() =>
            {
                handledThing.PatchCounter++;
                return this.repository.Save(handledThing);
            });
        }


        public Task<HandledSingleChild> Patch(HandledThing parent, HandledSingleChild child)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            return Task.Run(() =>
            {
                child.PatchHandlerCalled = true;
                return child;
            });
        }


        public Task<HandledChild> Post(HandledThing parent, HandledChild postedChild)
        {
            if (parent != postedChild.Parent)
                throw new InvalidOperationException("Parent was not set correctly for posted child.");
            return Task.Run(() =>
            {
                postedChild.HandlerWasCalled = true;
                this.repository.Post(postedChild);
                return postedChild;
            });
        }


        public Task<HandledThing> Post(HandledThing handledThing, PomonaContext context)
        {
            if (handledThing == null)
                throw new ArgumentNullException(nameof(handledThing));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return Task.Run(() =>
            {
                handledThing.Marker = "HANDLER WAS HERE!";
                return (HandledThing)this.repository.Post(handledThing);
            });
        }


        public Task<IQueryable<HandledThing>> QueryHandledThings(NancyContext nancyContext)
        {
            if (nancyContext.Request.Url.Path != "/handled-things")
                throw new InvalidOperationException("Should not be used for getting resource by id..");
            return Task.Run(() =>
            {
                return this.repository.Query<HandledThing>().ToList().Select(x =>
                {
                    x.QueryCounter++;
                    return x;
                }).AsQueryable();
            });
        }
    }
}