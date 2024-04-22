using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Runtime.Serialization.Json;

namespace ANGFLib
{
    public enum WebCommands
    {
        ConfirmIdPassword,
        DownloadModules,
        ListLoadFiles,
        ListSaveFiles,
        ListAutoLoadFiles,
        LoadFile,
        SaveFile,
        GetStars,
        AddStars,
    }
    public static class WebApiCategorys
    {
        public static readonly string Normal = "SAVE";
        public static readonly string Auto = "AUTO";
        public static readonly string SystemFile = "SystemFile";
        public static readonly string SkipFile = "skip";
    }
    [System.Runtime.Serialization.DataContract]
    public class SeriarizableBody
    {
        [System.Runtime.Serialization.DataMember()]
        public string BodyByBase64 { get; set; }
        [System.Runtime.Serialization.IgnoreDataMember()]
        public byte[] Body
        {
            get
            {
                if (BodyByBase64 == null) return null;
                if (BodyByBase64.Length == 0) return null;
                return Convert.FromBase64String(BodyByBase64);
            }
            set
            {
                if (value == null) BodyByBase64 = null;
                else if (value.Length == 0) BodyByBase64 = null;
                else BodyByBase64 = Convert.ToBase64String(value);
            }
        }
    }
    [System.Runtime.Serialization.DataContract]
    public class WebApiParameters : SeriarizableBody
    {
        [Obsolete]
        [System.Runtime.Serialization.DataMember()]
        public string Id { get; set; }
        [Obsolete]
        [System.Runtime.Serialization.DataMember()]
        public string Password { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public WebCommands Command { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string Prompt { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public bool AutoSaveFolderRequest { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string Name { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string Extention { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string Category { get; set; }
    }
    [System.Runtime.Serialization.DataContract]
    public class WebApiResults : SeriarizableBody
    {
        [System.Runtime.Serialization.DataMember()]
        public bool Success { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string ErrorMessage { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string[] Names { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string[] Urls { get; set; }
    }
}
