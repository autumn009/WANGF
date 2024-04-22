using ANGFLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace waRazorUI
{
    class CollectionListBuilder
    {
        private string Id;
        private bool lock解除;  // 全ての項目を表示して良いか
        private bool okEnabled;  // 単なるリスト表示ではなく選択機能が有効化
        private Collection collection;
        private string name;
        public CollectionListBuilder(string Id, bool lock解除, bool okEnabled)
        {
            this.Id = Id;
            this.lock解除 = lock解除;
            this.okEnabled = okEnabled;
            collection = SimpleName<Collection>.List.Values.First((x) => x.Id == this.Id);
            name = collection.Name;
        }

        internal void Build(Func<CollectionItem, bool, CollectionItem, State.CollectionState, Dummy> p)
        {
            DefaultPersons.独白.Say(lock解除 ? "全" + name + "一覧" : "累積された" + name + "一覧");
            int availableCount = 0, totalCount = 0;
            foreach (var item in collection.Collections)
            {
                if (item.GetRawSubItems == null)
                {
                    if (State.HasCollection(Id, item.Id, null) != State.CollectionState.None) availableCount++;
                    if (!item.Hidden) totalCount++;
                    var visualState = State.HasCollection(Id, item.Id, null);
                    p(item, visualState != State.CollectionState.None, null, visualState);
                }
                else
                {
                    bool rootVisible = false;
                    foreach (var subitem in item.GetSubItems())
                    {
                        if (State.HasCollection(Id, item.Id, subitem.Id) != State.CollectionState.None)
                        {
                            rootVisible = true;
                            break;
                        }
                    }
                    foreach (var subitem in item.GetSubItems())
                    {
                        var visualState = State.HasCollection(Id, item.Id, subitem.Id);
                        if (visualState != State.CollectionState.None) availableCount++;
                        if (!subitem.Hidden) totalCount++;
                        p(item, rootVisible, subitem, visualState);
                    }
                }
            }

            // totalCountは隠し以外の数であるため、availableCount * 100 / totalCountは
            // 100を超える可能性があるが、表示上は100までしか見せない

            int 獲得率 = totalCount == 0 ? 0 : Math.Min(100, availableCount * 100 / totalCount);

            DefaultPersons.システム.Say(string.Format(name + "数{0}/全数{1} 獲得率{2}%",
                availableCount, totalCount, 獲得率));
        }
    }
}
