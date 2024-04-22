using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANGFLib;

namespace FirstOtsukai
{
    // hao専用の場所だが、プロローグ等は使用していない (使用する必要もない)
    public abstract class HaoPlace: Place
    {
        public override string FatalLeaveConfim(Place dst)
        {
            Item Item1 = Items.GetItemByNumber(Flags.Equip[0]);
            Item Item2 = Items.GetItemByNumber(Flags.Equip[1]);
            if (Item1.IsItemNull && Item2.IsItemNull) return "何か着ないと移動できない!";
            if (Item1.IsItemNull ) return "何か上に着ないと移動できない!";
            if (Item2.IsItemNull) return "何か下に着ないと移動できない!";
            return null;
        }
    }
}
