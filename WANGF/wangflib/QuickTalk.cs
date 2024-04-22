using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ANGFLib
{
	/// <summary>
	/// 簡易な発言を行うラッパーを提供する
	/// </summary>
	public class QuickTalk
	{
		private Dictionary<string, Person> talkers = new Dictionary<string, Person>();
		private Dictionary<string, string> macros = new Dictionary<string, string>();


		/// <summary>
		/// メッセージ群を台詞として表示する
		/// 引数の文字列は1行単位で分解される
		/// 空行は無視される
		/// 半角空白、全角空白、タブまでは話者と見なされる
		/// 話者はAddTakerメソッドで登録する
		/// 本文の$に続く1文字はマクロとして置換される
		/// マクロはAddMacroメソッドで登録される
		/// 半角空白、全角空白、タブが含まれない行は、独白が話者と見なされる。このとき、本文に半角空白、全角空白、タブが含まれと判定が誤動作する
		/// </summary>
		/// <param name="text">複数行の文字列</param>
		public void Play( string text )
		{
			char[] whiteSpaces = { ' ', '\t', '　' };

			StringReader reader = new StringReader(text);
			for (; ; )
			{
				string s = reader.ReadLine();
				if (s == null) break;

				s = s.Trim();
				// 空行は無視
				if (s.Length == 0) continue;

                Person talker = null;
                string body;
                // 最初の空白文字までがTalker名、あとが本文
                int firstWhiteSpacePos = s.IndexOfAny(whiteSpaces);
                // 空白が2文字目より後にある場合は空白と承認しない (長いshortNameは認めない)
                // これを行わないと本文中に半角空白を使えない
                if (firstWhiteSpacePos < 0 || firstWhiteSpacePos > 2)
                {
                    talker = DefaultPersons.独白;
                    body = s;
                }
                else
                {
                    string shortName = s.Substring(0, firstWhiteSpacePos);
                    if (talkers.ContainsKey(shortName))
                    {
                        talker = talkers[shortName];
                    }
                    else
                    {
                        foreach (var n in State.loadedModules)
                        {
                            string id = n.GetQuickTalkPerson(shortName);
                            if (id != null)
                            {
                                talker = SimpleName<Person>.List[id];
                                break;
                            }
                        }
                    }
                    if (talker == null)
                    {
                        throw new ApplicationException(string.Format("QuickTalk構文エラー。{0}は定義されていない短いTalker名です。", shortName));
                    }
                    body = s.Substring(firstWhiteSpacePos + 1).Trim();
                }

				StringBuilder sb = new StringBuilder();
				bool macroFlag = false;
				foreach (char ch in body)
				{
					if (ch == '$')
					{
						macroFlag = true;
					}
					else if (macroFlag)
					{
                        string result = null;
                        foreach (var n in State.loadedModules)
                        {
                            result = n.GetQuickTalkMacro(new string(ch, 1));
                            if (result != null) break;
                        }
                        if (result == null)
                        {
                            if (!macros.ContainsKey(ch.ToString())) throw new ApplicationException(string.Format("QuickTalk構文エラー。{0}は定義されていないマクロ名です。", ch));
                            sb.Append(macros[ch.ToString()]);
                        }
                        else
                        {
                            sb.Append(result);
                        }
						macroFlag = false;
					}
					else
					{
						sb.Append(ch);
					}
				}

				talker.Say(sb.ToString());
			}
		}
		/// <summary>
		/// 話者を登録する
		/// wは主人公。dは独白が定義済みである
		/// </summary>
		/// <param name="shortName">便宜上の短い名前</param>
		/// <param name="talker">真の話者を示すPersonオブジェクト</param>
		public void AddTalker(string shortName, Person talker)
		{
			talkers[shortName] = talker;
		}

		/// <summary>
		/// マクロを登録する
		/// </summary>
		/// <param name="macroName">マクロの名前となる1文字</param>
		/// <param name="val">マクロの値</param>
		public void AddMacro(string macroName, string val)
		{
			macros[macroName] = val;
		}

		/// <summary>
		/// 上書き可能な話者として、w=主人公とd=独白を定義する
		/// </summary>
		public QuickTalk()
		{
			AddTalker("w", DefaultPersons.主人公);
			AddTalker("d", DefaultPersons.独白);
		}
	}
}
