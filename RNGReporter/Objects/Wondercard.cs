using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNGReporter.Objects
{

    public class Wondercard
    {
        public int eventSpecies { get; set; }
        public uint eventTid { get; set; }
        public uint eventSid { get; set; }
        public int eventAbility { get; set; }
        public int eventNature { get; set; }
        public int eventGender { get; set; }
        public int eventShininess { get; set; }

        public int[] eventIVInfo { get; set; }
        public int genderCase { get; set; }

    }

}
