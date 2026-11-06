// Hp2BaseMod 2026, By OneSuchKeeper

using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Save;

/// <summary>
/// Separates mod context from the rest of the save so it can be loaded first.
/// </summary>
[Serializable]
class ModSaveMeta
{
    public Dictionary<string, int> SourceGUID_Id = new Dictionary<string, int>();
    public string Version = MyPluginInfo.PLUGIN_VERSION;
}
