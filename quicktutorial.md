# Quick Tutorial

## ���̕����̖ړI
�@Hello World���W���[��(WANGF/HelloWorld�f�B���N�g���Ɠ�������)���쐬���A�f�X�N�g�b�v�łŎ��s����Ƃ���܂ł̎菇��������܂��B

## �O�����
- WANGF�f�X�N�g�b�v�ł̖{��(BlazorMaui001.exe)���N�����邱��
- Visual Studio 2022���C���X�g�[������A.NET 8��DLL���쐬�ł��邱�� (Blazor��MAUI�J���@�\�͓��ɗv��܂���B�쐬����͈̂�ʃN���X���C�u������DLL�����ł�)
- �Œ����Visual Stuio��C#�Ɋւ���m�������邱��
- �Œ����XML�̒m��������Ζ]�܂������A���̒i�K�ł͂Ȃ��Ă���

## �v���W�F�N�g�̍쐬
- Visual Studio 2022���N������
- �y�V�����v���W�F�N�g�̍쐬�z��I��
- �㕔�̌����{�b�N�X��"�N���X ���C�u����"�Ɠ����
- �����̃N���X ���C�u������I�ԁ@(�O��ɕ������t����"Razor�N���X ���C�u����"�Ȃǂ͑I�΂Ȃ�)
- �v���W�F�N�g���A�\�����[�V�������A�ꏊ�͎��R�ɑI��
- �y���ցz������
- .NET 8.0��I��
- �y�쐬�z������

## �ÓI�ȏ��t�@�C���̍쐬
- �v���W�F�N�g��AngfRuneTime.xml�Ƃ������O��XML�t�@�C����V�K�ǉ�����B���̃t�@�C�����͕ύX�ł��Ȃ��B
- AngfRuneTime.xml���J���ȉ��̓��e����͂���

```xml
<?xml version="1.0" encoding="utf-8" ?>
<root xmlns="http://angf.autumn.org/std001">
  <module>HelloWorld.dll</module>
  <startupModule>1</startupModule>
  <name>HelloWorld</name>
  <description>�ŏ��̈���ƂȂ�Œ���̃��W���[���ł�</description>
  <id>{c0541918-4b45-4b25-9a6f-09d42cffb978}</id>
</root>
```

- startupModule�̒l��1���ƁA�Q�[���{�̋N�����̃��j���[�ɒǉ������B���̃��W���[������Q�Ƃ���A�P�̂ł̓v���C�ł��Ȃ����W���[����0�ɂ��Ă����B
- name�v�f�̒l�̓��j���[�ɕ\������閼�O�ɂȂ�̂ŁA������₷���悤��"�Ԏq��HelloWorld"�ȂǂɕύX���Ă��ǂ��B�Y�t��HelloWorld�ƕ���킵���̂Ő���ύX���Ă�����
- description�v�f�̒����͂����̉�����Ȃ̂ōD���Ȃ悤�ɏ��������悤
- id�v�f�̒l�̓��j�[�N��ID�����񂪂���Ηǂ��̂ŁA�Y�t��HelloWorld�Ɗm���ɋ�ʉ\�ɂ��邽�߂ɕύX���Ă������BGUID��������g���`���͑��݂��Ȃ����A�V����GUID������𐶐����Ďg�p����ƈ��S���������B

## �ÓI�ȏ��t�@�C���̖��ߍ���
- Visual Studio�̃\�����[�V�����G�N�X�v���[����AngdRunTime.xml���E�N���b�N���ăv���p�e�B��I��
- �r���h�A�N�V�����𖄂ߍ��݃��\�[�X�ɕύX����

## wangflib.dll�ւ̎Q�Ƃ̒ǉ�
- Visual Studio�̃\�����[�V�����G�N�X�v���[���̈ˑ��֌W���E�N���b�N���A�v���W�F�N�g�Q�Ƃ̒ǉ��Ȃǂ�I�сA���̌�Awangflib.dll�ւ̎Q�Ƃ�ǉ�����Bwangflib.dll��BlazorMaui001�̃r���h�ɐ������Ă���΂ł��Ă���͂��ł���B

## ���W���[���N���X�̍쐬
- �v���W�F�N�g�쐬���Ɏ����I�ɐ������ꂽClass1.cs���E�N���b�N���Ė��O�̕ύX��I�сA"(�D���Ȗ��O).cs"�ɕύX����
- ���̃t�@�C���̒������ȉ��̂悤�ɏ���������

```csharp
using ANGFLib;

namespace HelloWorld
{
    public class HelloWorldModule : ANGFLib.Module
    {
        public override string Id => "{db41d86b-fff7-460e-b682-bb8e7ea3c756}";
    }
    public class HelloWorldModuleEx : ModuleEx
    {
        public override T[] QueryObjects<T>()
        {
            if (typeof(T) == typeof(ANGFLib.Module)) return new T[] { new HelloWorldModule() as T };
            return new T[0];
        }
    }
}

```

�@���j�I�Ȍo�܂ɂ��AWANGF�ɂ�2�̃��W���[���N���X������B1��ModuleEx���p�������g�����̂��郂�W���[���ŐV�������̂ł���B����1�́AModule�N���X���p��������{�I�ȏ����i�[����Â����̂ł���B

�@Module�N���X���p�������N���X�ɂ͍Œ�ł�Id���������Ȃ���΂Ȃ�Ȃ��B�����̓��j�[�N������ł��邪�AGUID���g���Ίm�����������B

�@ModuleEx�N���X���p�������N���X�͏�L�̒ʂ�ύX�����g�p���Ē��������B�ύX���ėǂ��̂̓N���X�������ł���B������񓮍�����S�ɗ���������Ȃ牽���ǂ����������Ă��ǂ��B

## �ꏊ�̍쐬
�@WANGF�ł̓Q�[���J�n���Ƀv���C���[������y�ꏊ�z���K�{�ł���B�������A���̏ꏊ�́y�l�̕����z�̂悤�ȋ�̓I�ȏꏊ�ł͂Ȃ��A�y�Q�[���̃G���g�����X�z�̂悤�Ȓ��ۓI�ȏꏊ�ł��ǂ��B

�@���̂��߂ɂ́APlace�N���X���p�������N���X���쐬����B�Œ�ł�HumanReadableName��Id����������BHumanReadableName�͐l�ԉǂ̖��O��Ԃ��B������̓v���C���[��������Ή��ł��ǂ��BId�͔C�ӂ̕�����ł���B

```csharp
    public class HelloWorldPlace : ANGFLib.Place
    {
        public override string HumanReadableName => "�n���[�E���[���h�������ꏊ";
        public override string Id => "{551c2659-af9c-4942-b78f-8e5c93bda1da}";
    }
```

�@�쐬�����N���X�͂��̂܂܂ł͉��̌��ʂ��������Ȃ��BModule�N���X���p���������W���[���̃N���X�ɁAGetPlaces���\�b�h���������āu���������ꏊ������v�ƃV�X�e�����ɐ錾������B�܂��AGetStartPlace���\�b�h���������āu������ԂŃv���C���[������ꏊ�͂������v�ƃV�X�e�����ɐ錾����BXML�t�@�C����startupModule��1��錾�������W���[����GetStartPlace���\�b�h���������Ȃ���΂Ȃ�Ȃ��B

```csharp
    public class HelloWorldModule : ANGFLib.Module
    {
        public override string Id => "{db41d86b-fff7-460e-b682-bb8e7ea3c756}";
        public override string GetStartPlace() => "{551c2659-af9c-4942-b78f-8e5c93bda1da}";
        public override Place[] GetPlaces() => new Place[] { new HelloWorldPlace() };
    }
```

## ���j���[�̍쐬
�@�ꏊ�Ɋ֘A�t����ꂽ���j���[��\������ɂ́A�ꏊ�N���X��ConstructMenu���\�b�h���I�[�o�[���C�h����B���̒���SimpleMenuItem�N���X�̃C���X�^���X���쐬���A������list��Add����B����ő�1�����̕���������x���Ƃ���{�^�����\�������̂Ńv���C���[�͂�����������Ƃ��o����B�������A���ł��ǉ��ł���B�{�^�����������Ƒ�2�����̃����_�������s�����B�����ł́ADefaultPersons(�V�X�e�����񋟂����{�I�ȃr���h�C�����ꂽ�l�I�u�W�F�N�g)�̒��́u�Ɣ��v����Ɍ��킹�Ă���B�u�Ɣ��v����͔����҂̖��O��\�����Ȃ�����ȁu�l�v�ł���BSay���\�b�h�ň����̕��������ʂɕ\������B���̃����_����bool�l��Ԃ��K�v������̂ōŌ��return true;�Ə�����Ă��邪�l�̓_�~�[�ł���B


```csharp
    public class HelloWorldPlace : ANGFLib.Place
    {
        public override string HumanReadableName => "�n���[�E���[���h�������ꏊ";
        public override string Id => "{551c2659-af9c-4942-b78f-8e5c93bda1da}";
        public override bool ConstructMenu(List<SimpleMenuItem> list)
        {
            list.Add(new SimpleMenuItem("Say Hello World", () =>
            {
                DefaultPersons.�Ɣ�.Say("Hello World");
                return true;
            }));
            return base.ConstructMenu(list);
        }
    }
```

## �r���h
- Visual Studio�Ńr���h���s��
- �r���h����������ƁA�v���W�F�N�g��bin�f�B���N�g���̉��K�w������DLL�t�@�C�������������

## �V���[�g�J�b�g�쐬�Ɣz�u
- DLL�t�@�C�����E�N���b�N���ăV���[�g�J�b�g�̍쐬���j���[��T���@(�n�[�h�����N/�V���{���b�N�����N�ł͂Ȃ��V���[�g�J�b�g�ł���)
- DLL�ւ̃V���[�g�J�b�g��C:\ProgramData\autumn\WANGF\modules�Ɉړ�������

## �e�X�g���s
- WANGF Desktop�{��(BlazorMaui001.exe)���N������ (�ǂ��̃f�B���N�g������̋N���ł��ǂ�)
- �Q�[���ꗗ�Ɏ����Ō��߂����O�̃��W���[���������邱�Ƃ��m�F����
- ������N���b�N����
- Say Hello World�Ə����ꂽ�{�^��������A�����������Hello World�ƕ\������邱�Ƃ��m�F����

## �z�z (Desktop)
- WANGF Desktop�{��(BlazorMaui001.exe)�̔z�z�������ςƂ���
- ����DLL�̃R�s�[��͂ǂ��ł��ǂ�
- DLL�ւ̃V���[�g�J�b�g��C:\ProgramData\autumn\WANGF\modules�ɕK���u��

## �z�z (Web)
�@�ڍ׎菇�͒����Ȃ�̂ł����ł̓q���g�̂ݎ����B�Ȃ��ADLL���̂��̂�Desktop�ł�Web�ł�����ł���B�������AWeb�ł̓t�@�C���𑀍�ł��Ȃ����̐����͑��݂���B

- �V����DLL�Ƃ���OnlyEmbeddedModules���쐬����
- �������玩��DLL���Q�Ƃ��A���񋟃N���X���쐬����B���񋟃N���X��OnlyGameStartupInfos.cs���Q�l�ɂ��č쐬����
- �w�b�h���W���[��(BlazorWA009�Ȃ�)����OnlyEmbeddedModules���Q�Ƃ���
- �����Web�u���E�U����Q�Ǝ��Ɏ���DLL�̖��O���Q�[���̑I�����ɒǉ��\�������悤�ɂȂ�
