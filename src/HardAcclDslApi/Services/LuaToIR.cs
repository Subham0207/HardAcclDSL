using System;
using System.Collections.Generic;

namespace HardAcclDslApi.Services.LuaToIR
{
    /// <summary>
    /// Converts Lua code to an Intermediate Representation (IR).
    /// </summary>
    public class LuaToIR
    {
        public LuaToIR()
        {
        }

        /// <summary>
        /// Converts Lua source code to IR format.
        /// </summary>
        /// <param name="LuaCode">The Lua source code to convert.</param>
        /// <returns>The intermediate representation.</returns>
        public string Convert(string LuaCode)
        {
            if (string.IsNullOrEmpty(LuaCode))
            {
                throw new ArgumentException("Lua code cannot be null or empty.", nameof(LuaCode));
            }

            

            return string.Empty;
        }
    }
}