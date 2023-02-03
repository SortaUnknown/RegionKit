﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegionKit.Modules.AridBarrens;

///<inheritdoc/>
[RegionKitModule(nameof(Register), nameof(Disable), moduleName: "AridBarrens")]
public static class _Module
{
	/// <summary>
	/// Applies hooks.
	/// </summary>
	public static void Register()
	{
		_CommonHooks.PostRoomLoad += RoomPostLoad;
	}
	/// <summary>
	/// Undoes hooks.
	/// </summary>
	public static void Disable()
	{
		_CommonHooks.PostRoomLoad -= RoomPostLoad;
	}

	private static void RoomPostLoad(Room self)
	{
		for (int k = 0; k < self.roomSettings.effects.Count; k++)
		{
			if (self.roomSettings.effects[k].type == _Enums.SandStorm)
			{
				self.AddObject(new SandStorm(self.roomSettings.effects[k], self));
			}
			else if (self.roomSettings.effects[k].type == _Enums.SandPuffs)
			{
				self.AddObject(new SandPuffsScene(self.roomSettings.effects[k], self));
			}
		}
	}
}
