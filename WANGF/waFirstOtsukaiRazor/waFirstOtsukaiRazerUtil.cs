using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;

namespace waFirstOtsukaiRazor
{
    internal static class waFirstOtsukaiRazerUtil
    {
        internal static async Task ButtonOKAsync(string selection)
        {
            UI.Actions.CloseCustomRazorComponent(selection);
            await Task.Delay(0);
        }
    }
}
