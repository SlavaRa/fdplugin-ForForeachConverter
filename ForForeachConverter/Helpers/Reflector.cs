// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Diagnostics;
using System.Reflection;
using System.Text;
using ASCompletion.Completion;
using ScintillaNet;

namespace ForForeachConverter.Helpers
{
    internal class Reflector
    {
        internal static readonly ASGeneratorReflector ASGenerator = new ASGeneratorReflector();
        internal static readonly SnippetManagerReflector SnippetManager = new SnippetManagerReflector();
    }

    internal class ASGeneratorReflector
    {
        internal string CleanType(string type)
        {
            var methodInfo = typeof(ASGenerator).GetMethod("CleanType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(methodInfo != null, "methodInfo is null");
            return (string)methodInfo.Invoke(null, new object[] { type });
        }

        internal int GetEndOfStatement(int startPos, int endPos, ScintillaControl sci)
        {
            var methodInfo = typeof(ASGenerator).GetMethod("GetEndOfStatement", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(methodInfo != null, "methodInfo is null");
            return (int) methodInfo.Invoke(null, new object[] {startPos, endPos, sci});
        }
    }

    internal class SnippetManagerReflector
    {
        /// <summary>
        /// Gets a snippet from a file in the snippets directory
        /// </summary>
        internal string GetSnippet(string word, string syntax, Encoding current)
        {
            var type = Assembly.GetEntryAssembly().GetType("FlashDevelop.Managers.SnippetManager");
            Debug.Assert(type != null, "type is null");
            var methodInfo = type.GetMethod("GetSnippet", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            Debug.Assert(methodInfo != null, "methodInfo is null");
            return (string) methodInfo.Invoke(null, new object[] {word, syntax, current});
        }
    }
}
