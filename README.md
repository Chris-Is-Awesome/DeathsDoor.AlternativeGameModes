This is a mod for Death's Door that enables other mods to register
alternative game modes that can be selected in the main menu when
creating a new file.

# How to select a mode in-game

Select a save slot, then while START is highlighted, press left or right
to cycle through the list of available game modes. Confirming will start
the currently selected one.

# How to add modes

To add a new game mode, call `AlternativeGameModes.Add` when your mod
initializes. This call takes two parameters: the name of the mode, which 
will be displayed on the menu, and a delegate that will be called when
a new save file is created in that mode. The delegate is called
at the start of SaveSlot.useSaveFile, with GameSave.currentSave already
set to the new file.

The delegate will not be called again when the file is reloaded, so it
should usually set a flag on the save file to indicate that the chosen
game mode is active.

## Example

    using AGM = DDoor.AlternativeGameModes;

    // ... somewhere in your mod class
    public void Start()
    {
        AGM.AlternativeGameModes.Add("START EXAMPLE", () =>
        {
            GameSave.currentSave.SetKeyState("ExampleMode", true);
        });
    }

# How to detect the selected mode

The property `AlternativeGameModes.SelectedModeName` contains the name
of the currently selected mode. You should only use this while the main
menu is active; it may not return a meaningful value elsewhere.
