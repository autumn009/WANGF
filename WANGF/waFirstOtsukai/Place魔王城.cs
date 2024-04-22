using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANGFLib;

namespace FirstOtsukai
{
    class Place魔王城:Place
    {
        public override string Id { get { return FirstOtsykaiConstants.魔王城ID; } }
        public override string HumanReadableName { get { return "魔王城"; } }
        public override Position GetDistance() { return new Position() { x = 100, y = 0 }; }
        public override string World { get { return HaoConstants.あの世界ID; } }
    }
}
