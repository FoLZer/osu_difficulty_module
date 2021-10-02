const findVisualStudio = require("./find-visualstudio.js");
const child_process = require("child_process");
const path = require("path");
const fs = require("fs");

findVisualStudio((err, vsInfo) => {
	if(err) {
		throw err;
	}
	console.log("Building default osu-difficulty-calculator");
	fs.rmSync(path.resolve(".","src","osu-difficulty-calculator","osu.Server.DifficultyCalculator","ServerDifficultyCalculator.cs"),{force:true});
	fs.rmSync(path.resolve(".","src","osu-difficulty-calculator","osu.Server.DifficultyCalculator","Program.cs"),{force:true});
	fs.rmSync(path.resolve(".","src","osu-difficulty-calculator","osu.Server.DifficultyCalculator","Commands"),{force:true,recursive:true});
	fs.rmSync(path.resolve(".","src","osu-difficulty-calculator","osu.Server.Queues.BeatmapProcessor","BeatmapProcessor.cs"),{force:true});
	fs.copyFileSync(path.resolve(".","src","replace","Calc.cs"),path.resolve(".","src","osu-difficulty-calculator","osu.Server.DifficultyCalculator","Calc.cs"));
	fs.copyFileSync(path.resolve(".","src","replace","ServerDifficultyCalculator.cs"),path.resolve(".","src","osu-difficulty-calculator","osu.Server.DifficultyCalculator","ServerDifficultyCalculator.cs"));
	fs.copyFileSync(path.resolve(".","src","replace","BeatmapProcessor.cs"),path.resolve(".","src","osu-difficulty-calculator","osu.Server.Queues.BeatmapProcessor","BeatmapProcessor.cs"));
	const sp = child_process.execSync(vsInfo.msBuild+" -restore -p:Configuration=Debug /t:Compile /clp:Verbosity=minimal osu.Server.sln", {cwd:path.resolve(".","src","osu-difficulty-calculator"),stdio:"inherit"});
	fs.rmSync(path.resolve(".","osu_Server_DifficultyCalculator.dll"),{force:true});
	fs.copyFileSync(path.resolve(".","src","osu-difficulty-calculator","osu.Server.DifficultyCalculator","bin","Debug","net5.0","osu.Server.DifficultyCalculator.dll"),path.resolve(".","osu_Server_DifficultyCalculator.dll"));
	console.log("Built successfully!");
})