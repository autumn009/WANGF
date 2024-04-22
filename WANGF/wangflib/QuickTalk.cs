using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ANGFLib
{
	/// <summary>
	/// �ȈՂȔ������s�����b�p�[��񋟂���
	/// </summary>
	public class QuickTalk
	{
		private Dictionary<string, Person> talkers = new Dictionary<string, Person>();
		private Dictionary<string, string> macros = new Dictionary<string, string>();


		/// <summary>
		/// ���b�Z�[�W�Q��䎌�Ƃ��ĕ\������
		/// �����̕������1�s�P�ʂŕ��������
		/// ��s�͖��������
		/// ���p�󔒁A�S�p�󔒁A�^�u�܂ł͘b�҂ƌ��Ȃ����
		/// �b�҂�AddTaker���\�b�h�œo�^����
		/// �{����$�ɑ���1�����̓}�N���Ƃ��Ēu�������
		/// �}�N����AddMacro���\�b�h�œo�^�����
		/// ���p�󔒁A�S�p�󔒁A�^�u���܂܂�Ȃ��s�́A�Ɣ����b�҂ƌ��Ȃ����B���̂Ƃ��A�{���ɔ��p�󔒁A�S�p�󔒁A�^�u���܂܂�Ɣ��肪�듮�삷��
		/// </summary>
		/// <param name="text">�����s�̕�����</param>
		public void Play( string text )
		{
			char[] whiteSpaces = { ' ', '\t', '�@' };

			StringReader reader = new StringReader(text);
			for (; ; )
			{
				string s = reader.ReadLine();
				if (s == null) break;

				s = s.Trim();
				// ��s�͖���
				if (s.Length == 0) continue;

                Person talker = null;
                string body;
                // �ŏ��̋󔒕����܂ł�Talker���A���Ƃ��{��
                int firstWhiteSpacePos = s.IndexOfAny(whiteSpaces);
                // �󔒂�2�����ڂ���ɂ���ꍇ�͋󔒂Ə��F���Ȃ� (����shortName�͔F�߂Ȃ�)
                // ������s��Ȃ��Ɩ{�����ɔ��p�󔒂��g���Ȃ�
                if (firstWhiteSpacePos < 0 || firstWhiteSpacePos > 2)
                {
                    talker = DefaultPersons.�Ɣ�;
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
                        throw new ApplicationException(string.Format("QuickTalk�\���G���[�B{0}�͒�`����Ă��Ȃ��Z��Talker���ł��B", shortName));
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
                            if (!macros.ContainsKey(ch.ToString())) throw new ApplicationException(string.Format("QuickTalk�\���G���[�B{0}�͒�`����Ă��Ȃ��}�N�����ł��B", ch));
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
		/// �b�҂�o�^����
		/// w�͎�l���Bd�͓Ɣ�����`�ς݂ł���
		/// </summary>
		/// <param name="shortName">�֋X��̒Z�����O</param>
		/// <param name="talker">�^�̘b�҂�����Person�I�u�W�F�N�g</param>
		public void AddTalker(string shortName, Person talker)
		{
			talkers[shortName] = talker;
		}

		/// <summary>
		/// �}�N����o�^����
		/// </summary>
		/// <param name="macroName">�}�N���̖��O�ƂȂ�1����</param>
		/// <param name="val">�}�N���̒l</param>
		public void AddMacro(string macroName, string val)
		{
			macros[macroName] = val;
		}

		/// <summary>
		/// �㏑���\�Șb�҂Ƃ��āAw=��l����d=�Ɣ����`����
		/// </summary>
		public QuickTalk()
		{
			AddTalker("w", DefaultPersons.��l��);
			AddTalker("d", DefaultPersons.�Ɣ�);
		}
	}
}
