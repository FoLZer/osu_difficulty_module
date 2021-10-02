const edge = require("edge-js");

const Calc = edge.func({
    assemblyFile: 'osu_Server_DifficultyCalculator.dll',
    typeName: 'osu.Server.DifficultyCalculator',
    methodName: 'Calc'
});

module.exports = {Calc};