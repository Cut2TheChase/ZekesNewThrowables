using System;
using System.Collections.Generic;
using Necro;
using HBS.Text;
using HBS.Collections;

namespace ZekesNewThrowables
{
    public static class Utils
    {
        public static Func<Dictionary<string, Action<TextFieldParser, EffectDef>>> GetMethodParsers;

        public static Func<TextFieldParser, string, bool,TagWeights> ParseTagWeights;

        public static Func<TextFieldParser, string, string, float> TryParseFloat;
    }
}
