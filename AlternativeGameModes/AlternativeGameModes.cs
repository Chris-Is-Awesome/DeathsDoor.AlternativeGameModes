using Collections = System.Collections.Generic;
using HL = HarmonyLib;

namespace DDoor.AlternativeGameModes;

public static class AlternativeGameModes
{
	public static void Add(string name, System.Action effect, string modeKey)
	{
		modes.Add(new() { Name = name, Effect = effect, ModeKey = modeKey });
	}

	public static string SelectedModeName => modes[selectedMode].Name;

	private struct AltGameMode
	{
		internal string Name;
		internal System.Action Effect;
		internal string ModeKey;
	}

	private static readonly Collections.List<AltGameMode> modes = new()
	{
		new() {Name = "START", Effect = () => {}}
	};

	private static int selectedMode = 0;

	[HL.HarmonyPatch(typeof(SaveMenu), nameof(SaveMenu.closeSubMenu))]
	private static class CloseSubMenuPatch
	{
		private static void Postfix(SaveMenu __instance, bool silent)
		{
			// When silent = true, we are starting the game.
			// We need to keep the mode set until NewGamePatch runs.
			if (!silent)
			{
				selectedMode = 0;
				__instance.selectedOptions[0].buttonText.text = modes[selectedMode].Name;
			}
		}
	}

	// SaveMenu does not have any implementation for onLeftEvent
	// or onRightEvent, so we cannot hook that directly.
	// Instead, we hook on UIObject and check if we're operating on
	// the SaveMenu. It's suboptimal but it works.
	[HL.HarmonyPatch(typeof(UIObject), nameof(UIObject.onRightEvent))]
	private static class RightPatch
	{
		private static void Postfix(UIObject __instance)
		{
			if (!(__instance is SaveMenu sm))
			{
				return;
			}
			if (sm.selectedSlot && sm.optionIndex == 0)
			{
				selectedMode = (selectedMode + 1) % modes.Count;
				sm.selectedOptions[0].buttonText.text = modes[selectedMode].Name;
			}
		}
	}

	[HL.HarmonyPatch(typeof(UIObject), nameof(UIObject.onLeftEvent))]
	private static class LeftPatch
	{
		private static void Postfix(UIObject __instance)
		{
			if (!(__instance is SaveMenu sm))
			{
				return;
			}
			if (sm.selectedSlot && sm.optionIndex == 0)
			{
				selectedMode--;
				if (selectedMode < 0)
				{
					selectedMode += modes.Count;
				}
				sm.selectedOptions[0].buttonText.text = modes[selectedMode].Name;
			}
		}
	}

	[HL.HarmonyPatch(typeof(SaveSlot), nameof(SaveSlot.useSaveFile))]
	private static class NewGamePatch
	{
		private static void Prefix(SaveSlot __instance)
		{
			// The original code does this anyway at the very start
			// of useSaveFile. We do it here, slightly sooner,
			// so that the Effect has access to the save file the 
			// same way as in normal code.
			GameSave.currentSave = __instance.saveFile;

			// If creating new file
			if (!__instance.saveFile.IsLoaded())
			{
				modes[selectedMode].Effect();
				selectedMode = 0;
			}
			// If loading an existing file
			else
			{
				// Check if file is an alt game mode
				// If it is, run its delegate
				// Start at index of 1 to skip the default "START" mode
				for (int i = 1; i < modes.Count; i++)
				{
					if (__instance.saveFile.IsKeyUnlocked(modes[i].ModeKey))
					{
						modes[i].Effect();
						break;
					}
				}
			}
		}
	}
}