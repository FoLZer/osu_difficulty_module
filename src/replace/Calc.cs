using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static osu.Server.DifficultyCalculator.ServerDifficultyCalculator;
using osu.Game.Beatmaps.Formats;

namespace osu.Server.DifficultyCalculator {
	[ComVisible(true)]
	public class CalcClass
    {
        public async Task<object> Calc(object _rulesetID, object _beatmapID) {
			int rulesetID = (int) _rulesetID;
			int beatmapID = (int) _beatmapID;
			LegacyDifficultyCalculatorBeatmapDecoder.Register();
			var calc = new ServerDifficultyCalculator(rulesetID);
			var beatmap = BeatmapLoader.GetBeatmap(beatmapID, true, false);
			beatmap.BeatmapInfo.OnlineBeatmapID = beatmapID;
			return calc.ProcessBeatmap(beatmap);
		}
		static public void Main(){}
	}
}