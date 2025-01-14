﻿namespace RegionKit.Modules.Misc;

[RegionKitModule(nameof(Enable), nameof(Disable), nameof(Setup), moduleName: "Miscellanceous")]
internal static class _Module
{

	public static void Setup()
	{
		_Enums.Register();
		ExtendedGates.InitExLocks();
	}
	public static void Enable()
	{

		//PaletteTextInput.Apply();
		SunBlockerFix.Apply();
		//CloudAdjustment.Apply();
		ExtendedGates.Enable();
		SuperstructureFusesHook.Apply();
		MoreFadePalettes.Apply();
		SlugcatRoomTemplates.Apply();
		RainSong.Enable();
		FadePaletteCombiner.Enable();
	}
	public static void Disable()
	{
		//PaletteTextInput.Undo();
		SunBlockerFix.Undo();
		//CloudAdjustment.Undo();
		ExtendedGates.Disable();
		SuperstructureFusesHook.Undo();
		MoreFadePalettes.Undo();
		SlugcatRoomTemplates.Undo();
		RainSong.Disable();
		FadePaletteCombiner.Disable();
	}
}
