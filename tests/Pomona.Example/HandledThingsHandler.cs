#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;

using Nancy;

using Pomona.Example.Models;

namespace Pomona.Example
{
    public class HandledThingsHandler
    {
        private readonly CritterRepository repository;


        public HandledThingsHandler(CritterRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            this.repository = repository;
        }


        public void Delete(HandledThing handledThing)
        {
            this.repository.Delete(handledThing);
        }


        public HandledThing Get(int id)
        {
            var thing = this.repository.Query<HandledThing>().First(x => x.Id == id);
            thing.FetchedCounter++;
            return thing;
        }


        public HandledThing Patch(HandledThing handledThing)
        {
            handledThing.PatchCounter++;
            return this.repository.Save(handledThing);
        }


        public HandledSingleChild Patch(HandledThing parent, HandledSingleChild child)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            child.PatchHandlerCalled = true;

            return child;
        }


        public HandledChild Post(HandledThing parent, HandledChild postedChild)
        {
            if (parent != postedChild.Parent)
                throw new InvalidOperationException("Parent was not set correctly for posted child.");

            postedChild.HandlerWasCalled = true;
            this.repository.Post(postedChild);
            return postedChild;
        }


        public HandledThing Post(HandledThing handledThing, PomonaContext context)
        {
            if (handledThing == null)
                throw new ArgumentNullException(nameof(handledThing));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            handledThing.Marker = "HANDLER WAS HERE!";
            return (HandledThing)this.repository.Post(handledThing);
        }


        public IQueryable<HandledThing> QueryHandledThings(NancyContext nancyContext)
        {
            if (nancyContext.Request.Url.Path != "/handled-things")
                throw new InvalidOperationException("Should not be used for getting resource by id..");
            return this.repository.Query<HandledThing>().ToList().Select(x =>
            {
                x.QueryCounter++;
                return x;
            }).AsQueryable();
        }
    }
}

