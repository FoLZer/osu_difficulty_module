process.env.EDGE_USE_CORECLR=1
const edge = require("edge-js");

const Calc = edge.func({
    assemblyFile: 'osu_Server_DifficultyCalculator.dll',
    typeName: 'osu.Server.DifficultyCalculator.Startup',
    methodName: 'Invoke'
});

module.exports = {Calc};