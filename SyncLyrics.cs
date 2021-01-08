﻿using Amaoto;
using Koioto.Support;
using Koioto.Support.FileReader;
using Koioto.Support.Log;
using Space.AioiLight.LRCDotNet;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Koioto.SamplePlugin.SyncLyrics
{
    public class SyncLyrics : Koioto.Plugin.Overlay
    {
        public override string Name => "SyncLyrics";
        public override string[] Creator => new string[] { "AioiLight" };
        public override string Version => "2.0";
        public override string Description => "Show sync lyrics (*.lrc) at playing screen.";

        public override void OnEnable()
        {
            Theme = Util.ThemeInit<Theme>(Path.Combine(Bridge.PluginDir, @"SyncLyrics.json"));
            LyricFont = Util.GetFontRenderFromTheme(Theme.LyricFont);
            base.OnEnable();
        }

        public override void OnDisable()
        {
            Theme = null;
            Lyric = null;
            LyricFont = null;
            LyricAndTimings = null;
            base.OnDisable();
        }
        public override void OnSelectedSong(Playable[] playable, ChartInfo chartInfo, PlayLog[] playLogs)
        {
            // Use player1's Playable
            var p = playable[0];

            Lyric = null;
            Lyric = new LyRiCs[p.Sections.Length];

            // *.lrc parse phase
            for (var section = 0; section < Lyric.Length; section++)
            {
                // get path for file
                var audioPath = chartInfo.Audio[section];

                if (audioPath == null)
                {
                    continue;
                }

                var folder = Path.GetDirectoryName(audioPath);
                var lrcFile = $"{Path.GetFileNameWithoutExtension(audioPath)}.lrc";

                var lrcPath = Path.Combine(folder, lrcFile);

                // read and parse
                if (!File.Exists(lrcPath))
                {
                    continue;
                }

                var file = File.ReadAllText(lrcPath);

                var result = LRCDotNet.Parse(file);

                Lyric[section] = result;
            }
            // phase end

            // Generation texture phase
            LyricAndTimings = new LyricAndTiming[Lyric.Length][];
            for (var section = 0; section < Lyric.Length; section++)
            {
                if (Lyric[section] == null)
                {
                    continue;
                }

                var lyric = Lyric[section].Lyrics;
                LyricAndTimings[section] = new LyricAndTiming[lyric.Count()];

                for (var l = 0; l < lyric.Count(); l++)
                {
                    // convert ms to us
                    var timing = (long)(lyric[l].Time.TotalMilliseconds * 1000.0);
                    // apply offset
                    if (Theme.Offset != 0)
                    {
                        timing += Theme.Offset;
                    }
                    var tex = LyricFont.GetTexture(lyric[l].Text);
                    LyricAndTimings[section][l] = new LyricAndTiming(tex, timing);
                }
            }
            // phase end

            Showing = null;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnDraw()
        {
            if (Showing != null)
            {
                // Apply theme
                Util.SetThemeToTexture(Showing, Theme.Lyric);
                Showing.ScaleX *= Util.GetProperScaleX(Showing, Theme.MaximumWidth);
                Showing.Draw(Theme.Lyric.X, Theme.Lyric.Y);
            }
        }

        public override void OnPlayer(long sectionTickValue)
        {
            if (LyricAndTimings[SectionIndex] == null)
            {
                return;
            }

            // calc accurate time from lag
            var accuTime = sectionTickValue - Lag;

            if (sectionTickValue < Counter)
            {
                // pick last lyric for measure moving
                var last = LyricAndTimings[SectionIndex].Where(t => t.Timing <= accuTime).LastOrDefault();

                Showing = last?.Tex;
                LyricIndex = LyricAndTimings[SectionIndex].ToList().IndexOf(last);

                Counter = sectionTickValue;
                return;
            }

            if (LyricAndTimings[SectionIndex].Length <= LyricIndex + 1)
            {
                return;
            }

            if (!ShowedFirstLyric)
            {
                if (accuTime >= LyricAndTimings[SectionIndex][0].Timing)
                {
                    Showing = LyricAndTimings[SectionIndex][0].Tex;
                    ShowedFirstLyric = true;
                    return;
                }
            }

            var nextLyric = LyricAndTimings[SectionIndex][LyricIndex + 1];

            if (accuTime >= nextLyric.Timing)
            {
                // set texture
                LyricIndex++;
                Showing = nextLyric.Tex;
            }

            Counter = sectionTickValue;
        }

        public override void OnChangedSection(int sectionIndex, int player, List<Chip> section)
        {
            // reset some vars
            if (player == 0)
            {
                SectionIndex = sectionIndex;
                LyricIndex = 0;
                Showing = null;
                ShowedFirstLyric = false;

                // get time until starts bgm
                Lag = section.First(c => c.ChipType == Chips.BGMStart).Time;
            }
        }

        private LyRiCs[] Lyric;
        private FontRender LyricFont;
        private int SectionIndex;
        private int LyricIndex;
        private Texture Showing;
        private LyricAndTiming[][] LyricAndTimings;
        private bool ShowedFirstLyric;

        private Theme Theme;

        private long Lag;
        private long Counter;
    }

    internal class LyricAndTiming
    {
        internal LyricAndTiming(Texture tex, long timing)
        {
            Tex = tex;
            Timing = timing;
        }

        internal Texture Tex { get; private set; }
        internal long Timing { get; private set; }
    }
}
