using System;
using System.Collections.Generic;
using System.Linq;
using static BeatmapEditor3D.DataModels.BeatmapsCollectionDataModel;

namespace EditorEX.Util
{
    //TODO: Make this a better algorithm LOL.
    //Points are decided arbitrarily, inspired by the BetterSongSearch plugin
    internal class BeatmapFilterUtil
    {
        public static List<BeatmapInfoData> Filter(List<BeatmapInfoData> beatmapInfos, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return beatmapInfos;

            var terms = text.Split(' ');
            var beatmapSortInfos = new List<BeatmapSortInfo>();

            foreach (var beatmapInfo in beatmapInfos)
            {
                var points = 0;

                for (var i = 0; i < terms.Length; i++)
                {
                    var term = terms[i];
                    if (!string.IsNullOrWhiteSpace(term))
                    {
                        if (beatmapInfo.songSubName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                            points += 1;

                        if (beatmapInfo.songAuthorName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                            points += 3;

                        if (beatmapInfo.levelAuthorName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                            points += 4;

                        if (beatmapInfo.songName.IndexOf(term, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                            points += 5;
                    }
                }

                if (points > 1)
                    beatmapSortInfos.Add(new(points, beatmapInfo));
            }

            beatmapSortInfos.Sort((x, y) => y.Points.CompareTo(x.Points));

            return beatmapSortInfos.Select((info) => info.BeatmapInfoData).ToList();
        }

        private struct BeatmapSortInfo
        {
            public readonly int Points;
            public readonly BeatmapInfoData BeatmapInfoData;

            public BeatmapSortInfo(int points, BeatmapInfoData beatmapInfoData)
            {
                Points = points;
                BeatmapInfoData = beatmapInfoData;
            }
        }
    }
}
