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
                if (beatmapInfo == null)
                    continue;

                float points = 0;

                for (var i = 0; i < terms.Length; i++)
                {
                    var term = terms[i];
                    var termMultiplier = 1f;
                    termMultiplier = (term.Length / 5f) + 1f;
                    if (!string.IsNullOrWhiteSpace(term))
                    {
                        float pointsToGo = 0f;
                        if (
                            beatmapInfo.songSubName != null
                            && beatmapInfo.songSubName.IndexOf(
                                term,
                                0,
                                StringComparison.CurrentCultureIgnoreCase
                            ) != -1
                        )
                            pointsToGo += 4f;

                        if (
                            beatmapInfo.songAuthorName != null
                            && beatmapInfo.songAuthorName.IndexOf(
                                term,
                                0,
                                StringComparison.CurrentCultureIgnoreCase
                            ) != -1
                        )
                            pointsToGo += 12f;

                        if (
                            beatmapInfo.levelAuthorName != null
                            && beatmapInfo.levelAuthorName.IndexOf(
                                term,
                                0,
                                StringComparison.CurrentCultureIgnoreCase
                            ) != -1
                        )
                            pointsToGo += 16f;

                        if (
                            beatmapInfo.songName != null
                            && beatmapInfo.songName.IndexOf(
                                term,
                                0,
                                StringComparison.CurrentCultureIgnoreCase
                            ) != -1
                        )
                            pointsToGo += 20f;

                        if (
                            beatmapInfo.songSubName?.Equals(
                                term,
                                StringComparison.CurrentCultureIgnoreCase
                            ) ?? false
                        )
                            pointsToGo *= 3f;

                        if (
                            beatmapInfo.songAuthorName?.Equals(
                                term,
                                StringComparison.CurrentCultureIgnoreCase
                            ) ?? false
                        )
                            pointsToGo *= 3f;

                        if (
                            beatmapInfo.levelAuthorName?.Equals(
                                term,
                                StringComparison.CurrentCultureIgnoreCase
                            ) ?? false
                        )
                            pointsToGo *= 3f;

                        if (
                            beatmapInfo.songName?.Equals(
                                term,
                                StringComparison.CurrentCultureIgnoreCase
                            ) ?? false
                        )
                            pointsToGo *= 3f;

                        pointsToGo *= termMultiplier;

                        points += pointsToGo;
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
            public readonly float Points;
            public readonly BeatmapInfoData BeatmapInfoData;

            public BeatmapSortInfo(float points, BeatmapInfoData beatmapInfoData)
            {
                Points = points;
                BeatmapInfoData = beatmapInfoData;
            }
        }
    }
}
