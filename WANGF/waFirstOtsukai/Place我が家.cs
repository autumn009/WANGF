using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;

namespace FirstOtsukai
{
    public class HaoPlace我が家 : HaoPlace
    {
        public override string Id
        {
            get { return FirstOtsykaiConstants.我が家ID; }
        }
        public override string HumanReadableName
        {
            get { return "我が家"; }
        }
        public override Position GetDistance()
        {
            return new Position() { x = 0, y = 0 };
        }
        public override bool ConstructMenu(List<SimpleMenuItem> list)
        {
            if (ANGFLib.SystemFile.IsDebugMode)
            {
                list.Add(new SimpleMenuItem("例外発生", async () =>
                {
                    await Task.Delay(0);
                    throw new Exception("Debug Exception");
                }));
            }
            return base.ConstructMenu(list);
        }
    }
}
