using System;
using System.Collections.Generic;
using System.Text;


namespace ANGFLib
{
    /// <summary>
    /// どこでも無い場所を示す仮のパラメータです。
    /// このクラスを使用せずとも、Idがnullないし空文字列の場所はすべてどこでもない場所と見なされます。
    /// </summary>
    public class PlaceNull : ANGFLib.Place
    {
        /// <summary>
        /// 人間が読める場所の名前を返します。
        /// </summary>
        public override string HumanReadableName
        {
            get { return "どこでもない場所"; }
        }
        /// <summary>
        /// ここではステータスを見せないことを指定します。
        /// </summary>
        public override bool IsStatusHide
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// 識別用のIdを返します。
        /// </summary>
        public override string Id
        {
            get { return ""; }
        }
    }
}
