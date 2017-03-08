using System.Diagnostics;
using System.Reflection;
using ASCompletion.Completion;
using ScintillaNet;

namespace ForForeachConverter.Helpers
{
    internal class Reflector
    {
        internal static readonly ASGeneratorReflector ASGenerator = new ASGeneratorReflector();
    }

    internal class ASGeneratorReflector
    {
        internal int GetEndOfStatement(int startPos, int endPos, ScintillaControl sci)
        {
            var methodInfo = typeof(ASGenerator).GetMethod("GetEndOfStatement", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(methodInfo != null, "methodInfo != null");
            return (int) methodInfo.Invoke(null, new object[] {startPos, endPos, sci});
        }
    }
}
