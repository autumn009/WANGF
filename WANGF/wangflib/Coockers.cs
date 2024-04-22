using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// 地図上の長さの表記や売買の価格の表記をカスタマイズします。
    /// 呼び出すことは自由です。
    /// 設定はフレームワークに任せるべきであり、自前で書き換えを行うべきではありません。
    /// </summary>
    public static class Coockers
    {
        /// <summary>
        /// 長さの表記をカスタマイズします。呼び出すことは自由ですが、設定はシステムに任せるべきです。
        /// </summary>
        public static MapLengthCoockerInvoker MapLengthCoocker = (l) => l.ToString("0" + "m");
        /// <summary>
        /// 通貨の表記をカスタマイズします。呼び出すことは自由ですが、設定はシステムに任せるべきです。
        /// </summary>
        public static PriceCoockerInvoker PriceCoocker = (p) =>
        {
            // Blazor WeBassemblyの制約:
            // 以下の1行がないといつの間にかカルチャの設定を忘れる
            Util.InitCulture();
            return p.ToString("C");
        };
    }
}
