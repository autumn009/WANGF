using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// アイテムが標準メニューで売却されたことを通知する。ModuleEx用クラス
    /// </summary>
    public abstract class SoldNotify
    {
        public abstract void NotifyItemWasSold(string itemId, int usedCount);
    }
}
