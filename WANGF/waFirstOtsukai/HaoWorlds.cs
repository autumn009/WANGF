using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANGFLib;

namespace FirstOtsukai
{
    class AnotherWorld : World
    {
        public override string Description { get { return "魔王の世界です。"; } }
        public override string HumanReadableName { get { return "あの世界"; } }
        public override string Id { get { return HaoConstants.あの世界ID; } }
    }
    class ThisWorld : World
    {
        public override string Description { get { return "初めてのおつかいを行う現実世界です。"; } }
        public override string HumanReadableName { get { return "初めてのおつかい世界"; } }
        public override string Id { get { return Constants.DefaultWordId; } }
        public override bool ForceOverride { get { return true; } }
    }
}
