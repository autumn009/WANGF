using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using ANGFLib;

using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Text.Encodings.Web;
using System.Runtime.InteropServices;

namespace ANGFLib
{
    /// <summary>
    /// �V���v���ȃ��j���[�̃A�N�V�����ł��B
    /// </summary>
    /// <returns>bool�l��Ԃ��܂����g�����͒�`����Ă��܂���B</returns>
    public delegate bool SimpleMenuAction();

    /// <summary>
    /// �W���I�ȃR�}���h���̕������񋟂��܂��B
    /// </summary>
    public class CommonCommandNames
    {
        /// <summary>
        /// �u�����v�̕������񋟂��܂��B
        /// </summary>
        public const string ���� = "����";
        /// <summary>
        /// �uSYSTEM�v�̕������񋟂��܂��B
        /// </summary>
        public const string SYSTEM = "SYSTEM";
    }

    /// <summary>
    /// ���j���[�̐F���ǂ����邩
    /// Default: �V�X�e���ɔC����@(�f�t�H���g)
    /// Normal: �ʏ�F�@(�������Ȃ�)
    /// Hot: ����
    /// </summary>
    public enum SimpleMenuType
    {
        Default = 0,
        Normal = 1,
        Hot = 2,
    }

    /// <summary>
    /// �V���v���ȃ��j���[�̍��ڂł��B
    /// </summary>
    [Serializable]
    public class SimpleMenuItem : MarshalByRefObject
    {
        /// <summary>
        /// �V���v���ȃ��j���[�̖��O�ł��B
        /// </summary>
        public string Name = null;
        /// <summary>
        /// ���j���[���I�����ꂽ�ꍇ�̃A�N�V�������w�肵�܂��Bnull�̏ꍇ�͉������܂���B
        /// </summary>
        public SimpleMenuAction SimpleMenuAction = null;
        /// ���j���[���I�����ꂽ�ꍇ�̃A�N�V�������w�肵�܂��Bnull�̏ꍇ�͉������܂���B
        /// </summary>
        public Func<Task<bool>> SimpleMenuActionAsync = null;/* DIABLE ASYNC WARN */
        /// <summary>
        /// �{�^���̉��ɏo��������B�I�v�V�����B������������ƃ��b�`�`���ɂȂ�B
        /// </summary>
        public string Explanation = null;
        /// <summary>
        /// �{�^���̃}�E�X�I�[�o�[�ŏo��������B�I�v�V�����B(�^�b�`���삾�Əo���Ȃ����ƒ���)
        /// </summary>
        public string MouseOverText = null;
        /// <summary>
        /// ���[�U�[��`�̃I�u�W�F�N�g���w�肵�܂��B�g�����͌��܂��Ă��܂���B
        /// </summary>
        public object UserParam = null;
        /// <summary>
        /// ���j���[�̃^�C�v��ݒ肵�܂��B
        /// </summary>
        public SimpleMenuType MenuType = SimpleMenuType.Default;
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="name">�l�ԉǂ̖��O�ł��B</param>
        /// <param name="simlpleMenuAction">�I�����Ɏ��s�����菇�ł��Bnull�Ȃ炠��܂���B</param>
        public SimpleMenuItem(string name, SimpleMenuAction simlpleMenuAction)
        {
            Name = name;
            SimpleMenuAction = simlpleMenuAction;
        }
        public SimpleMenuItem(string name, Func<Task<bool>> simlpleMenuActionAsync)/* DIABLE ASYNC WARN */
        {
            Name = name;
            SimpleMenuActionAsync = simlpleMenuActionAsync;/* DIABLE ASYNC WARN */
        }
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// ���O��userParam.ToString()�ƂȂ�܂��B
        /// </summary>
        /// <param name="userParam">���[�U�[��`�̔C�ӂ̃I�u�W�F�N�g�ł��B</param>
        public SimpleMenuItem(object userParam)
        {
            Name = userParam.ToString();
            UserParam = userParam;
        }
        /// <summary>
        /// �R���X�g���N�^�ł��B
        /// </summary>
        /// <param name="name">�l�ԉǂ̖��O�ł��B</param>
        /// <param name="simlpleMenuAction">�I�����Ɏ��s�����菇�ł��Bnull�Ȃ炠��܂���B</param>
        /// <param name="userParam">���[�U�[��`�̔C�ӂ̃I�u�W�F�N�g�ł��B</param>
        public SimpleMenuItem(string name, SimpleMenuAction simlpleMenuAction, object userParam)
        {
            Name = name;
            SimpleMenuAction = simlpleMenuAction;
            UserParam = userParam;
        }
    }

    /// <summary>
    /// �A�C�e���̉��i���擾����f���Q�[�g�^�ł��B
    /// </summary>
    /// <param name="item">�ΏۂƂ���A�C�e���ł��B</param>
    /// <param name="usedCount">�A�C�e���̎g�p�񐔂ł��B</param>
    /// <returns>���i�ł��B</returns>
    public delegate int GetPriceInvoker(Item item, int usedCount);

    /// <summary>
    /// �������������A�ԋp�l�̂Ȃ��f���Q�[�g�^�ł��B
    /// </summary>
    public delegate void VoidMethodInvoker();

    /// <summary>
    /// �������������A�ԋp�l�̂Ȃ��f���Q�[�g�^�����������Ɏ��f���Q�[�g�^�ł��B
    /// </summary>
    /// <param name="x">�������������A�ԋp�l�̂Ȃ��f���Q�[�g�^�ł��̒l�ł��B</param>
    public delegate void VoidMethodInvokerInvoker(VoidMethodInvoker x);

    /// <summary>
    /// UI�ōs���ׂ��A�N�V�����̃Z�b�g�ł�
    /// </summary>
    public class UIActionSet
    {
        /// <summary>
        /// ��ʏ�̑��������X�V���܂��B
        /// </summary>
        public Func<Dummy> ResetGameStatus = () => default;
        /// <summary>
        /// �e�L�X�g�̕\�����N���A���܂��B
        /// ��Ƀ��[�h/�Z�[�u��ɌĂяo���܂��B
        /// </summary>
        public Func<Dummy> ResetDisplay = () => default;
        /// <summary>
        /// 1�s�̃��b�Z�[�W���o�͂��܂��B
        /// </summary>
        public Func<Person, string, Dummy> messageOutputMethod = (Person talker, string message) => default;
        /// <summary>
        /// �V���v���ȃ��j���[��񋟂��܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<string, SimpleMenuItem[], SimpleMenuItem, Task<int>> simpleMenuMethodAsync = async (string prompt, SimpleMenuItem[] items, SimpleMenuItem systemMenu) => { await Task.Delay(0); return -1; };
        /// <summary>
        /// �Z�[�u����t�@�C�������[�U�[����擾���܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<string, Task<string>> saveFileNameAsync = async (string prompt) => { await Task.Delay(0); return string.Empty; };
        /// <summary>
        /// ���[�h����t�@�C���������[�U�[����擾���܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        /// <param name="prompt">�����ł�</param>
        /// <returns>�t�@�C���̖��O�ł��B���O�̈Ӗ��͎����ˑ��ł�</returns>
        public async Task<string> loadFileNameAsync(string prompt)
        {
            return await loadFileNameWithExtentionAsync
(prompt, General.FileExtention);
        }
        /// <summary>
        /// ���[�h����t�@�C���������[�U�[����擾���܂��B�g���q��C�ӂɎw��ł��܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<string, string, Task<string>> loadFileNameWithExtentionAsync = async (string prompt, string extention) => { await Task.Delay(0); return string.Empty; };
        /// <summary>
        /// �����Z�[�u�����t�H���_�ƃf�t�H���g�Ƃ��ă��[�h����t�@�C���������[�U�[����擾���܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<string, Task<string>> loadFileNameFromAutoSaveAsync = async (string prompt) => { await Task.Delay(0); return string.Empty; };
        /// <summary>
        /// 1�̕���������[�U�[�����͂��܂��B
        /// </summary>
        public Func<string, string, Task<string>> enterPlayerNameAsync = async (string prompt, string defaultValue) => { await Task.Delay(0); return string.Empty; };
        /// <summary>
        /// �A�C�e���g�p���j���[���J���Ď��s���܂��B
        /// </summary>
        public Func<Task<bool>> consumeItemMenuAsync = async () => { await Task.Delay(0); return false; };
        /// <summary>
        /// 1�̃A�C�e����I�����A���̃A�C�e����Id��Ԃ��܂��B
        /// </summary>
        public Func<Task<string>> selectOneItemAsync = async () => { await Task.Delay(0); return ""; };
        /// <summary>
        /// ������ʂ��J���܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<Task<bool>> equipMenuAsync = async () =>
        {
            var r = await UI.Actions.equipMenuExAsync(Flags.Equip.ToEquipSet(), DefaultPersons.��l��.Id, null);
            if (r == null) return false;
            Flags.Equip.FromEquipSet(r);
            return true;
        };
        /// <summary>
        /// ������ʂ��J���܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<EquipSet, string, Func<EquipSet, string>, Task<EquipSet>> equipMenuExAsync = async (equipSet, cutomValidator, personId) => { await Task.Delay(0); return null; };
        /// <summary>
        /// �̔���ʂ��J���܂��B
        /// </summary>
        public Func<GetPriceInvoker, Task<bool>> shopSellMenuAsync = async (GetPriceInvoker getPrice) => { await Task.Delay(0); return false; };
        /// <summary>
        /// ���p��ʂ��J���܂��B
        /// </summary>
        public Func<Item[], GetPriceInvoker, Task<bool>> shopBuyMenuAsync = async (Item[] sellingItems, GetPriceInvoker getPrice) => { await Task.Delay(0); return false; };
        /// <summary>
        /// �����̉�ʌ��ʂ𔭐������܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<Task<bool>> sleepFlashAsync = async () => { await Task.Delay(0); return false; };
        /// <summary>
        /// ��ʂ������Ȃ��ʌ��ʂ𔭐������܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<Task<bool>> WhiteFlashAsync = async () => { await Task.Delay(0); return false; };
        /// <summary>
        /// �W���[�i�����O�v���C�o�b�N���畜�A���܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<Task> restoreActionSetAsync = () => Task.CompletedTask;
        /// <summary>
        /// �W���[�i�����O�̃v���C�o�b�N���ɔ��������A�T�[�V������ʒm���܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<string, Task> tellAssertionFailedAsync = async (string message) => { await Task.Delay(0); };
        /// <summary>
        /// �i�������b�Z�[�W�œ`���܂��B��ʂ̃A�v������͌Ăяo���܂���B
        /// </summary>
        public Func<string, Task> progressStatusAsync = async (string message) => { await Task.Delay(0); };
        /// <summary>
        /// �W���[�i�����O�̃v���C�o�b�N���ł��邩���肵�܂��B
        /// </summary>
        public Func<bool> isJournalFilePlaying = delegate () { return false; };
        /// <summary>
        /// �����v�J�[�\���̐ݒ�Ɖ������s���܂��B
        /// </summary>
        public Func<bool, Task> �����v�Z�b�g���sAsync = async (bool on) => { await Task.Delay(0); };
        /// <summary>
        /// �G�N�X�|�[�g����t�@�C�������擾���܂��B
        /// </summary>
        public Func<string, string, string> ExportFileName���s = (dummy1, dummy2) => null;
        /// <summary>
        /// �C���|�[�g����t�@�C�������擾���܂��B
        /// </summary>
        public Func<string, string, string> ImportFileName���s = (dummy1, dummy2) => null;
        /// <summary>
        /// �g�[�^�����|�[�g��\�����܂��B
        /// </summary>
        public Func<Task> ShowTotalReportAsync = async () => { await Task.Delay(0); };
        /// <summary>
        /// �g�[�^�����|�[�g���_�E�����[�h���܂��B
        /// </summary>
        public Func<Task> DownloadTotalReportAsync = async () => { await Task.Delay(0); };
        /// <summary>
        /// �R���N�V�����̃��X�g���N�����܂��B
        /// </summary>
        public Func<string, bool, bool, Task<CollectionItem>> InvokeCollectionListAsync = async (id, a, b) => { await Task.Delay(0); return null; };
        /// <summary>
        /// �����T�C�N����ύX���܂�
        /// </summary>
        public Func<Task<bool>> ChangeCycleAsync = async () => { await Task.Delay(0); return false; };
        /// <summary>
        /// �C�ӂ̏������A�t�H�[���̃��C���X���b�h�Ŏ��s���܂��B
        /// </summary>
        public Func<Func<Task>, Task> CallAnyMethodAsync = async (x) => { await x(); };
        /// <summary>
        /// �C�ӂ̏������A�f�X�N�g�b�v�̏ꍇ�̂ݎ��s���܂��B
        /// </summary>
        public VoidMethodInvokerInvoker CallOnlyDesktop = (x) => { x(); };
        /// <summary>
        /// �C�ӂ̏������AWebPlayer�̏ꍇ�̂ݎ��s���܂��B
        /// </summary>
        public VoidMethodInvokerInvoker CallOnlyWeb = (x) => { /* no action here */ };
        /// <summary>
        /// ���C���̃t�H�[����Ԃ����\�b�h�ł��B
        /// </summary>
        public Func<object> GetMainForm = () => { return null; };
        /// <summary>
        /// �X�L�b�v���Ȃ�True��Ԃ����\�b�h�ł��B
        /// </summary>
        public Func<bool> IsSkipping = () => { return false; };
        /// <summary>
        /// �e��t�@�C����ǂݍ��݂܂�
        /// </summary>
        public Func<string, string, Task<byte[]>> LoadFileAsync = async (Category, Name) => { await Task.Delay(0); return null; };
        /// <summary>
        /// �e��t�@�C�����i���I�ɕۊǂ��܂�
        /// </summary>
        public Func<string, string, byte[], Task<string>> SaveFileAsync = async (Category, Name, Body) => { await Task.Delay(0); return null; };
        /// <summary>
        /// �����Z�[�u�����s����
        /// </summary>
        public Func<Task<bool>> AutoSaveFileAsync = async () => { await Task.Delay(0); return true; };
        /// <summary>
        /// �V���v���ȃ��[�h�I�����[�̃��X�gUI
        /// </summary>
        public Func<string, Tuple<string, string>[], Task> SimpleListAsync = async (title, items) => { await Task.Delay(0); };
        /// <summary>
        /// Desktop�Ȃ�True
        /// </summary>
        public Func<bool> IsDesktop = () => true;
        /// <summary>
        /// WebPlayer�̏ꍇ�̂݃X�v���b�V����\������
        /// </summary>
        public Func<string, Task> WebSplashAsync = async (htmlFragment) => { await Task.Delay(0); };
        /// <summary>
        /// WebPlayer�̏ꍇ�̂݃X�v���b�V����\������
        /// </summary>
        public Func<string, Dummy> SetPictureUrl = (url) => default;
        /// <summary>
        /// DesktopPlayer�̏ꍇ�̂݃X�v���b�V����\������
        /// �����̓r�b�g�}�b�v(JPEG)�ւ̃X�g���[���ł���
        /// </summary>
        public Func<Stream, Task> DesktopSplashAsync = async (bitmapStream) => { await Task.Delay(0); };
        /// <summary>
        /// HTML�t�H�[����\�����Č��ʂ𖼑O�ƒl�̃y�A�̃R���N�V�����Ƃ��Ď󂯎��
        /// HTML�t�H�[����form�v�f���܂�ł͂Ȃ�Ȃ�
        /// </summary>
        public Func<string, string, Task<Dictionary<string, string>>> HtmlFormAsync = async (title, htmlForm) =>
        {
            await Task.Delay(0);
            return new Dictionary<string, string>();
        };
        /// <summary>
        /// �A�b�v�O���[�h�`�F�b�N���s��
        /// </summary>
        public Func<string, string[], bool> UpgradeCheck = (msg, checkSams) => false;
        /// <summary>
        /// Web�A�N�Z�X�̃A�N�Z�X�g�[�N�����擾����
        /// </summary>
        public Func<string> GetAccessToken = () => null;
        /// <summary>
        /// ���A���^�C���ʒm���b�Z�[�W�̕\��
        /// </summary>
        public Func<string, Task> NotifyStatusMessageAsync = async (msg) => { await Task.Delay(0); };
        /// <summary>
        /// �����X�^�[���𓾂�
        /// </summary>
        public Func<int> GetStars = () => 0;
        /// <summary>
        /// �X�^�[�������
        /// </summary>
        public Func<int, Task> AddStarsAsync = async (delta) => { await Task.Delay(0); };
        /// <summary>
        /// ���t�̓���
        /// </summary>
        public Func<DateTime, DateTime, DateTime, Task<DateTime?>> DoEnterDateAsync = async (inital, min, max) => { await Task.Delay(0); return null; };
        /// <summary>
        /// ���t�����̓���
        /// </summary>
        public Func<DateTime, DateTime, DateTime, Task<DateTime?>> DoEnterDateTimeAsync = async (inital, min, max) => { await Task.Delay(0); return null; };
        /// <summary>
        /// ���t�����̓��̓I�v�V�����t��
        /// </summary>
        public Func<DateTime, DateTime, DateTime, string[], Task<Tuple<DateTime?,string>>> DoEnterDateTimeWithOptionsAsync = async (inital, min, max, options ) => { await Task.Delay(0); return null; };
        /// <summary>
        /// �����̓���
        /// </summary>
        public Func<string, int, int, int, Task<int?>> DoEnterNumberAsync = async (prompt, inital, min, max) => { await Task.Delay(0); return null; };
        /// <summary>
        /// �l�b�g���M
        /// </summary>
        public Func<string, string, Task<string>> NetSendAsync = async (id, base64) => { await Task.Delay(0); return null; };
        /// <summary>
        /// Uri�擾
        /// </summary>
        public Func<string> GetUri = () => "unknown";
        /// <summary>
        /// �J�X�^����Razor�R���|�[�l���g���J��
        /// ������Type�̓R���|�[�l���g�̌^�B
        /// �߂�l��object�͒�`�������B���R�Ɏg���ėǂ�
        /// </summary>
        public Func<Type, Task<object>> OpenCustomRazorComponentAsync = (type) => Task.FromResult<object>(null);
        /// <summary>
        /// �J�X�^����Razor�R���|�[�l���g�����
        /// ���̃A�N�V�����̓J�X�^���R���|�[�l���g���g���I�����ɌĂяo���ׂ�
        /// �߂�l�̃I�u�W�F�N�g��OpenCustomRazorComponent�̖߂�l�ƂȂ�
        /// </summary>
        public Action<object> CloseCustomRazorComponent = (obj) => { };
        public Action Reboot = () => { };
    }

    /// <summary>
    /// ���[�U�[�C���^�[�t�F�[�X���\����N���X�ł��B
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// ���݂̃A�N�V�����Z�b�g�ł��B
        /// </summary>
        static public UIActionSet Actions;

        // ���ݓǂݍ��܂�Ă���t�@�C���̃t���p�X
        // �ǂݍ��܂�Ă��Ȃ����null
        private static string currentFullPath;

        /// <summary>
        /// �ǂݏo����p��currentFullPath�̃��b�p
        /// </summary>
        public static string CurrentFullPath
        {
            get { return currentFullPath; }
        }

        /// <summary>
        /// ���b�Z�[�W�o�͂̃w���p
        /// </summary>
        /// <param name="talker"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        static public void M(Person talker, string format, params Object[] arg)
        {
            if (arg.Length == 0)
            {
                Actions.messageOutputMethod(talker, format);
            }
            else
            {
                Actions.messageOutputMethod(talker, string.Format(format, arg));
            }
        }

        private static async Task<string> loadSaveAsync(string name, string filename, Func<string, string, Task<string>> proc, bool isAuto = false)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                DefaultPersons.�V�X�e��.Say(name + "�͒��~����܂����B");
                return null; ;
            }
            string newFileName = null;
            string result = null;
            await UI.Actions.CallAnyMethodAsync(async () =>
            {
                await UI.Actions.�����v�Z�b�g���sAsync(true);
                try
                {
                    newFileName = await proc(isAuto ? "AUTO" : "SAVE", filename);
                    if (string.IsNullOrWhiteSpace(newFileName))
                    {
                        result = "���[�h/�Z�[�u�̏����͂ł��܂���ł����B";
                        newFileName = null;
                    }
                }
                catch (Exception e)
                {
                    result = e.ToString();
                }
                finally
                {
                    await UI.Actions.�����v�Z�b�g���sAsync(false);
                }
            });
            if (result != null)
            {
                UI.M(DefaultPersons.�V�X�e��, result);
                return null;
            }
            DefaultPersons.�V�X�e��.Say(name + "�͊������܂����B");
            return newFileName;
        }

        /// <summary>
        /// UI���݂Ń��[�h���s���܂��B
        /// </summary>
        /// <returns>���[�h�ɐ���������True��Ԃ��܂��B</returns>
        static public async Task<bool> LoadAsync()
        {
            string filename = await Actions.loadFileNameAsync("�f�[�^�̓ǂݍ��݂��s���܂��B");
            string result = await loadSaveAsync("�ǂݍ���", filename, State.LoadAsync);
            if (result == null) return false;
            currentFullPath = result; // �f�[�^��ǂݍ��񂾂���㏑�������Ă͂Ȃ�Ȃ�
            Actions.ResetGameStatus();
            Actions.ResetDisplay();
            DefaultPersons.�V�X�e��.Say("{0}��ǂݍ��݂܂����B", Path.GetFileNameWithoutExtension(result));
            return true;
        }

        /// <summary>
        /// UI���݂Ŏ����Z�[�u�t�H���_���f�t�H���g�Ƃ��ă��[�h���s���܂��B
        /// </summary>
        /// <returns>���[�h�ɐ���������True��Ԃ��܂��B</returns>
        static public async Task<bool> LoadFromAutoSaveAsync()
        {
            string filename = await Actions.loadFileNameFromAutoSaveAsync("�f�[�^�̓ǂݍ��݂��s���܂��B");
            string result = await loadSaveAsync("�ǂݍ���", filename, State.LoadAsync, true);
            if (result == null) return false;
            currentFullPath = null; // �����Z�[�u�̃f�[�^��ǂݍ��񂾂���㏑�������Ă͂Ȃ�Ȃ�
            Actions.ResetGameStatus();
            Actions.ResetDisplay();
            DefaultPersons.�V�X�e��.Say("{0}��ǂݍ��݂܂����B", Path.GetFileNameWithoutExtension(result));
            return true;
        }

        /// <summary>
        /// ���ɓǂݍ��܂�Ă���t�@�C�����㏑�����܂��B
        /// </summary>
        /// <returns>�Z�[�u�ɐ���������True��Ԃ��܂��B</returns>
        static public async Task<bool> SaveAsync()
        {
            if (currentFullPath == null) return await SaveAsAsync();
            // Skip.bin���t�@�C���ɏ���
            await ANGFLib.MessageSkipper.SaveAsync();
            string reault = await loadSaveAsync("�ۑ�", currentFullPath, State.SaveAsync);
            DefaultPersons.�V�X�e��.Say("{0}��ۑ����܂����B", Path.GetFileNameWithoutExtension(reault));
            return true;
        }

        /// <summary>
        /// �V�����t�@�C��������͂��ăt�@�C�����Z�[�u���܂�
        /// </summary>
        /// <returns>�Z�[�u�ɐ���������True��Ԃ��܂��B</returns>
        static async public Task<bool> SaveAsAsync()
        {
            string filename = await Actions.saveFileNameAsync("�f�[�^�̕ۑ����s���܂��B");
            // Skip.bin���t�@�C���ɏ���
            await ANGFLib.MessageSkipper.SaveAsync();
            string result = await loadSaveAsync("�ۑ�", filename, State.SaveAsync);
            if (result == null) return false;
            currentFullPath = result;
            DefaultPersons.�V�X�e��.Say("{0}��ۑ����܂����B", Path.GetFileNameWithoutExtension(result));
            return true;
        }

        static private bool done()
        {
            State.Terminate();
            return true;
        }

        static private async Task<bool> confirmCommonAsync(string label)
        {
            SimpleMenuItem[] items = {
                new SimpleMenuItem("�͂�", done ),
                new SimpleMenuItem("������", nop ),
            };
            int selection = await UI.SimpleMenuWithoutSystemAsync($"�{����{label}���܂���?", items);
            return selection == 0;
        }

        /// <summary>
        /// �I���̈ӎv�m�F���s���A�͂��ł����State.Terminate()���\�b�h���Ăяo���܂��B
        /// </summary>
        /// <returns>�u�͂��v�Ȃ�True��Ԃ��܂��B</returns>
        static public async Task<bool> DoneConfirmAsync() => await confirmCommonAsync("�I��");

        /// <summary>
        /// ���u�[�g�̈ӎv�m�F���s���A�͂��ł����State.Terminate()���\�b�h���Ăяo���܂��B
        /// </summary>
        /// <returns>�u�͂��v�Ȃ�True��Ԃ��܂��B</returns>
        static public async Task<bool> RebootConfirmAsync() => await confirmCommonAsync("���u�[�g");

        static private async Task<bool> itemAsync()
        {
            return await UI.Actions.consumeItemMenuAsync();
        }

        static private async Task<bool> equipAsync()
        {
            var targetList = Party.EnumPartyMembers().Select(c => Person.List[c]).OfType<IPartyMember>().Where(c=>c.GetDirectEquipEnabled()).OfType<Person>().ToArray();
            Person p = DefaultPersons.��l��;
            if ( targetList.Length > 1 )
            {
                int index = await UI.SimpleMenuWithCancelAsync("������ύX����������", targetList.Select(c => new SimpleMenuItem(c.HumanReadableName, null, null)).ToArray());
                p = targetList[index];
            }
            Func<EquipSet, string> customValidator = null;  // TBW must be implement
            Flags.Equip.TargetPersonId = p.Id;
            var eq = await UI.Actions.equipMenuExAsync(Flags.Equip.ToEquipSet(), p.Id, customValidator);
            if( eq == null ) return false;
            Flags.Equip.FromEquipSet(eq);
            return true;
        }

        static private bool nop()
        {
            // �������Ȃ�
            return true;
        }

        static private async Task<bool> goHomeAndSleepAsync()
        {
            if (!await UI.YesNoMenuAsync("�m�F", "�{����1�����I����", "����A�܂��I���Ȃ�")) return false;
            DefaultPersons.�V�X�e��.Say("1�����I���ɂ��܂��B");
            await State.GoNextDayMorningAsync();
            return true;
        }

        static private async Task<bool> restMenuAsync()
        {
            List<SimpleMenuItem> items = new List<SimpleMenuItem>();
            // ���ݎ�����薢�����A�Q�\�莞���܂ł̎��Ԃ����X�g�A�b�v����
            int from = Flags.Now.Hour;
            int to = State.�����̏A�Q����.Hour;
            if (General.GetDateOnly(Flags.Now) != General.GetDateOnly(State.�����̏A�Q����))
            {
                to += 24;
            }
            for (int h0 = from + 1; h0 <= to; h0++)
            {
                int h = h0; // �L���v�`���[�A�I��!
                string label = string.Format("�x�e{0}{1}���܂�", h >= 24 ? "����" : "", h % 24);
                items.Add(new SimpleMenuItem(label, () => General.RestUntilAsync(h % 24, h >= 24)));
            }
            await UI.SimpleMenuWithCancelAsync("[�x�e���j���[]", items.ToArray());
            return true;
        }

        static private async Task<bool> addEquipSetAsync(Person p)
        {
            DefaultPersons.�V�X�e��.Say($"{p.HumanReadableName}�����ݐg�ɕt���Ă��鑕���Z�b�g���ȒP�ɍČ��ł���悤�ɕۑ����܂��B");
            if (await UI.YesNoMenuAsync("�ۑ����܂���?", "�ۑ�����", "��߂�"))
            {
                var old = Flags.Equip.TargetPersonId;
                try
                {
                    Flags.Equip.TargetPersonId = p.Id;
                    var allItemNames = Enumerable.Range(0, SimpleName<EquipType>.List.Count).Select(c => Items.GetItemByNumber(Flags.Equip[c])).Where(c => !c.IsItemNull).Select(c => c.HumanReadableName);
                    string s = allItemNames.FirstOrDefault() ?? "�S��";
                    if (allItemNames.Count() > 1) s += " etc";
                    string name = s;
                    if (State.IsEquipSetName(name))
                    {
                        for (int i = 1; ; i++)
                        {
                            name = $"{s} ({i})";
                            if (!State.IsEquipSetName(name)) break;
                        }
                    }
                    State.SetEquipSet(name, General.�����i���̃R�s�[());
                    DefaultPersons.�V�X�e��.Say($"{name}�Ƃ��ĕۑ����܂����B����Ȍ�̓{�^��1�ł��̈ߑ����Č��ł��܂��B���O�͌ォ��ύX�ł��܂��B");
                }
                finally
                {
                    Flags.Equip.TargetPersonId = old;
                }
            }
            return true;
        }

        static private async Task<bool> renameRemoveCommonEquipSetAsync(Func<string,Task> doit)
        {
            await UI.SimpleMenuWithCancelAsync("�Ώۂ̑����Z�b�g��I�����ĉ������B", State.GetEquipSets().Where(c=>!State.DisabledEquipSetNames().Contains(c)).Select(c => new SimpleMenuItem(c, async () =>
            {
                await doit(c);
                return true;
            })).ToArray());
            return true;
        }

        static private async Task<bool> renameEquipSetAsync()
        {
            return await renameRemoveCommonEquipSetAsync(async (name) =>
            {
                string newName = await UI.Actions.enterPlayerNameAsync("�V�������O", name);
                if (string.IsNullOrWhiteSpace(newName))
                {
                    DefaultPersons.�V�X�e��.Say($"�󔒂̖��O�͎g�p�ł��܂���B");
                    return;
                }
                if (State.IsEquipSetName(newName))
                {
                    DefaultPersons.�V�X�e��.Say($"{newName}�͊��Ɏg�p�ς݂̖��O�ł��B");
                    return;
                }
                State.RenameEquipSetName(name, newName);
                DefaultPersons.�V�X�e��.Say($"{name}�̖��O��{newName}�ɕύX���܂����B");
            });
        }

        static private async Task<bool> removeEquipSetAsync()
        {
            return await renameRemoveCommonEquipSetAsync(async (name) => {
                if (await UI.YesNoMenuAsync("�{���ɍ폜���܂���?","�͂�","������"))
                {
                    State.RemoveEquipSet(name);
                    DefaultPersons.�V�X�e��.Say($"{name}���폜���܂����B");
                }
            });
        }

        static private async Task<bool> changeCycleAsync()
        {
            return await UI.Actions.ChangeCycleAsync();
        }
        static private bool SystemInfo()
        {
            DefaultPersons.�V�X�e��.Say("���݂̏�����: {0}", Flags.������);
            DefaultPersons.�V�X�e��.Say("���݂̏����X�^�[: {0}", StarManager.GetStars());
#if MYBLAZORAPP
            DefaultPersons.�V�X�e��.Say($"���݂̃v���b�g�t�H�[��: {State.PlatformName ?? "Unknown"}");
            var host = General.IsInAzure() ? "Azure/�l�b�g���M�\" : "Other/�l�b�g���M�s��";
            DefaultPersons.�V�X�e��.Say($"���݂̃z�X�g: { host }");
            var blazor = General.IsBlazorWebAssembly() ? "WebAssembly" : "Server";
            DefaultPersons.�V�X�e��.Say($"���݂�Blazor: {blazor}");
#if DEBUG
            DefaultPersons.�V�X�e��.Say($"���݂�RuntimeInformation.OSDescription: {RuntimeInformation.OSDescription}");
#endif
#else
            DefaultPersons.�V�X�e��.Say("���݂̃v���b�g�t�H�[��: {0}", State.IsInWebPlayer ? "Web" : "Desktop");
            var misc = Util.ReadRegistryForMisc();
            var f = misc.UseCloudStorage;
            if (State.IsInWebPlayer) f = true;
            DefaultPersons.�V�X�e��.Say("���݂̃X�g���[�W: {0}", f ? "�N���E�h" : "���[�J��");
#endif
            return true;
        }

        static private async Task<bool> systemMenuAsync()
        {
            List<SimpleMenuItem> items = new List<SimpleMenuItem>();
            if ((State.MenuStopMaps & MenuStopControls.Item) == 0)
            {
                items.Add(new SimpleMenuItem("������", itemAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.Equip) == 0)
            {
                items.Add(new SimpleMenuItem(CommonCommandNames.����, equipAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.Rest) == 0)
            {
                items.Add(new SimpleMenuItem("�x�e1����", General.Rest60Async));
                items.Add(new SimpleMenuItem("�x�e15��", General.Rest15Async));
                items.Add(new SimpleMenuItem("�x�e �w�莞�܂�", restMenuAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.GoHome) == 0)
            {
                items.Add(new SimpleMenuItem("1�����I����", goHomeAndSleepAsync));
            }
            var oneManParty = Party.EnumPartyMembers().Count() == 1;
            foreach (var item in Party.EnumPartyMembers())
            {
                var p = Person.List[item];
                var label = "�����Z�b�g�ǉ�";
                if (!oneManParty) label += $" ({p.HumanReadableName})";
                items.Add(new SimpleMenuItem(label, async ()=> { await addEquipSetAsync(p); return true; }));
            }
            items.Add(new SimpleMenuItem("�����Z�b�g���O�ύX", renameEquipSetAsync));
            items.Add(new SimpleMenuItem("�����Z�b�g�폜", removeEquipSetAsync));
            if ((State.MenuStopMaps & MenuStopControls.ChangeCycle) == 0)
            {
                items.Add(new SimpleMenuItem("�����T�C�N���ύX", changeCycleAsync));
            }

            foreach (var m in State.loadedModules) m.ConstructSystemMenu(items, State.CurrentPlace);

            if ((State.MenuStopMaps & MenuStopControls.Load) == 0)
            {
                items.Add(new SimpleMenuItem("���[�h", LoadAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.AutoLoad) == 0)
            {
                items.Add(new SimpleMenuItem("�����Z�[�u �t�H���_���烍�[�h", LoadFromAutoSaveAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.Save) == 0 && currentFullPath != null)
            {
                items.Add(new SimpleMenuItem(Path.GetFileNameWithoutExtension(currentFullPath) + "�֏㏑���Z�[�u", SaveAsync));
            }
            if ((State.MenuStopMaps & MenuStopControls.Save) == 0)
            {
                items.Add(new SimpleMenuItem("���O��t���ăZ�[�u", SaveAsAsync));
            }
            foreach (var item in SimpleName<Collection>.List.Values)
            {
                var id0 = item.Id;  // Capture on
                items.Add(new SimpleMenuItem(item.HumanReadableName+"�E�ꗗ", async () =>
                {
                    _ = await UI.Actions.InvokeCollectionListAsync(id0, false, false);
                    return false;
                }));
            }
            items.Add(new SimpleMenuItem("�V�X�e���ڍ׏��", SystemInfo));
            if (SystemFile.IsDebugMode)
            {
                items.Add(new SimpleMenuItem("�f�o�b�O�p���������Ԉړ�", async () =>
                {
                    if (!UI.Actions.isJournalFilePlaying())
                    {
                        var minDate = DateTime.MinValue;
                        var maxDate = new DateTime(9998, 12, 31);
                        var date = await UI.Actions.DoEnterDateAsync(Flags.Now, minDate, maxDate);
                        if (date == null) return false;
                        await General.TimeWarpAsync(date.Value);
                    }
                    return true;
                }));
                items.Add(new SimpleMenuItem("�e�X�g��p�V���b�v", async () =>
                {
                    var list = Item.List.Values.Where(c => !c.IsItemNull);
                    await UI.Actions.shopBuyMenuAsync(list.ToArray(), (a, b) => 100);
                    return true;
                }));
                items.Add(new SimpleMenuItem("�������S���~�A�b�v", () =>
                {
                    Flags.������ += 1000000;
                    DefaultPersons.�V�X�e��.Say("�������𑝂₵�܂����B");
                    return true;
                }));
                items.Add(new SimpleMenuItem("�����X�^�[�S�A�b�v", () =>
                {
                    StarManager.AddStar(100);
                    DefaultPersons.�V�X�e��.Say("�����X�^�[�𑝂₵�܂����B");
                    return true;
                }));
                items.Add(new SimpleMenuItem("�W���[�i�����O�Đ�", async () =>
                {
                    await General.StartJournalingPlaybackAsync();
                    return true;
                }
                ));
            }
            items.Add(new SimpleMenuItem("�Q�[���E���|�[�g�̃_�E�����[�h", async () =>
            {
                await UI.Actions.DownloadTotalReportAsync();
                return true;
            }));
            var extraSubMenus = ANGFLib.MenuItem.List.Values.Where(c => c.MenuType == MyMenuType.Info).ToArray();
            foreach (var subMenu in extraSubMenus)
            {
                items.Add(new SimpleMenuItem(subMenu.HumanReadableName, async () =>
                {
                    //if (subMenu.Method != null) subMenu.Method(UI.Actions.GetMainForm());
                    if (subMenu.MethodAsync != null) await subMenu.MethodAsync(UI.Actions.GetMainForm());
                    return true;
                }));
            }
            //if (extraSubMenus.Length > 0)
            //{
            //items.Add(new SimpleMenuItem("�ǉ����j���[��", async () => { await extraSubMenu(extraSubMenus); return true; }));
            //}
            //foreach (var item in ANGFLib.MenuItem.List.Values.Where(c => c.MenuType == MyMenuType.Top))
            //{
            //items.Add(new SimpleMenuItem(item.HumanReadableName, async () =>
            //{
            //if (item.Method != null) item.Method(null);
            //if (item.MethodAsync != null) await item.MethodAsync(null);
            //return true;
            //}));
            //}
#if false
            items.Add(new SimpleMenuItem("Person�������`�F�b�N", () =>
            {
                foreach (var item in Person.AllPersonIds)
                {
                    System.Diagnostics.Debug.Assert(Person.List.Keys.Contains(item));
                }
                return false;
            }));
#endif
            items.Add(new SimpleMenuItem("���u�[�g", async () =>
            {
                if (!await UI.YesNoMenuAsync("�Z�[�u���Ă��Ȃ��f�[�^�͑S�Ď����܂��B�Q�[�������u�[�g���܂���?", "�͂�", "������")) return true;
                if (!await UI.YesNoMenuAsync("�{���Ƀ��u�[�g����̂ł���?", "�͂�", "������")) return true;
                UI.Actions.Reboot();
                return true;
            })
            { MenuType = SimpleMenuType.Hot });
            if (!General.IsBlazorWebAssembly())
            {
                items.Add(new SimpleMenuItem("�I��", async () =>
                {
                    if (!await UI.YesNoMenuAsync("�Z�[�u���Ă��Ȃ��f�[�^�͑S�Ď����܂��B�Q�[�����I�����܂���?", "�͂�", "������")) return true;
                    if (!await UI.YesNoMenuAsync("�{���ɏI������̂ł���?", "�͂�", "������")) return true;
                    Environment.Exit(0);
                    return true;
                })
                { MenuType = SimpleMenuType.Hot });
            }

            await UI.simpleMenuWithCancelForSystemMenuAsync("[�V�X�e�� ���j���[]", items.ToArray());
            return true;
        }

        //private static async Task extraSubMenu(MenuItem[] extraSubMenus)
        //{
        //List<SimpleMenuItem> items = new List<SimpleMenuItem>();
        //    foreach (var item in extraSubMenus)
        //    {
        //items.Add(new SimpleMenuItem(item.HumanReadableName, async () => {
        //            if (item.Method != null) item.Method(null);
        //if (item.MethodAsync != null) await item.MethodAsync(null);
        //  return true; }));
        //}
        //    if (SystemFile.IsDebugMode) {
        //          items.Add(new SimpleMenuItem("���b�Z�[�W���ǃN���A", () => {
        //        MessageSkipper.Clear();
        //          return true;
        //    }));
        //}
        //await UI.SimpleMenuWithCancelForSystemMenu("[�ǉ� �V�X�e�� ���j���[]", items.ToArray());
        //  }

        private static SimpleMenuItem systemMenuItem = new SimpleMenuItem(CommonCommandNames.SYSTEM, systemMenuAsync);
        /// <summary>
        /// �V���v���ȃ��j���[�ł��B�V�X�e�����j���[���t���܂��B�I�΂ꂽ�ꍇ�A���ڂɎ��s���ׂ����e������Ύ��s���܂��B
        /// </summary>
        /// <param name="prompt">�v�����v�g������ł��B</param>
        /// <param name="items">���j���[�ꗗ�ł�</param>
        /// <returns>�I�����ꂽ�Y�����ł��B</returns>
        static public async Task<int> SimpleMenuAsync(string prompt, SimpleMenuItem[] items)
        {
            EnableEquipButtons(true);
            return await Actions.simpleMenuMethodAsync(prompt, items, systemMenuItem);
        }

        /// <summary>
        /// �V���v���ȃ��j���[�ł��B�L�����Z�����t���܂��B�V�X�e�����j���[�͕t���܂���B�I�΂ꂽ�ꍇ�A���ڂɎ��s���ׂ����e������Ύ��s���܂��B
        /// </summary>
        /// <param name="prompt">�v�����v�g������ł��B</param>
        /// <param name="items">���j���[�ꗗ�ł�</param>
        /// <returns>�I�����ꂽ�Y�����ł��B�L�����Z�����ꂽ�ꍇ��-1��Ԃ��܂��B</returns>
        static public async Task<int> SimpleMenuWithCancelAsync(string prompt, SimpleMenuItem[] items)
        {
            EnableEquipButtons(false);
            return await Actions.simpleMenuMethodAsync(prompt, items, new SimpleMenuItem("�L�����Z��"));
        }
        static private async Task<int> simpleMenuWithCancelForSystemMenuAsync(string prompt, SimpleMenuItem[] items)
        {
            EnableEquipButtons(true);
            return await Actions.simpleMenuMethodAsync(prompt, items, new SimpleMenuItem("�L�����Z��"));
        }
        /// <summary>
        /// �V���v���ȃ��j���[�ł��B�L�����Z����A�V�X�e�����j���[�͕t���܂���B�I�΂ꂽ�ꍇ�A���ڂɎ��s���ׂ����e������Ύ��s���܂��B
        /// </summary>
        /// <param name="prompt">�v�����v�g������ł��B</param>
        /// <param name="items">���j���[�ꗗ�ł�</param>
        /// <returns>�I�����ꂽ�Y�����ł��B</returns>
        static public async Task<int> SimpleMenuWithoutSystemAsync(string prompt, SimpleMenuItem[] items)
        {
            EnableEquipButtons(false);
            return await Actions.simpleMenuMethodAsync(prompt, items, null);
        }

        /// <summary>
        /// ����̃��j���[���o���܂�
        /// </summary>
        /// <param name="prompt">�v�����v�g������ł��B</param>
        /// <param name="yesMessage">�m��̑I����������ł��B</param>
        /// <param name="noMessage">�ے�̑I����������ł��B</param>
        /// <returns>�m��̑I�������I�΂ꂽ��True��Ԃ��܂��B</returns>
        static public async Task<bool> YesNoMenuAsync(string prompt, string yesMessage, string noMessage)
        {
            SimpleMenuItem[] items = {
                new SimpleMenuItem(yesMessage ),
                new SimpleMenuItem(noMessage ),
            };
            EnableEquipButtons(false);
            return await Actions.simpleMenuMethodAsync(prompt, items, null) == 0;
        }

        /// <summary>
        /// �O���̃��j���[���o���܂�
        /// </summary>
        /// <param name="prompt">�v�����v�g������ł��B</param>
        /// <param name="message1">�I����1������ł��B</param>
        /// <param name="message2">�I����2������ł��B</param>
        /// <param name="message3">�I����3������ł��B</param>
        /// <returns>0,1,2�őI�΂ꂽ���ڂ������܂��B</returns>
        static public async Task<int> �O��Async(string prompt, string message1, string message2, string message3)
        {
            SimpleMenuItem[] items = {
                new SimpleMenuItem(message1 ),
                new SimpleMenuItem(message2 ),
                new SimpleMenuItem(message3 ),
            };
            EnableEquipButtons(false);
            return await Actions.simpleMenuMethodAsync(prompt, items, null);
        }

        public static Func<bool,Dummy> EnableEquipButtonsBody;
        public static void EnableEquipButtons(bool b)
        {
            if (EnableEquipButtonsBody != null) EnableEquipButtonsBody(b);
        }

    }
}
