using Collections = System.Collections.Generic;
using HL = HarmonyLib;

namespace DeathsDoor.AlternativeGameModes;

public static class AlternativeGameModes
{
    public static void Add(string name, System.Action effect)
    {
        modes.Add(new() { Name = name, Effect = effect });
    }

    private struct AltGameMode
    {
        internal string Name;
        internal System.Action Effect;
    }

    private static readonly Collections.List<AltGameMode> modes = new()
    {
        new() {Name = "START", Effect = () => {}}
    };

    private static int selectedMode = 0;

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
            if (!__instance.saveFile.IsLoaded())
            {
                // The original code does this anyway at the very start
                // of useSaveFile. We do it here, slightly sooner,
                // so that the Effect has access to the save file the 
                // same way as in normal code.
                GameSave.currentSave = __instance.saveFile;
                modes[selectedMode].Effect();
            }
        }
    }
}