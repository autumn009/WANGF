using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// Worldを表現する基底クラスです
    /// </summary>
    public abstract class World : SimpleName<World>
    {
        /// <summary>
        /// 説明文です。
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// 可視性を示します
        /// </summary>
        public virtual bool Visible
        {
            get { return true; }
        }
        /// <summary>
        /// 内部使用専用です。
        /// </summary>
        public static void ForceToInit()
        {
            // nop
        }
        static World()
        {
            SimpleName<World>.AddAndCheck("{2bc4dab6-0308-4bf8-ab16-20284bc04dbb}", new DefaultWorld());
        }
    }
    /// <summary>
    /// デフォルトのWorldです。WorldIdがnullの世界に該当します
    /// </summary>
    public class DefaultWorld : World
    {
        /// <summary>
        /// このIDのオブジェクトをオーバーライドして名前を変更できます。
        /// </summary>
        public override string Id { get { return Constants.DefaultWordId; } }
        /// <summary>
        /// デフォルトの名前です
        /// </summary>
        public override string HumanReadableName { get { return "基本世界"; } }
        /// <summary>
        /// デフォルトの説明です
        /// </summary>
        public override string Description { get { return "基本となる世界です。"; } }
    }
}
