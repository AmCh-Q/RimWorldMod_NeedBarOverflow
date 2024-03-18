using System.Reflection;
using UnityEngine;

namespace NeedBarOverflow.Patches
{
    using static Common;
    public static class GenUI
    {
        public static readonly MethodInfo Prefix = ((ActionRef<float>)PrefixMethod).Method;
        // Limit pct before drawing UI to avoid overflowing UI visuals
        private static void PrefixMethod(ref float pct) => pct = Mathf.Min(pct, 1f);
    }
}
