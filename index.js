const edge = require("edge-js");

const Calc = edge.func({
    assemblyFile: 'osu_Server_DifficultyCalculator.dll'
});

module.exports = {Calc};