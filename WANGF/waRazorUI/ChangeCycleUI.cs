using ANGFLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace waRazorUI
{
    class ChangeCycleUI
    {
		public static async Task<bool> ChangeCycleAsync()
		{
			int 生活サイクル起点時間 = Flags.生活サイクル起点時間;

			// 現在の"起床時刻"ではなく、実際に今日起きた時刻を起点にしなければならない
			int 旧起床時刻 = State.今日の起床時刻.Hour;
			DefaultPersons.システム.Say("今日の起床時刻: " + 旧起床時刻.ToString() + "時00分");
			const string format = "{0}{1}時間 {2}時起床 {3}時就寝";
			var list = new List<SimpleMenuItem>();
			for (int i = 0; i < 5; i++)
			{
				var i0 = i; // Capture on
				list.Add(new SimpleMenuItem(string.Format(format, "+", i, (旧起床時刻 + i) % 24, (旧起床時刻 + i + 16) % 24), () =>
				{
                    JournalingWriter.IsTemporaryStopped = false;
                    return moveCycle(i0);
				}));
			}
			for (int i = 0; i < 4; i++)
			{
				var i0 = i; // Capture on
							// 以下の + 24はマイナス値を回避するためのゲタとしてはかせている
							// マイナス値の%はマイナスの結果を出して意図した値にならない
				list.Add(new SimpleMenuItem(string.Format(format,
					"-", i + 1, (旧起床時刻 - i - 1 + 24) % 24, (旧起床時刻 - i - 1 + 16) % 24), () =>
					{
						JournalingWriter.IsTemporaryStopped = false;
						return moveCycle(-i - 1);
					}));
			}
            JournalingWriter.IsTemporaryStopped = true;
            try
            {
                await UI.SimpleMenuWithCancelAsync("起床時刻変更", list.ToArray());
            }
            finally
            {
				// 既にクリア済みかも知れないが、確実にクリアしておく。
				// キャンセルされたときはここでクリアする。
                JournalingWriter.IsTemporaryStopped = false;
            }
			return false;   // dummy value
		}

		private static bool moveCycle(int i)
		{
			int 旧起床時刻 = State.今日の起床時刻.Hour;
			DateTime 新今日の就寝時刻 = State.今日の起床時刻.AddHours(16).AddHours(i);
			if (新今日の就寝時刻 <= Flags.Now)
			{
				DefaultPersons.システム.Say("就寝時刻を現在時刻よりも手前に変えることはできません。");
				return false;	// dummy value
			}
			// 以下の + 24はマイナス値を回避するためのゲタとしてはかせている
			// マイナス値の%はマイナスの結果を出して意図した値にならない
			int 新生活サイクル起点時間 = (Flags.生活サイクル起点時間 + i) % 24;
			Flags.生活サイクル起点時間 = 新生活サイクル起点時間;
			State.今日の就寝時刻 = 新今日の就寝時刻;
			JournalingWriter.Write("L", i);
			return false;   // dummy value
		}
	}
}
