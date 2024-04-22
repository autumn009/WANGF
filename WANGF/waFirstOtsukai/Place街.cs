using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANGFLib;

namespace FirstOtsukai
{
    class Place街 : Place
    {
        public override string Id { get { return FirstOtsykaiConstants.街ID; } }
        public override string HumanReadableName { get { return "街"; } }
        public override Position GetDistance() { return new Position() { x = 0, y = 0 }; }
        public override string World { get { return HaoConstants.あの世界ID; } }
    }
}
