using System;
using System.Collections.Generic;
using System.Text;


namespace ANGFLib
{
    /// <summary>
    /// �ǂ��ł������ꏊ���������̃p�����[�^�ł��B
    /// ���̃N���X���g�p�����Ƃ��AId��null�Ȃ����󕶎���̏ꏊ�͂��ׂĂǂ��ł��Ȃ��ꏊ�ƌ��Ȃ���܂��B
    /// </summary>
    public class PlaceNull : ANGFLib.Place
    {
        /// <summary>
        /// �l�Ԃ��ǂ߂�ꏊ�̖��O��Ԃ��܂��B
        /// </summary>
        public override string HumanReadableName
        {
            get { return "�ǂ��ł��Ȃ��ꏊ"; }
        }
        /// <summary>
        /// �����ł̓X�e�[�^�X�������Ȃ����Ƃ��w�肵�܂��B
        /// </summary>
        public override bool IsStatusHide
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// ���ʗp��Id��Ԃ��܂��B
        /// </summary>
        public override string Id
        {
            get { return ""; }
        }
    }
}
