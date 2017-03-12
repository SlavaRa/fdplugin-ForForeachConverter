// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ScintillaNet;

namespace ForForeachConverter.Commands.Haxe
{
    class ConvertForeachToForinCommand : ConvertForeachToForCommand
    {
        public new static bool IsValidForConvert(ScintillaControl sci) => false;
        public override bool IsValid() => false;
    }
}
