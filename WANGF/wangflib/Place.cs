using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;


namespace ANGFLib
{
    /// <summary>
    /// �s���\���̂���S�Ă̏ꏊ�̃R���N�V����
    /// </summary>
    public class Places
    {
        /// <summary>
        /// �֗��ŕ�����₷���V���[�g�J�b�g�Ƃ��āA�ǂ��ł��Ȃ��ꏊ�������܂��B
        /// </summary>
		public static readonly ANGFLib.Place PlaceNull = new PlaceNull();

        /// <summary>
        /// �A�Q���f�t�H���g�ɍs���ꏊ��Ԃ��܂��B
        /// �Ăяo���ꂽ�^�C�~���O�œ��I�Ɍ��肷�邱�Ƃ��ł��܂��B
        /// �܂薈�ӈႤ�ꏊ�ɍs�����Ƃ��ł��܂��B
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
    /// �ȒP�Ȉꎞ�I�ȏꏊ�Ƃ��Ďg�p����ꏊ���`���܂�
    /// </summary>
    public class MiscPlace : ANGFLib.Place
    {
        private string name;
        private int id;
        /// <summary>
        /// �l�Ԃ��ǂ̖��O�ł��B
        /// </summary>
        public override string HumanReadableName
        {
            get { return name; }
        }
        /// <summary>
        /// ��ӂ̎��ʖ��ł��B
        /// </summary>
        public override string Id
        {
            get { return id.ToString(); }
        }
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="id">��ӂ̎��ʖ��ł�</param>
        /// <param name="name">�l�Ԃ��ǂ̖��O�ł�</param>
        public MiscPlace(int id, string name)
        {
            this.name = name;
            this.id = id;
        }
    }

    /// <summary>
    /// �ȒP�Ȉꎞ�I�ȏꏊ�Ƃ��Ďg�p����ꏊ���`���܂�
    /// </summary>
    public class RoadPlace : ANGFLib.Place
    {
        private string name;
        private string next;
        private string prev;
        private string id;
        /// <summary>
        /// �l�Ԃ��ǂ̖��O�ł��B
        /// </summary>
        public override string HumanReadableName => name;
        /// <summary>
        /// ��ӂ̎��ʖ��ł��B
        /// </summary>
        public override string Id => id.ToString();
        /// <summary>
        /// �����N���ꂽ�ꏊ���ł�
        /// </summary>
        /// <returns>�ꏊID�̔z��</returns>
        public override string[] GetLinkedPlaceIds() => new string[] { next, prev };
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="id">��ӂ̎��ʖ��ł�</param>
        /// <param name="name">�l�Ԃ��ǂ̖��O�ł�</param>
        /// <param name="nextPlaceId">���̏ꏊ��ID</param>
        /// <param name="prevPlaceId">��O�̏ꏊ��ID</param>
        public RoadPlace(string name, string id, string nextPlaceId, string prevPlaceId)
        {
            this.name = name;
            this.id = id;
            this.prev = prevPlaceId;
            this.next = nextPlaceId;
        }
    }
}
