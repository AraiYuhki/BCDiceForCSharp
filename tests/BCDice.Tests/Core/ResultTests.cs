using BCDice.Core;
using Xunit;

namespace BCDice.Tests.Core
{
    public class ResultTests
    {
        [Fact]
        public void Success_SetsCorrectFlags()
        {
            var result = Result.Success("成功");

            Assert.Equal("成功", result.Text);
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.False(result.IsCritical);
            Assert.False(result.IsFumble);
            Assert.False(result.IsSecret);
        }

        [Fact]
        public void Failure_SetsCorrectFlags()
        {
            var result = Result.Failure("失敗");

            Assert.Equal("失敗", result.Text);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.False(result.IsCritical);
            Assert.False(result.IsFumble);
            Assert.False(result.IsSecret);
        }

        [Fact]
        public void Critical_SetsSuccessAndCritical()
        {
            var result = Result.Critical("クリティカル！");

            Assert.Equal("クリティカル！", result.Text);
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.True(result.IsCritical);
            Assert.False(result.IsFumble);
        }

        [Fact]
        public void Fumble_SetsFailureAndFumble()
        {
            var result = Result.Fumble("ファンブル！");

            Assert.Equal("ファンブル！", result.Text);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.False(result.IsCritical);
            Assert.True(result.IsFumble);
        }

        [Fact]
        public void Builder_CanSetAllProperties()
        {
            var rands = new[] { (3, 6), (5, 6) };
            var detailedRands = new[]
            {
                new DetailedRandResult(RandResultKind.Normal, 6, 3),
                new DetailedRandResult(RandResultKind.Normal, 6, 5)
            };

            var result = Result.CreateBuilder("テスト結果")
                .SetSecret(true)
                .SetSuccess(true)
                .SetCritical(true)
                .SetRands(rands)
                .SetDetailedRands(detailedRands)
                .Build();

            Assert.Equal("テスト結果", result.Text);
            Assert.True(result.IsSecret);
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.True(result.IsCritical);
            Assert.False(result.IsFumble);
            Assert.Equal(rands, result.Rands);
            Assert.Equal(detailedRands, result.DetailedRands);
        }

        [Fact]
        public void Builder_SetCondition_True_SetsSuccess()
        {
            var result = Result.CreateBuilder("条件テスト")
                .SetCondition(true)
                .Build();

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
        }

        [Fact]
        public void Builder_SetCondition_False_SetsFailure()
        {
            var result = Result.CreateBuilder("条件テスト")
                .SetCondition(false)
                .Build();

            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
        }
    }
}
