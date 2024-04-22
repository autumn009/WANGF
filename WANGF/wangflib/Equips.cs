using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// 装備の未来シミュレーションを行います。
    /// </summary>
    /// <param name="set">未来の火葬場の装備セットです。</param>
    /// <param name="msg">パネルに出るメッセージです。</param>
    /// <returns>不可ならfalseを返します。</returns>
	public delegate bool FutureEquipSimulationInvoker(EquipSet set, out string msg);

    /// <summary>
    /// ファッションの未来シミュレーションです。
    /// </summary>
	public static class FutureEquipSimulations
	{
        /// <summary>
        /// ファッションの未来シミュレーションをコレクションします。
        /// </summary>
		public static List<FutureEquipSimulationInvoker> list = new List<FutureEquipSimulationInvoker>();

        /// <summary>
        /// ファッションの未来シミュレーションを追加します。
        /// 一般のモジュールは呼び出すべきではありません。
        /// </summary>
        /// <param name="item">追加すべき未来ファッションシミュレーションです。</param>
		public static void AddItem(FutureEquipSimulationInvoker item)
		{
			list.Add(item);
		}
	}

    /// <summary>
    /// その性別で装備できないアイテムと装備部位を区別する
    /// ModuleEx経由で提供する
    /// </summary>
    public interface IEquipChecker
    {
        public bool IsEquippableItem(int equipOrder, string personId, string ItemId);
    }
}
