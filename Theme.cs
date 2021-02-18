using Koioto.Support.Theme;

namespace Koioto.SamplePlugin.SyncLyrics
{
    /// <summary>
    /// SyncLyrics's theme configuration.
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// Offset of lyric's appear timing.
        /// Smaller is faster.
        /// Default: 100ms
        /// </summary>
        public int Offset { get; set; } = -100 * 1000;

        /// <summary>
        /// Position, scaling, opacity, and alignment setting.
        /// </summary>
        public NormalTexture Lyric { get; set; }
            = new NormalTexture()
            {
                X = 960,
                Y = 1080,
                ScaleX = 1,
                ScaleY = 1,
                Opacity = 1,
                ReferencePoint = Amaoto.ReferencePoint.BottomCenter
            };

        /// <summary>
        /// Font setting.
        /// </summary>
        public StringTexture LyricFont { get; set; }
            = new StringTexture()
            {
                Font = new Font()
                {
                    FontSize = 48,
                    EdgeSize = 8
                },
                ForeColor = new Color()
                {
                    Red = 255,
                    Green = 255,
                    Blue = 255,
                },
                BackColor = new Color()
                {
                    Red = 0,
                    Green = 0,
                    Blue = 0
                }
            };

        /// <summary>
        /// Maximum width.
        /// </summary>
        public int MaximumWidth { get; set; } = 1520;
    }
}
