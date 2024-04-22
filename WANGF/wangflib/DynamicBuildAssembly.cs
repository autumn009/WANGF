using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Microsoft.CSharp;

namespace ANGFLib
{
    class DynamicBuildAssembly
    {
        private const string starter = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using ANGFLib;


[assembly: AssemblyTitle(""{2}"")]
[assembly: AssemblyVersion(""{3}"")]
[assembly: AssemblyFileVersion(""{3}"")]

[AutoFlags(""StdFlagSetOf{1}"")]
public class {0}Module : ANGFLib.Module
{{
    [DynamicObjectFlag(""Std"")]
    public static dynamic MyFlags = new SimpleDynamicObject();  // save/load storage
    public static dynamic MyTempFlags = new SimpleDynamicObject();  // not save/load storage
    public override string Id {{ get {{ return ""{1}""; }} }}
";
        private const string ender = @"
}
";
        private static void generaleGetter(StringBuilder sb, string Name, XElement place, bool forceToExist = false, Type type = null)
        {
            XNamespace ns = XmlNamespacesConstants.StdXmlNamespace;
            var elem = place.Element(ns + Name);
            if (elem == null && !forceToExist) return;
            var elemCs = elem.Element(ns + "Cs");
            sb.AppendFormat("{0}Getter=()=>", Name);
            if (elemCs == null)
            {
                if (type == typeof(bool))
                {
                    bool val = false;
                    if (elem.Value.Trim().ToLower() == "1" || elem.Value.Trim().ToLower() == "true") val = true;
                    sb.AppendFormat("{0},\r\n", val ? "1" : "0");
                }
                else if (type == typeof(int))
                {
                    sb.AppendFormat("{0},\r\n", elem.Value.Trim());
                }
                else
                {
                    sb.AppendFormat("@\"{0}\",\r\n", elem.Value.Replace("\"", "\"\""));
                }
            }
            else
            {
                // Line指令を挿入してエラー情報を改善できるか?
                sb.AppendLine();
                sb.AppendFormat("#Line {0}\r\n", ((System.Xml.IXmlLineInfo)elemCs).LineNumber);
                sb.AppendFormat("{0},\r\n", elemCs.Value);
            }
            return;
        }

        private static void buildPlaces(StringBuilder sb, IEnumerable<XElement> places)
        {
            XNamespace ns = XmlNamespacesConstants.StdXmlNamespace;
            if (places.Count() == 0) return;
            sb.AppendLine("public override ANGFLib.Place[] GetPlaces()");
            sb.AppendLine("{");
            sb.AppendLine("return new Place[] {");
            foreach (var p in places)
            {
                sb.AppendLine("new SimplePlace(){");
                generaleGetter(sb, "Id", p, true, typeof(string));
                generaleGetter(sb, "Name", p, true, typeof(string));
                generaleGetter(sb, "ForceOverride", p, false, typeof(bool));
                generaleGetter(sb, "Parent", p, false, typeof(string));
                generaleGetter(sb, "Visible", p, false, typeof(bool));
                generaleGetter(sb, "PositionX", p, false, typeof(int));
                generaleGetter(sb, "PositionY", p, false, typeof(int));
                var see = p.Element(ns + "See");
                if (see != null)
                {
                    var superTalk = see.Element(ns + "SuperTalk");
                    if (superTalk != null)
                    {
                        sb.AppendFormat("SeeSuperTalk=@\"{0}\",", superTalk.Value.Replace("\"", "\"\""));
                    }
                }


                // TBW

                sb.AppendLine("},");
            }
            sb.AppendLine("};");
            sb.AppendLine("}");
        }

        private static string getCond(XNamespace ns, XElement p)
        {
            var cond = p.Element(ns + "Cond");
            if( cond == null ) return "true";
            if( string.IsNullOrWhiteSpace(cond.Value) ) return "true";
            return cond.Value;
        }

        private static void buildPersons(StringBuilder sb, IEnumerable<XElement> persons)
        {
            XNamespace ns = XmlNamespacesConstants.StdXmlNamespace;
            if (persons.Count() == 0) return;
            sb.AppendLine("public override ANGFLib.Person[] GetPersons()");
            sb.AppendLine("{");
            sb.AppendLine("return new Person[] {");
            foreach (var p in persons)
            {
                var sexNode = p.Element(ns + "Sex");
                string sex = sexNode != null ? sexNode.Value : "0";
                var placeNode = p.Element(ns + "PersonPlace");
                string place= placeNode!=null ? placeNode.Value:"";
                sb.AppendFormat("new SimplePerson(\"{0}\",\"{1}\",Sex.{2},\"{3}\",(p)=>{4},null){{\r\n",
                    p.Element(ns + "Id").Value,
                    p.Element(ns + "Name").Value,
                    (Sex)int.Parse(sex),
                    place,
                    getCond(ns, p));
                generaleGetter(sb, "ForceOverride", p, false, typeof(bool));
                var talk = p.Element(ns + "Talk");
                if (talk != null)
                {
                    var superTalk = talk.Element(ns + "SuperTalk");
                    if (superTalk != null)
                    {
                        sb.AppendFormat("TalkSuperTalk=@\"{0}\",", superTalk.Value.Replace("\"", "\"\""));
                    }
                }

                // TBW

                sb.AppendLine("},");
            }
            sb.AppendLine("};");
            sb.AppendLine("}");
        }

        private static string buildBody(CollectionItem item)
        {
            var simpleCollectionItem = item as SimpleCollectionItem;
            if (simpleCollectionItem == null) return "";    // no action requied
            return string.Format("SuperTalk.CollectionPlay(superTalk{0},@\"{1}\",@\"{2}\");\r\n", General.ToCSKeyword(simpleCollectionItem.SuperTalk.Id), csAtText(simpleCollectionItem.Id), csAtText(simpleCollectionItem.OwnerModuleId));
        }

        private static string csAtText(string src)
        {
            return src.Replace("\"", "\"\"");
        }

        private static void buildCollections(StringBuilder sb, IEnumerable<XElement> superTalks, string ownerModuleId)
        {
            var superTalkCollections = superTalks.Select((n) => new SuperTalkCollections(n.Value)).ToArray();
            List<SuperTalkCollections.CG> groups = new List<SuperTalkCollections.CG>();
            foreach (var superTalkCollection in superTalkCollections) groups.AddRange(superTalkCollection.CollectionGroups.Values);

            Dictionary<string, Collection> collections = new Dictionary<string, Collection>();
            foreach (var n in superTalkCollections)
            {
                foreach (var m in n.Collections.Values)
                {
                    if (!collections.Keys.Contains(m.KindId))
                    {
                        string name = Collection.List[m.KindId].Name;
                        collections.Add(m.KindId, new Collection() { RawId = m.KindId, Name = name, Collections = new CollectionItem[0] });
                    }
                    if (m.ParentId == null)
                    {
                        var item = new SimpleCollectionItem() { Id = m.Id, Name = m.Name, SuperTalk = n.SuperTalk, OwnerModuleId = ownerModuleId };
                        var t = collections[m.KindId].Collections.ToList();
                        t.Add(item);
                        collections[m.KindId].Collections = t.ToArray();
                    }
                    else
                    {
                        // グループがなければグループをつくる
                        var found = collections[m.KindId].Collections.FirstOrDefault(c=>c.Id == m.ParentId);
                        if (found == null)
                        {
                            var list = collections[m.KindId].Collections.ToList();
                            var foundInGroup = groups.FirstOrDefault(c => c.Id == m.ParentId);
                            found = new CollectionItem() { Id = m.ParentId, Name = foundInGroup.Name, GetRawSubItems = () => new ANGFLib.CollectionItem[0] };
                            list.Add(found);
                            collections[m.KindId].Collections = list.ToArray();
                        }
                        var list2 = found.GetRawSubItems().ToList();
                        list2.Add(new SimpleCollectionItem()
                        {
                             Id = m.Id,
                             Name = m.Name,
                             SuperTalk = n.SuperTalk,
                             OwnerModuleId = ownerModuleId,
                        });
                        found.GetRawSubItems = () => list2.ToArray();
                    }
                }
            }

            foreach (var n in superTalkCollections)
            {
                sb.AppendFormat("public const string superTalk{0}=@\"{1}\";\r\n", General.ToCSKeyword(n.SuperTalk.Id), csAtText(n.SuperTalk.Source));
            }

            sb.AppendLine("public override Collection[] GetCollections()");
            sb.AppendLine("{");
            sb.AppendLine("return new Collection[]");
            sb.AppendLine("{");
            foreach (var n in collections.Values)
            {
                sb.AppendLine("new SimpleOverwriteCollection()");
                sb.AppendLine("{");
                sb.AppendFormat("RawId = \"{0}\",\r\n", n.Id);
                sb.AppendFormat("Name = \"{0}\",\r\n",n.Name.Replace("\"","\\\""));
                sb.AppendLine("Collections = new CollectionItem[]");
                sb.AppendLine("{");

                foreach (var m in n.Collections)
                {
                    sb.AppendFormat("new CollectionItem(){{Name=\"{0}\",Id=\"{1}\", Procedure=(subid)=>{{{2}}},",
                            m.Name.Replace("\"", "\\\""),
                            m.Id, buildBody(m));
                    if (m.GetRawSubItems != null)
                    {
                        sb.AppendLine("GetRawSubItems = () =>{ return new[] {");
                        foreach (var o in m.GetRawSubItems())
                        {
                            sb.AppendFormat("new CollectionItem() {{ Id = \"{0}\", Name = \"{1}\", Procedure=(subid)=>{{{2}}} }},\r\n", o.Id, o.Name.Replace("\"", "\\\""), buildBody(o));
                        }
                        sb.AppendLine("}; },");
                    }
                    sb.AppendLine("},");
                }

                sb.AppendLine("}");
                sb.AppendLine("},");
            }
            sb.AppendLine("};");
            sb.AppendLine("}");

        }

        private static void buildCodeFragments(StringBuilder sb, IEnumerable<XElement> superTalks)
        {
            var superTalkCollections = superTalks.Select((n) => new SuperTalkCollections(n.Value)).ToArray();
            foreach (var n in superTalkCollections)
            {
                foreach (var m in n.CodeFragments)
                {
                    sb.AppendFormat("public void {0}()\r\n",General.GenerateProcName(m.Key));
                    sb.AppendLine("{");
                    sb.AppendLine(m.Value);
                    sb.AppendLine("}");
                }
            }
        }

        private static string buildSource(MyXmlDoc module, out string[] additionalReferModuleNames )
        {
            var sb = new StringBuilder();
            XNamespace ns = XmlNamespacesConstants.StdXmlNamespace;
            string moduleId = "Mod" + module.id;
            sb.AppendFormat(starter, "Mod" + General.ToCSKeyword(module.id), moduleId, module.name, module.versionInfo.ToString());

            // References
            var refers = module.moduleEx.Elements(ns + "Refer");
            additionalReferModuleNames = refers.Select(c => c.Value).ToArray();

            // Places
            var places = module.moduleEx.Elements(ns + "Place");
            buildPlaces(sb, places);

            // Persons
            var persons = module.moduleEx.Elements(ns + "Person");
            buildPersons(sb, persons);

            // Collections
            var superTalks = module.moduleEx.Descendants(ns + "SuperTalk");
            buildCollections(sb, superTalks, moduleId);

            // CodeFragments
            buildCodeFragments(sb, superTalks);


            sb.Append(ender);
            return sb.ToString();   //.Split(new string[] { "\r\n" }, StringSplitOptions.None);
        }

#if MYBLAZORAPP
        [Obsolete]
        internal static Assembly CreateAssembly(ReferModuleInfo n, StringBuilder sb, out string source)
        {
            source = null;
            return null;
        }
#else
        internal static Assembly CreateAssembly(ReferModuleInfo n, StringBuilder sb, out string source)
        {
            var module = Scenarios.Modules.First(c => c.id == n.Id);

            string[] additionalReferModuleNames;
            source = buildSource(module, out additionalReferModuleNames);
            var compiler = CodeDomProvider.CreateProvider("CSharp");
            var opt = new CompilerParameters();
            opt.ReferencedAssemblies.Add("System.dll");
            opt.ReferencedAssemblies.Add("System.Core.dll");
            opt.ReferencedAssemblies.Add("System.Drawing.dll");
            opt.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            opt.ReferencedAssemblies.Add("ANGFLib.dll");
            foreach (var item in additionalReferModuleNames)
            {
                foreach (var n2 in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (Path.GetFileName(n2.Location).ToLower() == item.ToLower())
                    {
                        opt.ReferencedAssemblies.Add(n2.Location);
                        break;
                    }
                }
            }
            opt.GenerateExecutable = false;
            opt.GenerateInMemory = true;
            opt.IncludeDebugInformation = false;
            var r = compiler.CompileAssemblyFromSource(opt, source);
            if (r.Errors.Count > 0)
            {
                foreach (var e in r.Errors)
                {
                    sb.AppendLine(e.ToString());
                }
                sb.AppendFormat("Errors={0}", r.Errors.Count.ToString());
                return null;
            }
            return r.CompiledAssembly;
        }
#endif
    }
}
