using ANGFLib;
using waSimpleRPGBase;

namespace OnlyEmbeddedModules
{
    public class OnlyGameStartupInfos : GameStartupInfos
    {
        // WASMベースで起動する場合に参照します
        public override IEnumerable<GameStartupInfo> EnumEmbeddedModules()
        {
            return new StaticGameStartupInfo[]
            {
                // これらの行を削除してあなたのモジュールを追加してください
                // はじめてのRPG
                StaticGameStartupInfo.Create(new Type[]{ typeof(RpgBaseModuleEx), typeof(waFirstRPG.HarModuleEx)}),
                // はじめてのお使い
                StaticGameStartupInfo.Create(new Type[]{ typeof(FirstOtsukai.HaoModuleEx)}),
                // お姉ちゃんのお使い
                StaticGameStartupInfo.Create(new Type[]{ typeof(FirstOtsukai.HaoModuleEx) ,typeof(ExtOtsukai.ExtOtsukaiModuleEx)}),
            };
        }
        public override string VersionMessage => "Basic Games 0.01";
    }
}
