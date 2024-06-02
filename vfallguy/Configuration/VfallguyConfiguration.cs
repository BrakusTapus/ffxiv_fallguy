using System.Collections.Generic;
using System;
using Dalamud.Configuration;
using ECommons.DalamudServices;

namespace vfallguy.Configuration;

public class VfallguyConfiguration: IPluginConfiguration
{
	public int Version { get; set; }
	public bool HideOtherPlayers;

	[NonSerialized]
	public readonly Dictionary<string, Action<bool, bool, bool>> SettingDictionary =
		new(StringComparer.InvariantCultureIgnoreCase);

	public void Save() => Svc.PluginInterface.SavePluginConfig(this);
}
