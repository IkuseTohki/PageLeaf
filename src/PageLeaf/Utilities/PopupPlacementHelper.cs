using System;
using System.Windows;

namespace PageLeaf.Utilities
{
    public static class PopupPlacementHelper
    {
        /// <summary>
        /// ポップアップの表示位置を計算し、ウィンドウ内に収まるように調整します。
        /// </summary>
        /// <param name="targetRect">ターゲット（カーソル）の矩形（ウィンドウ基準）</param>
        /// <param name="popupSize">ポップアップのサイズ</param>
        /// <param name="windowSize">ウィンドウのサイズ</param>
        /// <param name="padding">ウィンドウ端からの余白</param>
        /// <returns>調整後の表示位置（ウィンドウ基準）</returns>
        public static Point CalculatePopupPosition(Rect targetRect, Size popupSize, Size windowSize, double padding = 20.0)
        {
            double x = targetRect.Left;
            double y = targetRect.Bottom;

            // 右端のガード
            if (x + popupSize.Width > windowSize.Width)
            {
                double overflow = (x + popupSize.Width) - windowSize.Width + padding;
                x -= overflow;
            }

            // 下端のガード (下に入り切らない場合は上に表示)
            if (y + popupSize.Height > windowSize.Height)
            {
                // ターゲットの上端 - ポップアップの高さ
                y = targetRect.Top - popupSize.Height;
            }

            // 画面左上（0,0）からはみ出さないようにする（最終ガード）
            x = Math.Max(0, x);
            y = Math.Max(0, y);

            return new Point(x, y);
        }
    }
}
