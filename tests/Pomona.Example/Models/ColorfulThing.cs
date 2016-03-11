#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Example.Models
{
    public class ColorfulThing : EntityBase
    {
        public ColorfulThing()
        {
            Color = new WebColor("#00ff00");
        }


        public WebColor Color { get; set; }
    }
}