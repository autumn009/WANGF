using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Win32;

namespace ANGFLib
{
    /// <summary>
    /// 生のMyXmlDoc
    /// </summary>
    [Serializable]
    public class RawMyXmlDoc : MarshalByRefObject
    {
        /// <summary>
        /// AngfRuntime.xmlの位置。埋め込まれていたらDLLの位置。システムモジュールの場合はnull位置。埋め込まれていたらDLLの位置。システムモジュールの場合はnull
        /// </summary>
        public string AngfRuntimeXmlFilePath;
        /// <summary>
        /// AngfRuntime.xmlのid
        /// </summary>
        public string id;
        /// <summary>
        /// AngfRuntime.xmlのname
        /// </summary>
        public string name;
        /// <summary>
        /// AngfRuntime.xmlのdescription (もしあれば)
        /// </summary>
        public string description;
        /// <summary>
        /// AngfRuntime.xmlのis18k。無ければfalse
        /// </summary>
        public bool is18k;
        /// <summary>
        /// AngfRuntime.xmlのmodule (DLLファイル名パス)
        /// </summary>
        /// 
        public string module;
        /// <summary>
        /// 文字列形式のmoduleEx要素ノード
        /// </summary>
        public string rawModuleEx;
        /// <summary>
        /// moduleEx要素ノード
        /// </summary>
        public XElement moduleEx
        {
            get { return (rawModuleEx != null) ? XElement.Parse(rawModuleEx) : null; }
            set { rawModuleEx = (value != null) ? value.ToString() : null; }
        }
        /// <summary>
        /// 最低バージョン
        /// </summary>
        public Version MinVersion;
        /// <summary>
        /// スタートアップ モジュールならtrue
        /// </summary>
        public bool startupModule;
        /// <summary>
        /// シェアワールドとなる別モジュールのID
        /// </summary>
        public string shareWorld;
        /// <summary>
        /// シェアワールドとなる別モジュールの最低バージョン
        /// </summary>
        public Version shareWorldMinVersion;
        /// <summary>
        /// 必要とされるモジュール一覧
        /// </summary>
        public ReferModuleInfo[] require;
        /// <summary>
        /// 読み込んだパス
        /// </summary>
        public string xmlFilePath;
        /// <summary>
        /// ファイルのバージョン。無い場合はnull。まだ読み込んでいない場合もnull。
        /// 全モジュールのマニフェストを読み込む際に設定されるので、それ以後であれば
        /// たいてい参照可能と思って良い　(非nullの値が入っていると思って良い)
        /// </summary>
        public Version versionInfo;
        /// <summary>
        /// 自動テスト可能ならtrue
        /// </summary>
        public bool AutoTestEnabled;
        /// <summary>
        /// 起動時のタイトルCG(JEPG)があればそのファイルイメージ。なければnull
        /// </summary>
        public byte[] TitlePicture;
    }
    /// <summary>
    /// AngfRuntime.xmlを内部処理用に表現します
    /// </summary>
    [Serializable]
    public class MyXmlDoc : RawMyXmlDoc
    {
        /// <summary>
        /// 追加で参照されるべきモジュールの名前 (ダイナミックにコンパイルされる場合のみ)
        /// 実際はmoduleExの中にアクセスするラッパである
        /// </summary>
        public string[] AdditionalReferModuleNames
        {
            get
            {
                XNamespace ns = ANGFLib.XmlNamespacesConstants.StdXmlNamespace;
                return moduleEx.Descendants(ns + "Refer").Select(c => c.Value.Trim()).ToArray();
            }
            set
            {
                XNamespace ns = ANGFLib.XmlNamespacesConstants.StdXmlNamespace;
                // remove all Refer nodes
                for (; ; )
                {
                    var found = moduleEx.Descendants(ns + "Refer").FirstOrDefault();
                    if (found == null) break;
                    found.Remove();
                }
                // add new Refer nodes
                foreach (var n in value)
                {
                    moduleEx.Add(new XElement(ns + "Refer", n));
                }
            }
        }

        /// <summary>
        /// モジュールに対応して読み込むDLLがあるときそのフルパス
        /// 無ければnull
        /// nullでないならそのパスは有効であるはずだ
        /// </summary>
        public string ModuleDllFilePath
        {
            get
            {
                if (module == null || AngfRuntimeXmlFilePath == null) return null;
                if (AngfRuntimeXmlFilePath.StartsWith("http:") || AngfRuntimeXmlFilePath.StartsWith("https:")) return AngfRuntimeXmlFilePath;
                // Webプレイヤーがテンポラリファイルとして読み込んだパスはそのまま使うべき
                if (AngfRuntimeXmlFilePath.ToLower().EndsWith(".tmp")) return AngfRuntimeXmlFilePath;
                if (AngfRuntimeXmlFilePath.ToLower().EndsWith(".dll")) return AngfRuntimeXmlFilePath;
                return Path.Combine(Path.GetDirectoryName(AngfRuntimeXmlFilePath), module);
            }
        }

        /// <summary>
        /// そのモジュールを代表するファイルが存在する位置
        /// 人間可読であるため、パスとして有効では無い説明文字列が返る可能性がある
        /// </summary>
        public string DisplayFilePath
        {
            get
            {
                if (AngfRuntimeXmlFilePath == null) return "(not a file)";
                return AngfRuntimeXmlFilePath;
            }
        }

        /// <summary>
        /// ロードします
        /// </summary>
        /// <param name="doc">解析対象のXMLオブジェクト</param>
        /// <param name="filename">XMLオブジェクトを読み込んだ実在するファイルパス</param>
        /// <returns>成功したらtrue</returns>
        public bool LoadFromXDocument(XDocument doc, string filename)
        {
            AngfRuntimeXmlFilePath = filename;
            XNamespace ns = ANGFLib.XmlNamespacesConstants.StdXmlNamespace;
            var src = doc.Descendants(ns + "module");
            var srcEx = doc.Descendants(ns + "moduleEx");
            if (!((src.Count() > 0) ^ (srcEx.Count() > 0))) return false;
            this.module = src.FirstOrDefault() == null ? null : src.First().Value;
            this.moduleEx = srcEx.FirstOrDefault();
            this.name = (from n in doc.Descendants(ns + "name") select n).First().Value;
            if (doc.Descendants(ns + "description").FirstOrDefault() != null)
            {
                this.description = doc.Descendants(ns + "description").First().Value;
            }
            this.id = (from n in doc.Descendants(ns + "id") select n).First().Value;
            this.startupModule = int.Parse((from n in doc.Descendants(ns + "startupModule") select n).First().Value) != 0;
            var is18kFirstNodes = doc.Descendants(ns + "is18k").FirstOrDefault();
            this.is18k = false;
            if (is18kFirstNodes != null)
            {
                Util.TryParseMyBool(is18kFirstNodes.Value, out this.is18k);
            }
            var autoTestFirstNodes = doc.Descendants(ns + "autotest").FirstOrDefault();
            this.AutoTestEnabled = false;
            if (autoTestFirstNodes != null)
            {
                Util.TryParseMyBool(autoTestFirstNodes.Value, out this.AutoTestEnabled);
            }
            var shareWordNode = doc.Descendants(ns + "shareWorld").First();
            this.shareWorld = shareWordNode.Value;
            var verNodeParent = shareWordNode.Attribute("version");
            this.shareWorldMinVersion = verNodeParent != null ? Version.Parse(verNodeParent.Value) : new Version();
            var r = new List<ReferModuleInfo>();
            foreach (var n in from n in doc.Descendants(ns + "require") select n)
            {
                // 空の要素は書くことが許されており合法であるが、無効である
                if (n.Value.Trim().Length == 0) continue;
                var nameNode = n.Attribute("name");
                var verNode = n.Attribute("version");
                r.Add(new ReferModuleInfo()
                {
                    Id = n.Value,
                    FullPath = null,
                    Name = nameNode != null ? nameNode.Value : n.Value,
                    MinVersion = verNode != null ? Version.Parse(verNode.Value) : new Version(),
                });
            }
            this.require = r.ToArray();

            if (this.moduleEx != null)
            {
                var staticVersion = this.moduleEx.Element(ns + "Version");
                if (staticVersion != null) Version.TryParse(staticVersion.Value, out this.versionInfo);
            }

            return true;
        }
        private XElement createXelementWithVersion(string elemName, string name, string val, Version version)
        {
            XNamespace ns = ANGFLib.XmlNamespacesConstants.StdXmlNamespace;
            var node = new XElement(ns + elemName);
            if (name != null) node.Add(new XAttribute("name", name));
            if (version != null && version != new Version()) node.Add(new XAttribute("version", version.ToString()));
            node.Add(val);
            return node;
        }
        /// <summary>
        /// セーブします
        /// </summary>
        /// <returns>XMLオブジェクトにセーブされた結果</returns>
        public XDocument SaveToXDocument()
        {
            XNamespace ns = ANGFLib.XmlNamespacesConstants.StdXmlNamespace;
            var doc = new XDocument();
            var root = new XElement(ns + "root");
            doc.Add(root);
            if (this.module != null) root.Add(new XElement(ns + "module", this.module));
            if (this.moduleEx != null)
            {
                root.Add(this.moduleEx);
                var version = this.moduleEx.Element(ns + "Version");
                if (version != null)
                {
                    version.Value = this.versionInfo.ToString();
                }
                else
                {
                    moduleEx.Add(new XElement(ns + "Version", this.versionInfo.ToString()));
                }
            }
            root.Add(new XElement(ns + "name", this.name));
            if (this.description != null)
            {
                root.Add(new XElement(ns + "description", this.description));
            }
            root.Add(new XElement(ns + "id", this.id));
            root.Add(new XElement(ns + "startupModule", this.startupModule ? "1" : "0"));
            root.Add(createXelementWithVersion("shareWorld", null, this.shareWorld, this.shareWorldMinVersion));
            if (this.require != null)
            {
                foreach (var r in this.require) root.Add(createXelementWithVersion("require", r.Name, r.Id, r.MinVersion));
            }
            return doc;
        }

        /// <summary>
        /// ダミーのオブジェクトを作成する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static MyXmlDoc CreateErrorObject(string s)
        {
            var alter = new MyXmlDoc();
            alter.AngfRuntimeXmlFilePath = s;
            alter.id = Guid.NewGuid().ToString("N");
            alter.name = "読み込めなかったモジュール";
            alter.description = s;
            alter.module = s;
            alter.rawModuleEx = null;
            alter.MinVersion = new Version(0, 0, 0, 0);
            alter.startupModule = false;
            alter.shareWorld = null;
            alter.shareWorldMinVersion = new Version(0, 0, 0, 0);
            alter.require = new ReferModuleInfo[0];
            alter.AngfRuntimeXmlFilePath = s;
            alter.versionInfo = null;
            alter.xmlFilePath = s;
            return alter;
        }
    }
}
