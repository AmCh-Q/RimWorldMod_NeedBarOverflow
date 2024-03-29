using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace NeedBarOverflow.Patches
{
	using static Utility;
	public static class RemoveLastMin
	{
		public static readonly
			TransIL transpiler = Transpiler;
		public static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			int lastIdxBeforeLdc = -1;
			for (int i = instructionList.Count - 2; i > 0; i--)
			{
				if (instructionList[i].LoadsConstant(1.0) &&
					instructionList[i + 1].Calls(m_Min))
				{
					state = 1;
					lastIdxBeforeLdc = i - 1;
					break;
				}
			}
			for (int i = 0; i < instructionList.Count; i++)
			{
				yield return instructionList[i];
				if (i == lastIdxBeforeLdc)
				{
					state = 2;
					i += 2;
				}
			}
			Debug.CheckTranspiler(state, 2);
		}
	}
}
