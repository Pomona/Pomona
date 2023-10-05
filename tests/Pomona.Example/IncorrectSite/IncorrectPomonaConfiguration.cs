#region License
// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/
#endregion

using System;
using System.Collections.Generic;

using Pomona.FluentMapping;

namespace Pomona.Example.IncorrectSite
{
    public class IncorrectPomonaConfiguration : PomonaConfigurationBase
    {
        // This configuration is incorrect, as IncorrectChildResource is missing in the list of SourceTypes.
        public override IEnumerable<Type> SourceTypes => new[] { typeof(IncorrectResource) };

        public override ITypeMappingFilter TypeMappingFilter => new IncorrectTypeMappingFilter(SourceTypes);

        public class MeetingRoomRules
        {
            public void Map(ITypeMappingConfigurator<IncorrectResource> meetingRoom)
            {
                meetingRoom.HandledBy<IncorrectHandler>();
            }
        }
    }
}

