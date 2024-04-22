using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANGFLib
{
    /// <summary>
    /// 移動に関する情報を受け渡すために使用されます。
    /// </summary>
    public class MoveInfo
    {
        /// <summary>
        /// 移動できる場合はtrueとします。
        /// </summary>
        public bool IsAvailable;
        /// <summary>
        /// 移動に要する時間を格納します。時間が存在しない場合はMinValueを格納します。
        /// </summary>
        public TimeSpan TimeToWillGo;
        /// <summary>
        /// 移動できる場合は距離を格納します。単位は仮想世界依存です。
        /// </summary>
        public int length;
        /// <summary>
        /// 補助的な説明文を補います。
        /// </summary>
        public string SupplyDescription;
    }

    /// <summary>
    /// 移動方法を示すオブジェクトを取得します。
    /// </summary>
    /// <param name="a">移動元</param>
    /// <param name="b">移動先</param>
    /// <returns>結果の情報を格納します。</returns>
    public delegate MoveInfo HowToMoveInvoker(Place a, Place b);
    /// <summary>
    /// 移動に関する情報を保持します。
    /// </summary>
    public static class Moves
    {
        private static double pow2(double a)
        {
            return a * a;
        }

        /// <summary>
        /// 2つに場所間の距離を計算します。
        /// </summary>
        /// <param name="a">場所その1</param>
        /// <param name="b">場所その2</param>
        /// <returns>計算結果</returns>
        public static int CalcLength(Place a, Place b)
        {
            // 少なくとも片方が不明の場合は最大値を仮に返しておく
            if (a == null || b == null) return int.MaxValue;
            if (a.GetParentDistance() == null || b.GetParentDistance() == null) return int.MaxValue;
            return (int)Math.Sqrt(pow2(a.GetParentDistance().x - b.GetParentDistance().x) + pow2(a.GetParentDistance().y - b.GetParentDistance().y));
        }
        /// <summary>
        /// 移動方法を処理するメソッドを指定します。設定はフレームワークに任せるべきです。
        /// </summary>
        public static HowToMoveInvoker HowToMove = (a, b) =>
        {
            // 時速4kmを仮定 (計算結果(メートル)/4(km/h)/1000m*60分とする)
            return new MoveInfo() { IsAvailable = true, TimeToWillGo = new TimeSpan(0, CalcLength(a, b) * 60 / 4 / 1000, 0), length = CalcLength(a, b), SupplyDescription = " ("+CalcLength(a, b).ToString()+")" };
        };
        /// <summary>
        /// サブ移動方法を処理するメソッドを指定します。設定はフレームワークに任せるべきです。
        /// </summary>
        public static HowToMoveInvoker HowToSubMove = (a, b) =>
        {
            return new MoveInfo() { IsAvailable = true, TimeToWillGo = TimeSpan.MinValue, length=0, SupplyDescription = "" };
        };
    }
}
