using HBS.DebugConsole;
using HBS.Scripting.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZekesNewThrowables
{
    [ScriptBinding("")]
    public class TestConsoleCommand
    {
        [ScriptBinding]
        public static void ZekesSecretCommand()
        {
            DebugConsole.Log("Hey I work <3");
        }

    }
}
