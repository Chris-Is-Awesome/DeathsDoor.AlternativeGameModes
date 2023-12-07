using Bep = BepInEx;
using HL = HarmonyLib;

namespace DeathsDoor.AlternativeGameModes;

[Bep.BepInPlugin("deathsdoor.alternativegamemodes", "AlternativeGameModes", "1.0.0.0")]
internal class AlternativeGameModesPlugin : Bep.BaseUnityPlugin
{
    public void Start()
    {
        new HL.Harmony("deathsdoor.alternativegamemodes").PatchAll();
    }
}
