using System;
using System.Collections.Generic;
using System.Linq;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ゲームシステムのレジストリ
    /// </summary>
    public class GameSystemRegistry
    {
        private readonly Dictionary<string, IGameSystem> _systemsById;
        private readonly List<IGameSystem> _sortedSystems;

        /// <summary>
        /// 新しいレジストリを作成する
        /// </summary>
        public GameSystemRegistry()
        {
            _systemsById = new Dictionary<string, IGameSystem>(StringComparer.OrdinalIgnoreCase);
            _sortedSystems = new List<IGameSystem>();
        }

        /// <summary>
        /// 登録されているゲームシステムの数
        /// </summary>
        public int Count => _systemsById.Count;

        /// <summary>
        /// 登録されている全ゲームシステム（ソート済み）
        /// </summary>
        public IReadOnlyList<IGameSystem> Systems => _sortedSystems;

        /// <summary>
        /// ゲームシステムを登録する
        /// </summary>
        /// <param name="gameSystem">登録するゲームシステム</param>
        /// <exception cref="ArgumentException">同じIDのゲームシステムが既に登録されている場合</exception>
        public void Register(IGameSystem gameSystem)
        {
            if (gameSystem == null)
            {
                throw new ArgumentNullException(nameof(gameSystem));
            }

            if (_systemsById.ContainsKey(gameSystem.Id))
            {
                throw new ArgumentException($"Game system with ID '{gameSystem.Id}' is already registered.", nameof(gameSystem));
            }

            _systemsById[gameSystem.Id] = gameSystem;
            RebuildSortedList();
        }

        /// <summary>
        /// 複数のゲームシステムを一括登録する
        /// </summary>
        /// <param name="gameSystems">登録するゲームシステムのコレクション</param>
        public void RegisterAll(IEnumerable<IGameSystem> gameSystems)
        {
            foreach (var gameSystem in gameSystems)
            {
                if (!_systemsById.ContainsKey(gameSystem.Id))
                {
                    _systemsById[gameSystem.Id] = gameSystem;
                }
            }
            RebuildSortedList();
        }

        /// <summary>
        /// IDでゲームシステムを取得する
        /// </summary>
        /// <param name="id">ゲームシステムのID</param>
        /// <returns>ゲームシステム、見つからない場合はnull</returns>
        public IGameSystem? GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return _systemsById.TryGetValue(id, out var system) ? system : null;
        }

        /// <summary>
        /// 名前でゲームシステムを検索する
        /// </summary>
        /// <param name="name">検索する名前（部分一致）</param>
        /// <returns>一致するゲームシステムのリスト</returns>
        public IReadOnlyList<IGameSystem> SearchByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Array.Empty<IGameSystem>();
            }

            return _sortedSystems
                .Where(s => s.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 ||
                           s.Id.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        /// <summary>
        /// 指定したIDのゲームシステムが登録されているか確認する
        /// </summary>
        /// <param name="id">ゲームシステムのID</param>
        /// <returns>登録されている場合はtrue</returns>
        public bool Contains(string id)
        {
            return !string.IsNullOrEmpty(id) && _systemsById.ContainsKey(id);
        }

        /// <summary>
        /// 全てのゲームシステムを削除する
        /// </summary>
        public void Clear()
        {
            _systemsById.Clear();
            _sortedSystems.Clear();
        }

        private void RebuildSortedList()
        {
            _sortedSystems.Clear();
            _sortedSystems.AddRange(_systemsById.Values.OrderBy(s => s.SortKey, StringComparer.OrdinalIgnoreCase));
        }
    }
}
