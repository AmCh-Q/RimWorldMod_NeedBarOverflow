using UnityEngine;

namespace NeedBarOverflow.Patches
{
	// Some methods give unwanted negative results, we add postfix to adjust them to zero
	using static Utility;
	public static class Add0LowerBound
	{
		public static readonly ActionRef<float> 
			postfix = Postfix;
		private static void Postfix(ref float __result) 
			=> __result = Mathf.Max(__result, 0f);
	}
}
