using BepInEx;

#pragma warning disable BepInEx001

namespace Hp2BaseMod;

/// <summary>
/// Base class for Hp2BaseMod plugins.
/// 
/// Provides automatic registration for:
/// - Config properties (via [ConfigProperty] on fields)
/// - Interop methods (via [InteropMethod] on methods)
/// 
/// Your plugin class must be declared as 'partial' to use these features.
/// </summary>
public abstract partial class Hp2BaseModPlugin : BaseUnityPlugin
{
    public const string CONFIG_GENERAL = "general";

    /// <summary>
    /// The unique mod ID assigned by ModInterface.
    /// </summary>
    public int ModId { get; private set; }

    private bool _hasInitialized = false;

    /// <summary>
    /// Initializes the plugin with automatic config registration.
    /// Config properties are registered immediately.
    /// Interop methods are registered on first Awake() call.
    /// </summary>
    /// <param name="pluginGuid">The BepInEx plugin GUID (typically MyPluginInfo.PLUGIN_GUID)</param>
    protected Hp2BaseModPlugin(string pluginGuid)
    {
        ModId = ModInterface.GetSourceId(pluginGuid);
    }

    /// <summary>
    /// Called by Unity when the plugin is loaded.
    /// Registers interop methods on first call.
    /// Override this method to add your own initialization, but make sure to call base.Awake()!
    /// </summary>
    protected virtual void Awake()
    {
        if (!_hasInitialized)
        {
            _hasInitialized = true;

            using (ModInterface.Log.MakeIndent())
            {
                // Register interop methods (ModInterface.Log is available now)
                RegisterInteropMethods();
            }
        }
    }

    /// <summary>
    /// Registers interop methods. Override this if you need custom registration logic.
    /// The source generator provides an override if [InteropMethod] methods exist.
    /// </summary>
    protected virtual void RegisterInteropMethods()
    {
        // No-op by default
    }
}