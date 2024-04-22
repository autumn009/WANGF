using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;


namespace ANGFLib
{
    /// <summary>
    /// 行く可能性のある全ての場所のコレクション
    /// </summary>
    public class Places
    {
        /// <summary>
        /// 便利で分かりやすいショートカットとして、どこでもない場所を示します。
        /// </summary>
		public static readonly ANGFLib.Place PlaceNull = new PlaceNull();

        /// <summary>
        /// 就寝時デフォルトに行く場所を返します。
        /// 呼び出されたタイミングで動的に決定することもできます。
        /// つまり毎晩違う場所に行くこともできます。
        /// </summary>
		public static async System.Threading.Tasks.Task<string> GetDefaultPlaceIDAsync()
        {
            foreach (var n in State.loadedModules.Reverse())
            {
                string p0 = await n.GetDefaultPlaceAsync();
                if (p0 != null)
                {
                    return p0;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 簡単な一時的な場所として使用する場所を定義します
    /// </summary>
    public class MiscPlace : ANGFLib.Place
    {
        private string name;
        private int id;
        /// <summary>
        /// 人間が可読の名前です。
        /// </summary>
        public override string HumanReadableName
        {
            get { return name; }
        }
        /// <summary>
        /// 一意の識別名です。
        /// </summary>
        public override string Id
        {
            get { return id.ToString(); }
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">一意の識別名です</param>
        /// <param name="name">人間が可読の名前です</param>
        public MiscPlace(int id, string name)
        {
            this.name = name;
            this.id = id;
        }
    }

    /// <summary>
    /// 簡単な一時的な場所として使用する場所を定義します
    /// </summary>
    public class RoadPlace : ANGFLib.Place
    {
        private string name;
        private string next;
        private string prev;
        private string id;
        /// <summary>
        /// 人間が可読の名前です。
        /// </summary>
        public override string HumanReadableName => name;
        /// <summary>
        /// 一意の識別名です。
        /// </summary>
        public override string Id => id.ToString();
        /// <summary>
        /// リンクされた場所情報です
        /// </summary>
        /// <returns>場所IDの配列</returns>
        public override string[] GetLinkedPlaceIds() => new string[] { next, prev };
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">一意の識別名です</param>
        /// <param name="name">人間が可読の名前です</param>
        /// <param name="nextPlaceId">次の場所のID</param>
        /// <param name="prevPlaceId">手前の場所のID</param>
        public RoadPlace(string name, string id, string nextPlaceId, string prevPlaceId)
        {
            this.name = name;
            this.id = id;
            this.prev = prevPlaceId;
            this.next = nextPlaceId;
        }
    }
}
