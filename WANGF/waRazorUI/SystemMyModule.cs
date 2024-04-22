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
<name>システム</name>
<id>{93029964-704B-4d38-BE1D-EDB6182602E8}</id>
<shareWorld></shareWorld>
<require></require>
</root>
";
        public override string Id { get { return "{3273845a-2f5e-4b9b-85d5-34c678a2ccdb}"; } }

        // OnMenuを呼べ
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
            // フロントメニューは仮想の場なので、出入りは宣言しない
        }

        public override void OnLeaveing()
        {
            // フロントメニューは仮想の場なので、出入りは宣言しない
        }

        public override string HumanReadableName
        {
            get { return "(SYSTEMメニュー)"; }
        }
        public override bool IsStatusHide
        {
            get { return true; }
        }

        public override async Task OnMenuAsync()
        {
            List<SimpleMenuItem> items = new List<SimpleMenuItem>();
            items.Add(new SimpleMenuItem("全ファイル・ダウンロード", async () =>
            {
                var q = new QuickTalk();
                q.Play("""
                    Webブラウザのローカルストレージに保存されたデータは、ローカルストレージをクリアすると消えてしまいます。
                    本コマンドでバックアップを推奨します。
                    他のマシン、他のWebブラウザでプレイを継続する場合は、本コマンドでダウンロードしたデータを別のマシン、Webブラウザからアップロードして御使用ください。
                    ダウンロードを中止する場合はキャンセルで戻って下さい。
                    """);
                int r = await UI.SimpleMenuWithCancelAsync("ダウンロードしますか?", new SimpleMenuItem[] { new SimpleMenuItem("ダウンロードする") });
                if (r < 0) return false;

                var keys = await wangfUtil.LocalStorage.EnumItemKeysAsync();

                byte[] result;
                using (var ms = new MemoryStream())
                {
                    // メモリストリーム上にZipArchiveを作成する
                    using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        foreach (var key in keys)
                        {
                            var str = await wangfUtil.LocalStorage.GetItemAsync(key);
                            if (str != null)
                            {
                                DefaultPersons.独白.Say($"archiving: {key}");
                                var data = Encoding.UTF8.GetBytes(str);
                                var entry = zipArchive.CreateEntry(wangfUtil.FileNameEncoder(key));
                                using (var es = entry.Open())
                                {
                                    // エントリにバイナリを書き込む
                                    es.Write(data, 0, data.Length);
                                }
                            }
                            else
                                DefaultPersons.独白.Say($"skipped: {key}");
                        }
                    }
                    result = ms.ToArray();
                }
                DefaultPersons.独白.Say($"size: {result.Length}");

                await JsWrapper.DownloadFileFromStreamAsync("wangfBackup" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip", result);

                return true;
            }));
            items.Add(new SimpleMenuItem("全ファイル・アップロード", async () =>
            {
                var q = new QuickTalk();
                q.Play("""
                    全ファイル・ダウンロードで保存したZIPファイルを復元します。
                    現在、ブラウザ内にあるWANGF関連ファイルは全て消去され、アップロードしたファイルで置き換えられます。
                    セーブしたデータも集めた各種コレクションなども全て消えます。
                    アップロードを中止する場合はキャンセルで戻って下さい。
                    """);
                HtmlGenerator.UploadEnabled = true;
                var old = HtmlGenerator.UploadDescription;
                int r = await UI.SimpleMenuWithCancelAsync("キャンセルする場合", new SimpleMenuItem[0]);
                HtmlGenerator.UploadEnabled = false;
                return true;
            }));
            items.Add(new SimpleMenuItem("全ファイル・クリア", async () =>
            {
                var q = new QuickTalk();
                q.Play("""
                    ゲームを完全に初期状態に戻します。
                    現在、ブラウザ内にあるWANGF関連ファイルは全て消去されます。
                    セーブしたデータも集めた各種コレクションなども全て消えます。
                    クリアを中止する場合はキャンセルで戻って下さい。
                    """);
                int r = await UI.SimpleMenuWithCancelAsync("クリアしますか?", new SimpleMenuItem[] { new SimpleMenuItem("クリアする") });
                if (r == 0)
                {
                    if (await UI.YesNoMenuAsync("本当にクリアしますか?", "する", "しない"))
                    {
                        await wangfUtil.LocalStorage.ClearAllAsync();
                        State.Terminate();
                    }
                }
                return true;
            }));
            items.Add(new SimpleMenuItem("終了", async () =>
            {
                // 終了の意志を確認。確認時は既にcurrentPalceがPlaceNullになっている
                await UI.DoneConfirmAsync();
                // 意思確認がYesなら再起動させる
                if (string.IsNullOrWhiteSpace(Flags.CurrentPlaceId))
                {
                    State.Terminate();
                }
                return true;
            }));
            await UI.SimpleMenuWithoutSystemAsync("機能を選択", items.ToArray());
        }
    }
}
