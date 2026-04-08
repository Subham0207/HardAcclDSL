using LuaNET.Lua54;
using static LuaNET.Lua54.Lua;
using System.Text.RegularExpressions;

namespace HardAcclDslApi.Services;

public sealed class LuaExecutionService
{
    private static readonly Regex GlobalNameRegex = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    // Capture Lua print(...) output in-memory so the API can return it as structured response data.
        private const string InitializeOutputCaptureScript = @"
__dotnet_outputs = {}
_G.print = function(...)
    local count = select('#', ...)
    local parts = {}
    for i = 1, count do
        parts[#parts + 1] = tostring(select(i, ...))
    end
    __dotnet_outputs[#__dotnet_outputs + 1] = table.concat(parts, '\t')
end
";

        private const string ReadOutputCaptureScript = @"
if __dotnet_outputs == nil then
    return ''
end
return table.concat(__dotnet_outputs, '\n')
";

    public LuaExecutionResult Execute(string luaCode, IReadOnlyDictionary<string, double>? numberGlobals = null)
    {
        if (string.IsNullOrWhiteSpace(luaCode))
        {
            return LuaExecutionResult.Failure("Lua code is required.");
        }

        lua_State state = luaL_newstate();
        if (state == 0)
        {
            return LuaExecutionResult.Failure("Unable to create Lua runtime state.");
        }

        try
        {
            luaL_openlibs(state);

            var initializeStatus = ExecuteChunk(state, InitializeOutputCaptureScript);
            if (initializeStatus != LUA_OK)
            {
                var initializeError = ReadErrorMessage(state);
                return LuaExecutionResult.Failure($"Lua init error: {initializeError}");
            }

            if (numberGlobals is not null)
            {
                var globalsValidationError = TryInjectNumberGlobals(state, numberGlobals);
                if (globalsValidationError is not null)
                {
                    return LuaExecutionResult.Failure(globalsValidationError);
                }
            }

            var loadStatus = luaL_loadstring(state, luaCode);
            if (loadStatus != LUA_OK)
            {
                var loadError = ReadErrorMessage(state);
                return LuaExecutionResult.Failure($"Lua load error: {loadError}");
            }

            var execStatus = lua_pcallk(state, 0, LUA_MULTRET, 0, null, null);
            if (execStatus != LUA_OK)
            {
                var runtimeError = ReadErrorMessage(state);
                var printedLinesOnFailure = ReadCapturedOutput(state);
                return LuaExecutionResult.Failure($"Lua runtime error: {runtimeError}", printedLinesOnFailure);
            }

            var returnCount = lua_gettop(state);
            var values = new List<string>(capacity: returnCount);
            for (var i = 1; i <= returnCount; i++)
            {
                values.Add(ReadValueAsString(state, i));
            }

            var printedLines = ReadCapturedOutput(state);

            return LuaExecutionResult.FromSuccess(values, printedLines);
        }
        catch (Exception ex)
        {
            return LuaExecutionResult.Failure($"Lua execution failed: {ex.Message}");
        }
        finally
        {
            lua_close(state);
        }
    }

    private static string? TryInjectNumberGlobals(lua_State state, IReadOnlyDictionary<string, double> numberGlobals)
    {
        foreach (var entry in numberGlobals)
        {
            if (string.IsNullOrWhiteSpace(entry.Key) || !GlobalNameRegex.IsMatch(entry.Key))
            {
                return $"Invalid global variable name '{entry.Key}'. Use Lua identifier format [A-Za-z_][A-Za-z0-9_]*.";
            }

            if (double.IsNaN(entry.Value) || double.IsInfinity(entry.Value))
            {
                return $"Global variable '{entry.Key}' must be a finite number.";
            }

            lua_pushnumber(state, entry.Value);
            lua_setglobal(state, entry.Key);
        }

        return null;
    }

    private static string ReadErrorMessage(lua_State state)
    {
        var message = luaL_checkstring(state, -1) ?? "Unknown Lua error.";
        lua_pop(state, 1);
        return message;
    }

    private static string ReadValueAsString(lua_State state, int stackIndex)
    {
        var valueType = lua_type(state, stackIndex);
        return valueType switch
        {
            LUA_TNIL => "nil",
            LUA_TBOOLEAN => lua_toboolean(state, stackIndex) != 0 ? "true" : "false",
            LUA_TNUMBER => ReadNumber(state, stackIndex),
            LUA_TSTRING => luaL_checkstring(state, stackIndex) ?? string.Empty,
            _ => lua_typename(state, valueType) ?? "unknown"
        };
    }

    private static string ReadNumber(lua_State state, int stackIndex)
    {
        var isNumber = 0;
        var value = lua_tonumberx(state, stackIndex, ref isNumber);
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static int ExecuteChunk(lua_State state, string chunk)
    {
        var loadStatus = luaL_loadstring(state, chunk);
        if (loadStatus != LUA_OK)
        {
            return loadStatus;
        }

        return lua_pcallk(state, 0, LUA_MULTRET, 0, null, null);
    }

    private static IReadOnlyList<string> ReadCapturedOutput(lua_State state)
    {
        lua_settop(state, 0);

        var readOutputStatus = ExecuteChunk(state, ReadOutputCaptureScript);
        if (readOutputStatus != LUA_OK)
        {
            var outputError = ReadErrorMessage(state);
            return new[] { $"[output capture error] {outputError}" };
        }

        var output = luaL_checkstring(state, -1) ?? string.Empty;
        lua_pop(state, 1);

        if (string.IsNullOrEmpty(output))
        {
            return Array.Empty<string>();
        }

        return output.Split('\n', StringSplitOptions.None);
    }
}

public sealed class LuaExecutionResult
{
    public bool Success { get; init; }
    public string Error { get; init; } = string.Empty;
    public IReadOnlyList<string> ReturnValues { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> PrintedLines { get; init; } = Array.Empty<string>();

    public static LuaExecutionResult FromSuccess(IReadOnlyList<string> returnValues, IReadOnlyList<string> printedLines)
    {
        return new LuaExecutionResult
        {
            Success = true,
            ReturnValues = returnValues,
            PrintedLines = printedLines,
        };
    }

    public static LuaExecutionResult Failure(string error, IReadOnlyList<string>? printedLines = null)
    {
        return new LuaExecutionResult
        {
            Success = false,
            Error = error,
            PrintedLines = printedLines ?? Array.Empty<string>(),
        };
    }
}
