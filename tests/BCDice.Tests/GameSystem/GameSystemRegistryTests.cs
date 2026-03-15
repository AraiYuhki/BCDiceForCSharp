using System;
using BCDice.GameSystem;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class GameSystemRegistryTests
    {
        [Fact]
        public void Register_SingleSystem_IncreasesCount()
        {
            var registry = new GameSystemRegistry();
            registry.Register(DiceBot.Instance);

            Assert.Equal(1, registry.Count);
        }

        [Fact]
        public void Register_MultipleSystem_IncreasesCount()
        {
            var registry = new GameSystemRegistry();
            registry.Register(DiceBot.Instance);
            registry.Register(Cthulhu.Instance);

            Assert.Equal(2, registry.Count);
        }

        [Fact]
        public void Register_DuplicateId_ThrowsException()
        {
            var registry = new GameSystemRegistry();
            registry.Register(DiceBot.Instance);

            Assert.Throws<ArgumentException>(() => registry.Register(DiceBot.Instance));
        }

        [Fact]
        public void GetById_ExistingSystem_ReturnsSystem()
        {
            var registry = new GameSystemRegistry();
            registry.Register(DiceBot.Instance);

            var result = registry.GetById("DiceBot");

            Assert.NotNull(result);
            Assert.Equal("DiceBot", result.Id);
        }

        [Fact]
        public void GetById_CaseInsensitive_ReturnsSystem()
        {
            var registry = new GameSystemRegistry();
            registry.Register(DiceBot.Instance);

            var result = registry.GetById("dicebot");

            Assert.NotNull(result);
            Assert.Equal("DiceBot", result.Id);
        }

        [Fact]
        public void GetById_NonExisting_ReturnsNull()
        {
            var registry = new GameSystemRegistry();
            registry.Register(DiceBot.Instance);

            var result = registry.GetById("Unknown");

            Assert.Null(result);
        }

        [Fact]
        public void SearchByName_PartialMatch_ReturnsResults()
        {
            var registry = new GameSystemRegistry();
            registry.Register(DiceBot.Instance);
            registry.Register(Cthulhu.Instance);

            var results = registry.SearchByName("クトゥルフ");

            Assert.Single(results);
            Assert.Equal("Cthulhu", results[0].Id);
        }

        [Fact]
        public void SearchByName_IdMatch_ReturnsResults()
        {
            var registry = new GameSystemRegistry();
            registry.Register(Cthulhu.Instance);

            var results = registry.SearchByName("Cthulhu");

            Assert.Single(results);
        }

        [Fact]
        public void Contains_ExistingId_ReturnsTrue()
        {
            var registry = new GameSystemRegistry();
            registry.Register(DiceBot.Instance);

            Assert.True(registry.Contains("DiceBot"));
        }

        [Fact]
        public void Contains_NonExistingId_ReturnsFalse()
        {
            var registry = new GameSystemRegistry();
            registry.Register(DiceBot.Instance);

            Assert.False(registry.Contains("Unknown"));
        }

        [Fact]
        public void Systems_ReturnsSortedList()
        {
            var registry = new GameSystemRegistry();
            registry.Register(Cthulhu.Instance);  // SortKey: "くとうるふ"
            registry.Register(DiceBot.Instance);  // SortKey: "*DiceBot"

            var systems = registry.Systems;

            Assert.Equal(2, systems.Count);
            Assert.Equal("DiceBot", systems[0].Id);  // "*" comes first
            Assert.Equal("Cthulhu", systems[1].Id);
        }

        [Fact]
        public void Clear_RemovesAllSystems()
        {
            var registry = new GameSystemRegistry();
            registry.Register(DiceBot.Instance);
            registry.Register(Cthulhu.Instance);

            registry.Clear();

            Assert.Equal(0, registry.Count);
            Assert.Empty(registry.Systems);
        }

        [Fact]
        public void RegisterAll_AddsMultipleSystems()
        {
            var registry = new GameSystemRegistry();
            registry.RegisterAll(new IGameSystem[] { DiceBot.Instance, Cthulhu.Instance });

            Assert.Equal(2, registry.Count);
        }
    }
}
