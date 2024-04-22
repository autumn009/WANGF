using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ANGFLib
{
    /// <summary>
    /// ���ǊǗ�
    /// </summary>
	public class MessageSkipper
	{
		private static Dictionary<int, bool> flags = new Dictionary<int, bool>();
        private static bool loaded = false;
        private static bool dirty = false;

        private static bool isMessage����(int hash) => flags.ContainsKey(hash);

        public static void Clear()
        {
            flags.Clear();
            dirty = true;
        }

        /// <summary>
        /// ���b�Z�[�W�����ǂɐݒ肷��
        /// </summary>
        /// <param name="message"></param>
        public static void SetMessage����(string message)
        {
            int hash = Util.MyCalcHash(message);
            if (isMessage����(hash)) return;
            flags[hash] = true;
            dirty = true;
        }

        /// <summary>
        /// ���b�Z�[�W�����ǂ����肷��
        /// </summary>
        public static bool IsMessage����(string message)
		{
            //if( loaded == false ) await load();
			return isMessage����(Util.MyCalcHash(message));
		}

        /// <summary>
        /// Skipper�I�u�W�F�N�g��ۑ�����
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
        /// Skipper�I�u�W�F�N�g���ēǍ�����
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
                // ������Ζ�����Ԃ���n�܂�
                loaded = true;
                return;
            }
            catch (System.IO.IOException)
            {
                // System.IO.IOException: The process cannot access the file 'C:\Documents and Settings\Lchen\Application Data\Pie Dey\ANGF\skip.bin' because it is being used by another process.
                // �Ȃǂ̖�肪���������ꍇ�͓ǂݍ��݂�x��������
                return;
            }
		}

        /// <summary>
        /// ���[�h���Ă��Ȃ���Ԃɏ�Ԃ�߂�
        /// </summary>
        public static void SetAsNotLoaded()
        {
            loaded = false;
        }
    }
}
