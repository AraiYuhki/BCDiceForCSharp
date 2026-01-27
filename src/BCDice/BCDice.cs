using System.Collections.Generic;
using BCDice.Core;
using BCDice.GameSystem;

namespace BCDice
{
    /// <summary>
    /// BCDiceライブラリのメインファサードクラス
    /// </summary>
    public static class BCDiceRunner
    {
        private static readonly GameSystemRegistry _registry = new GameSystemRegistry();
        private static IGameSystem _currentSystem = DiceBot.Instance;

        /// <summary>
        /// 静的コンストラクタ - デフォルトのゲームシステムを登録
        /// </summary>
        static BCDiceRunner()
        {
            _registry.Register(DiceBot.Instance);
            _registry.Register(Cthulhu.Instance);
            _registry.Register(SwordWorld2_5.Instance);
            _registry.Register(DoubleCross3.Instance);
            _registry.Register(Insane.Instance);
            _registry.Register(Shinobigami.Instance);
            _registry.Register(Nechronica.Instance);
            _registry.Register(Arianrhod2E.Instance);
            _registry.Register(Emoklore.Instance);
            _registry.Register(Satasupe.Instance);
            _registry.Register(MagicaLogia.Instance);
            _registry.Register(Kamigakari.Instance);
            _registry.Register(MeikyuuKingdom.Instance);
        }

        /// <summary>
        /// ゲームシステムレジストリ
        /// </summary>
        public static GameSystemRegistry Registry => _registry;

        /// <summary>
        /// 現在選択されているゲームシステム
        /// </summary>
        public static IGameSystem CurrentSystem
        {
            get => _currentSystem;
            set => _currentSystem = value ?? DiceBot.Instance;
        }

        /// <summary>
        /// 登録されている全ゲームシステム
        /// </summary>
        public static IReadOnlyList<IGameSystem> GameSystems => _registry.Systems;

        /// <summary>
        /// IDでゲームシステムを取得する
        /// </summary>
        /// <param name="id">ゲームシステムID</param>
        /// <returns>ゲームシステム、見つからない場合はnull</returns>
        public static IGameSystem? GetGameSystem(string id)
        {
            return _registry.GetById(id);
        }

        /// <summary>
        /// ゲームシステムをIDで切り替える
        /// </summary>
        /// <param name="id">ゲームシステムID</param>
        /// <returns>切り替え成功時true</returns>
        public static bool SetGameSystem(string id)
        {
            var system = _registry.GetById(id);
            if (system != null)
            {
                _currentSystem = system;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 現在のゲームシステムでコマンドを実行する
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <returns>実行結果、コマンドが認識されない場合はnull</returns>
        public static Result? Eval(string command)
        {
            return _currentSystem.Eval(command);
        }

        /// <summary>
        /// 現在のゲームシステムでコマンドを実行する
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>実行結果、コマンドが認識されない場合はnull</returns>
        public static Result? Eval(string command, IRandomizer randomizer)
        {
            return _currentSystem.Eval(command, randomizer);
        }

        /// <summary>
        /// 指定したゲームシステムでコマンドを実行する
        /// </summary>
        /// <param name="systemId">ゲームシステムID</param>
        /// <param name="command">コマンド文字列</param>
        /// <returns>実行結果、ゲームシステムまたはコマンドが認識されない場合はnull</returns>
        public static Result? Eval(string systemId, string command)
        {
            var system = _registry.GetById(systemId);
            return system?.Eval(command);
        }

        /// <summary>
        /// 指定したゲームシステムでコマンドを実行する
        /// </summary>
        /// <param name="systemId">ゲームシステムID</param>
        /// <param name="command">コマンド文字列</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>実行結果、ゲームシステムまたはコマンドが認識されない場合はnull</returns>
        public static Result? Eval(string systemId, string command, IRandomizer randomizer)
        {
            var system = _registry.GetById(systemId);
            return system?.Eval(command, randomizer);
        }

        /// <summary>
        /// ゲームシステムを登録する
        /// </summary>
        /// <param name="gameSystem">登録するゲームシステム</param>
        public static void RegisterGameSystem(IGameSystem gameSystem)
        {
            _registry.Register(gameSystem);
        }

        /// <summary>
        /// 名前でゲームシステムを検索する
        /// </summary>
        /// <param name="name">検索文字列（部分一致）</param>
        /// <returns>マッチしたゲームシステムのリスト</returns>
        public static IReadOnlyList<IGameSystem> SearchGameSystems(string name)
        {
            return _registry.SearchByName(name);
        }
    }
}
