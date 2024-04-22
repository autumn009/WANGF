using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANGFLib;

namespace FirstOtsukai
{
    public class HaoPlaceこうばん : HaoPlace
    {
        public override string Id
        {
            get { return FirstOtsykaiConstants.こうばんID; }
        }
        public override string HumanReadableName
        {
            get { return "こうばん"; }
        }
        public override Position GetDistance()
        {
            return new Position() { x = 0, y = 100 };
        }
    }
}
