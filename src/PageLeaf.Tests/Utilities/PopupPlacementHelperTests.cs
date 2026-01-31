using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Utilities;
using System.Windows;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class PopupPlacementHelperTests
    {
        [TestMethod]
        public void CalculatePopupPosition_ShouldKeepWithinWindow_WhenRightOverflow()
        {
            // テスト観点: 右端からはみ出す場合、左にシフトされることを確認する
            // Arrange
            var windowSize = new Size(1000, 600);
            var targetRect = new Rect(800, 100, 10, 20); // X=800, Width=10
            var popupSize = new Size(300, 50); // 右端(800+300=1100)が1000を超える

            // Act
            var result = PopupPlacementHelper.CalculatePopupPosition(targetRect, popupSize, windowSize);

            // Assert
            // 800 + 300 - 1000 + 20 = 120 (オーバーフロー分 + パディング)
            // 800 - 120 = 680
            Assert.AreEqual(680, result.X);
            Assert.IsTrue(result.X + popupSize.Width <= windowSize.Width);
        }

        [TestMethod]
        public void CalculatePopupPosition_ShouldFlipUp_WhenBottomOverflow()
        {
            // テスト観点: 下端からはみ出す場合、ターゲットの上に表示されることを確認する
            // Arrange
            var windowSize = new Size(1000, 600);
            var targetRect = new Rect(100, 550, 10, 20); // Bottom=570
            var popupSize = new Size(300, 50); // 下端(570+50=620)が600を超える

            // Act
            var result = PopupPlacementHelper.CalculatePopupPosition(targetRect, popupSize, windowSize);

            // Assert
            // Yは targetRect.Top(550) - popupSize.Height(50) = 500 になるべき
            Assert.AreEqual(500, result.Y);
        }

        [TestMethod]
        public void CalculatePopupPosition_ShouldNotBeNegative()
        {
            // テスト観点: 調整によって座標が負（画面外）にならないことを確認する
            // Arrange
            var windowSize = new Size(1000, 600);
            var targetRect = new Rect(10, 10, 10, 20);
            var popupSize = new Size(1100, 700); // ウィンドウより大きい

            // Act
            var result = PopupPlacementHelper.CalculatePopupPosition(targetRect, popupSize, windowSize);

            // Assert
            Assert.AreEqual(0, result.X);
            Assert.AreEqual(0, result.Y);
        }
    }
}
