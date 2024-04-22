using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// 既読管理
    /// </summary>
	public class MessageSkipper
	{
		private static Dictionary<int, bool> flags = new Dictionary<int, bool>();
        private static bool loaded = false;
        private static bool dirty = false;

        private static bool isMessage既読(int hash) => flags.ContainsKey(hash);

        public static void Clear()
        {
            flags.Clear();
            dirty = true;
        }

        /// <summary>
        /// メッセージを既読に設定する
        /// </summary>
        /// <param name="message"></param>
        public static void SetMessage既読(string message)
        {
            int hash = Util.MyCalcHash(message);
            if (isMessage既読(hash)) return;
            flags[hash] = true;
            dirty = true;
        }

        /// <summary>
        /// メッセージが既読か判定する
        /// </summary>
        public static bool IsMessage既読(string message)
		{
            //if( loaded == false ) await load();
			return isMessage既読(Util.MyCalcHash(message));
		}

        /// <summary>
        /// Skipperオブジェクトを保存する
        /// </summary>
        public static async Task SaveAsync()
        {
            if (loaded && dirty)
            {
                using (var stream = new MemoryStream())
                {
                    using (var b = new BinaryWriter(stream))
                    {
                        foreach (int item in flags.Keys)
                        {
                            b.Write(item);
                        }
                    }
                    await UI.Actions.SaveFileAsync("skip", "skip.bin", stream.ToArray());
                    dirty = false;
                }
            }
        }

        /// <summary>
        /// Skipperオブジェクトを再読込する
        /// </summary>
		public static async Task ReloadAsync()
		{
			await loadAsync();
		}

        //static MessageSkipper()
        //{
        //await load();
        //}

        private static async Task loadAsync()
		{
            try
            {
                flags.Clear();
                var bytes = await UI.Actions.LoadFileAsync("skip", "skip.bin");
                if (bytes != null)
                {
                    using (var stream = new MemoryStream(bytes))
                    {
                        using (var r = new BinaryReader(stream))
                        {
                            for (int i = 0; i < bytes.Length; i += 4)
                            {
                                flags[r.ReadInt32()] = true;
                            }
                        }

                    }
                }
                loaded = true;
            }
            catch (FileNotFoundException)
            {
                // 無ければ無い状態から始まる
                loaded = true;
                return;
            }
            catch (System.IO.IOException)
            {
                // System.IO.IOException: The process cannot access the file 'C:\Documents and Settings\Lchen\Application Data\Pie Dey\ANGF\skip.bin' because it is being used by another process.
                // などの問題が発生した場合は読み込みを遅延させる
                return;
            }
		}

        /// <summary>
        /// ロードしていない状態に状態を戻す
        /// </summary>
        public static void SetAsNotLoaded()
        {
            loaded = false;
        }
    }
}
