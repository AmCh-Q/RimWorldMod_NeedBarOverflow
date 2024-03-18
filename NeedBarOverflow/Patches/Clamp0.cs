using System.Reflection;
using UnityEngine;

namespace NeedBarOverflow.Patches
{
    // Some methods give unwanted negative results, we add postfix to adjust them to zero
    using static Common;
    public static class Clamp0
    {
        public static readonly MethodInfo Postfix = ((ActionRef<float>)PostfixMethod).Method;
        private static void PostfixMethod(ref float __result) => __result = Mathf.Max(__result, 0f);
    }
}
