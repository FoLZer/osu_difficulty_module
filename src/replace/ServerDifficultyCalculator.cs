// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper;
using JetBrains.Annotations;
using MySqlConnector;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;

namespace osu.Server.DifficultyCalculator
{
    public class ServerDifficultyCalculator
    {
        private static readonly List<Ruleset> available_rulesets = getRulesets();

        private readonly bool processConverts;
        private readonly bool dryRun;
        private readonly Ruleset processableRuleset;

        public ServerDifficultyCalculator(int rulesetId, bool processConverts = true, bool dryRun = false)
        {
            this.processConverts = processConverts;
            this.dryRun = dryRun;

            processableRuleset = available_rulesets.Single(r => r.RulesetInfo.ID == rulesetId);
        }

        public List<List<attr>> ProcessBeatmap(WorkingBeatmap beatmap)
        {
            Debug.Assert(beatmap.BeatmapInfo.OnlineBeatmapID != null, "beatmap.BeatmapInfo.OnlineBeatmapID != null");

            int beatmapId = beatmap.BeatmapInfo.OnlineBeatmapID.Value;

            try
            {
                if (beatmap.Beatmap.HitObjects.Count == 0)
                {
                    using (var conn = Database.GetSlaveConnection())
                    {
                        if (conn?.QuerySingleOrDefault<int>("SELECT `approved` FROM `osu_beatmaps` WHERE `beatmap_id` = @BeatmapId", new { BeatmapId = beatmapId }) > 0)
                            throw new ArgumentException($"Ranked beatmap {beatmapId} has 0 hitobjects!");
                    }
                }
                return computeDifficulty(beatmapId, beatmap, processableRuleset);
            }
            catch (Exception e)
            {
                throw new Exception($"{beatmapId} failed with: {e.Message}");
            }
        }

        public class attr
        {
            public int BeatmapId;
            public int Mode;
            public int Mods;
            public int Attribute;
            public float Value;

            public attr(int beatmapId, int iD, int legacyMod, int id, float v)
            {
                BeatmapId = beatmapId;
                Mode = iD;
                Mods = legacyMod;
                Attribute = id;
                Value = v;
            }
        }

        private List<List<attr>> computeDifficulty(int beatmapId, WorkingBeatmap beatmap, Ruleset ruleset)
        {
            var parameters = new List<List<attr>>();
            foreach (var attribute in ruleset.CreateDifficultyCalculator(beatmap).CalculateAll())
            {
                if (dryRun)
                    continue;

                var legacyMod = attribute.Mods.ToLegacy();

                var attrs = new List<attr>();
                bool hasDifVal = false;
                bool hasComboVal = false;
                foreach (var (id, value) in attribute.Map())
                {
                    attrs.Add(new attr(beatmapId, (int)ruleset.RulesetInfo.ID, (int)legacyMod, id, Convert.ToSingle(value)));
                    if(id == 9)
                    {
                        hasComboVal = true;
                    } else if(id == 11)
                    {
                        hasDifVal = true;
                    }
                }

                if(!hasDifVal)
                {
                    attrs.Add(new attr(beatmapId, (int)ruleset.RulesetInfo.ID, (int)legacyMod, 11, Convert.ToSingle(attribute.StarRating)));
                }
                if (!hasComboVal)
                {
                    attrs.Add(new attr(beatmapId, (int)ruleset.RulesetInfo.ID, (int)legacyMod, 9, Convert.ToSingle(attribute.MaxCombo)));
                }

                parameters.Add(attrs);
            }
            return parameters;
        }

        private static List<Ruleset> getRulesets()
        {
            const string ruleset_library_prefix = "osu.Game.Rulesets";

            var rulesetsToProcess = new List<Ruleset>();

            foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, $"{ruleset_library_prefix}.*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    Type type = assembly.GetTypes().First(t => t.IsPublic && t.IsSubclassOf(typeof(Ruleset)));
                    rulesetsToProcess.Add((Ruleset)Activator.CreateInstance(type));
                }
                catch
                {
                    throw new Exception($"Failed to load ruleset ({file})");
                }
            }

            return rulesetsToProcess;
        }
    }
}
