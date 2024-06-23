using System;
using UnityEngine;

namespace NeedBarOverflow.Patches
{
	// Some methods give unwanted negative results, we add d_postfix to m_adjust them to zero

	public static class Add0LowerBound
	{
		public static readonly Delegate
			d_postfix = PostfixMethod;
		public static void PostfixMethod(ref float __result)
			=> __result = Mathf.Max(__result, 0f);
	}
}
