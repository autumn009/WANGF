using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ANGFLib;

namespace wangflib
{
    public static class BatchTest
    {
        public static IBatchTestingForFramework BatchTestingForFramework { get; set; }
        public static IBatchTestingForEachTitle BatchTestingForEachTitle { get; set; }
    }

    public interface IBatchTestingForFramework
    {
        public Task ClearAsync();/* DIABLE ASYNC WARN */
        public Task<IEnumerable<string>> EnumResultsAsync();/* DIABLE ASYNC WARN */
        public Task StartBatchTestingAsync(int max);/* DIABLE ASYNC WARN */
        public Task TerminateBatchTestingAsync();/* DIABLE ASYNC WARN */
        public Task<DateTime> GetStartDateTimeAsync();
        public Task SetStartDateTimeAsync(DateTime dateTime);
    }
    public interface IBatchTestingForEachTitle
    {
        public Task<bool> IsBatchTestingAsync();/* DIABLE ASYNC WARN */
        public Task<int> GetCountAsync();/* DIABLE ASYNC WARN */
        public Task<int> GetMaxAsync();/* DIABLE ASYNC WARN */
        public Task SetCountAsync(int n);/* DIABLE ASYNC WARN */
        /// <summary>
        /// 自動テストをスタートさせる必要があればtrueを返す
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsStartBatchTestingAsync();/* DIABLE ASYNC WARN */
        public Task EndBatchTestingAsync(bool successed, string name, string result);/* DIABLE ASYNC WARN */
    }
}
