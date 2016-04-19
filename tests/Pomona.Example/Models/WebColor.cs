#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Example.Models
{
    public class WebColor
    {
        public WebColor(string color)
        {
            color = color.Trim();
            // Parses #RRGGBB where each component is a hex value
            if (color.Length != 7 || color[0] != '#')
            {
                throw new InvalidOperationException(
                    "Unable to parse color, needs to be in format #RRGGBB (hex for each component)");
            }

            for (var i = 0; i < 3; i++)
            {
                var colorComponentValue = Convert.ToInt32(color.Substring(1 + (i * 2), 2), 16);
                if (i == 0)
                    Red = colorComponentValue;
                if (i == 1)
                    Green = colorComponentValue;
                if (i == 2)
                    Blue = colorComponentValue;
            }
        }


        public int Blue { get; set; }
        public int Green { get; set; }
        public int Red { get; set; }


        public override string ToString()
        {
            return "The color is " + ToStringConverted();
        }


        public string ToStringConverted()
        {
            return $"#{Red:x2}{Green:x2}{Blue:x2}";
        }
    }
}