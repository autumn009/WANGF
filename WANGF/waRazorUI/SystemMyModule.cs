using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ANGFLib;

namespace waRazorUI
{
    public class SystemMyModule : Module
    {
        public const string xmlDocument = @"<?xml version='1.0' encoding='utf-8' ?>
<root xmlns='http://angf.autumn.org/std001'>
<module></module>
<startupModule>1</startupModule>
<name>�V�X�e��</name>
<id>{93029964-704B-4d38-BE1D-EDB6182602E8}</id>
<shareWorld></shareWorld>
<require></require>
</root>
";
        public override string Id { get { return "{3273845a-2f5e-4b9b-85d5-34c678a2ccdb}"; } }

        // OnMenu���Ă�
        public override bool ConstructMenu(List<SimpleMenuItem> list, Place place)
        {
            return false;
        }
        public override Place[] GetPlaces()
        {
            return new Place[] {
                new PlaceFrontMenu(),
            };
        }
        public override string GetStartPlace()
        {
            return Constants.SystemPlaceID;
        }
    }
    public class SystemMyModuleEx : ModuleEx
    {
        public override T[] QueryObjects<T>()
        {
            if (typeof(T) == typeof(ANGFLib.Module)) return new T[] { new SystemMyModule() as T };
            if (typeof(T) == typeof(XDocument)) return new T[] { XDocument.Parse(SystemMyModule.xmlDocument) as T };
            return new T[0];
        }
    }
    public class PlaceFrontMenu : Place
    {
        public override string Id
        {
            get { return Constants.SystemPlaceID; }
        }

        public override void OnEntering()
        {
            // �t�����g���j���[�͉��z�̏�Ȃ̂ŁA�o����͐錾���Ȃ�
        }

        public override void OnLeaveing()
        {
            // �t�����g���j���[�͉��z�̏�Ȃ̂ŁA�o����͐錾���Ȃ�
        }

        public override string HumanReadableName
        {
            get { return "(SYSTEM���j���[)"; }
        }
        public override bool IsStatusHide
        {
            get { return true; }
        }

        public override async Task OnMenuAsync()
        {
            List<SimpleMenuItem> items = new List<SimpleMenuItem>();
            items.Add(new SimpleMenuItem("�S�t�@�C���E�_�E�����[�h", async () =>
            {
                var q = new QuickTalk();
                q.Play("""
                    Web�u���E�U�̃��[�J���X�g���[�W�ɕۑ����ꂽ�f�[�^�́A���[�J���X�g���[�W���N���A����Ə����Ă��܂��܂��B
                    �{�R�}���h�Ńo�b�N�A�b�v�𐄏����܂��B
                    ���̃}�V���A����Web�u���E�U�Ńv���C���p������ꍇ�́A�{�R�}���h�Ń_�E�����[�h�����f�[�^��ʂ̃}�V���AWeb�u���E�U����A�b�v���[�h���Č�g�p���������B
                    �_�E�����[�h�𒆎~����ꍇ�̓L�����Z���Ŗ߂��ĉ������B
                    """);
                int r = await UI.SimpleMenuWithCancelAsync("�_�E�����[�h���܂���?", new SimpleMenuItem[] { new SimpleMenuItem("�_�E�����[�h����") });
                if (r < 0) return false;

                var keys = await wangfUtil.LocalStorage.EnumItemKeysAsync();

                byte[] result;
                using (var ms = new MemoryStream())
                {
                    // �������X�g���[�����ZipArchive���쐬����
                    using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        foreach (var key in keys)
                        {
                            var str = await wangfUtil.LocalStorage.GetItemAsync(key);
                            if (str != null)
                            {
                                DefaultPersons.�Ɣ�.Say($"archiving: {key}");
                                var data = Encoding.UTF8.GetBytes(str);
                                var entry = zipArchive.CreateEntry(wangfUtil.FileNameEncoder(key));
                                using (var es = entry.Open())
                                {
                                    // �G���g���Ƀo�C�i������������
                                    es.Write(data, 0, data.Length);
                                }
                            }
                            else
                                DefaultPersons.�Ɣ�.Say($"skipped: {key}");
                        }
                    }
                    result = ms.ToArray();
                }
                DefaultPersons.�Ɣ�.Say($"size: {result.Length}");

                await JsWrapper.DownloadFileFromStreamAsync("wangfBackup" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip", result);

                return true;
            }));
            items.Add(new SimpleMenuItem("�S�t�@�C���E�A�b�v���[�h", async () =>
            {
                var q = new QuickTalk();
                q.Play("""
                    �S�t�@�C���E�_�E�����[�h�ŕۑ�����ZIP�t�@�C���𕜌����܂��B
                    ���݁A�u���E�U���ɂ���WANGF�֘A�t�@�C���͑S�ď�������A�A�b�v���[�h�����t�@�C���Œu���������܂��B
                    �Z�[�u�����f�[�^���W�߂��e��R���N�V�����Ȃǂ��S�ď����܂��B
                    �A�b�v���[�h�𒆎~����ꍇ�̓L�����Z���Ŗ߂��ĉ������B
                    """);
                HtmlGenerator.UploadEnabled = true;
                var old = HtmlGenerator.UploadDescription;
                int r = await UI.SimpleMenuWithCancelAsync("�L�����Z������ꍇ", new SimpleMenuItem[0]);
                HtmlGenerator.UploadEnabled = false;
                return true;
            }));
            items.Add(new SimpleMenuItem("�S�t�@�C���E�N���A", async () =>
            {
                var q = new QuickTalk();
                q.Play("""
                    �Q�[�������S�ɏ�����Ԃɖ߂��܂��B
                    ���݁A�u���E�U���ɂ���WANGF�֘A�t�@�C���͑S�ď�������܂��B
                    �Z�[�u�����f�[�^���W�߂��e��R���N�V�����Ȃǂ��S�ď����܂��B
                    �N���A�𒆎~����ꍇ�̓L�����Z���Ŗ߂��ĉ������B
                    """);
                int r = await UI.SimpleMenuWithCancelAsync("�N���A���܂���?", new SimpleMenuItem[] { new SimpleMenuItem("�N���A����") });
                if (r == 0)
                {
                    if (await UI.YesNoMenuAsync("�{���ɃN���A���܂���?", "����", "���Ȃ�"))
                    {
                        await wangfUtil.LocalStorage.ClearAllAsync();
                        State.Terminate();
                    }
                }
                return true;
            }));
            items.Add(new SimpleMenuItem("�I��", async () =>
            {
                // �I���̈ӎu���m�F�B�m�F���͊���currentPalce��PlaceNull�ɂȂ��Ă���
                await UI.DoneConfirmAsync();
                // �ӎv�m�F��Yes�Ȃ�ċN��������
                if (string.IsNullOrWhiteSpace(Flags.CurrentPlaceId))
                {
                    State.Terminate();
                }
                return true;
            }));
            await UI.SimpleMenuWithoutSystemAsync("�@�\��I��", items.ToArray());
        }
    }
}
