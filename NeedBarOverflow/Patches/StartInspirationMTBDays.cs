using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NeedBarOverflow.Patches
{
    using static Common;
    public static class StartInspirationMTBDays
    {
        public static readonly MethodInfo Transpiler = ((Func_Transpiler)TranspilerMethod).Method;
        public static IEnumerable<CodeInstruction> TranspilerMethod(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                yield return codeInstruction;
                if (codeInstruction.Calls(get_CurLevel))
                {
                    state++;
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
                    yield return new CodeInstruction(OpCodes.Call, m_Min);
                }

            }
            Debug.CheckTranspiler(state, state > 0);
        }
    }
}
