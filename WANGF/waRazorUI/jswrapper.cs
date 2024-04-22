using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace waRazorUI
{
    public static class JsWrapper
    {
        //[Inject]
        public static IJSRuntime JsRuntime { get; set; }

        public static async Task appendBufferAsync(string str)
        {
            await JsRuntime.InvokeAsync<string>("appendBuffer", str);
        }
        public static async Task clearBufferAsync()
        {
            await JsRuntime.InvokeAsync<string>("clearBuffer");
        }
        public static async Task<string> getBufferAsync()
        {
            return await JsRuntime.InvokeAsync<string>("getBuffer");
        }
        public static async Task<string> localStorageGetItemWrapperAsync(string key)
        {
            var r = await JsRuntime.InvokeAsync<string>("localStorageGetItemWrapper", key);
            if (r == null) return null;
            var sb = new StringBuilder();
            for (; ; )
            {
                var r2 = await getBufferAsync();
                if (string.IsNullOrEmpty(r2)) break;
                sb.Append(r2);
            }
            return sb.ToString();
        }
        public static async Task localStorageSetItemWrapperAsync(string key, string val)
        {
            const int limit = 10000;
            await clearBufferAsync();
            int p = 0;
            for (; ; )
            {
                string s;
                if (p + limit >= val.Length) s = val.Substring(p);
                else s = val.Substring(p, limit);
                await appendBufferAsync(s);
                p += s.Length;
                if (p >= val.Length) break;
            }
            await JsRuntime.InvokeAsync<string>("localStorageSetItemWrapper", key);
            await clearBufferAsync();
        }
        public static async Task localStorageRemoveItemWrapperAsync(string key)
        {
            await JsRuntime.InvokeAsync<string>("localStorageRemoveItemWrapper", key);
        }
        public static async Task localStorageClearAllWrapperAsync()
        {
            await JsRuntime.InvokeAsync<string>("localStorageClear");
        }
        public static async Task<string[]> localStorageEnumKeysWrapperAsync()
        {
            var x = await JsRuntime.InvokeAsync<string[]>("localStorageEnumKeys");
            return x;
        }

        public static async Task setBackColorWrapperAsync(string color/*"#RRGGBB"*/)
        {
            await JsRuntime.InvokeAsync<string>("setBackColorWrapper", color);
        }
        public static async Task AlertAsync(string messgae)
        {
            await JsRuntime.InvokeAsync<string>("myalert", messgae);
        }
        public static async Task SetWaitCursorAsync()
        {
            await JsRuntime.InvokeAsync<string>("setWaitCursor");
        }
        public static async Task ResetWaitCursorAsync()
        {
            await JsRuntime.InvokeAsync<string>("resetWaitCursor");
        }

        internal static async Task CollectResultAsync(Dictionary<string, string> rdic)
        {
            string [] r = await JsRuntime.InvokeAsync<string[]>("collectResult");
            for (int i = 0; i < r.Length; i+=2)
            {
                rdic.Add(r[i], r[i + 1]);
            }
        }

        public static async Task DownloadFileFromStreamAsync(string filename, byte[] bytes)
        {
            var fileStream = new MemoryStream(bytes);
            using var streamRef = new DotNetStreamReference(stream: fileStream);
            await JsRuntime.InvokeVoidAsync("downloadFileFromStream", filename, streamRef);
        }
    }
}
