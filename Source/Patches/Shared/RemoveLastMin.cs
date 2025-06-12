using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeedBarOverflow.Patches
{
	// Some methods call Mathf.Min() to clamp values
	// Use this transpiler to remove it
	public static class RemoveLastMin
	{
		public static readonly Delegate d_transpiler = TranspilerMethod;
		public static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			int lastIdxBeforeLdc = -1;
			for (int i = instructionList.Count - 2; i > 0; i--)
			{
				if (instructionList[i].LoadsConstant(1.0) &&
					instructionList[i + 1].Calls(Refs.m_Min))
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
