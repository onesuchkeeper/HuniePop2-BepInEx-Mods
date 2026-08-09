using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod.Utility;

/// <summary>
/// Fluent helpers for <see cref="CutsceneStepInfo"/>, the mod-authoring (Info) side of a cutscene step.
/// These are meant to be chained directly onto a CutsceneStepUtility.Make*Info(...) call to set the
/// step's proceed type and/or doll target when the factory's own defaults (AUTOMATIC / FOCUSED) aren't
/// what's needed, e.g.:
/// </summary>
public static class CutsceneStepInfoFluentExtensions
{
    /// <summary>Sets how the step advances to the next one.</summary>
    public static CutsceneStepInfo Proceed(this CutsceneStepInfo info, CutsceneStepProceedType proceedType)
    {
        info.ProceedType = proceedType;
        return info;
    }

    /// <summary>Targets the currently-focused doll (the common case, and most factories' default).</summary>
    public static CutsceneStepInfo TargetFocused(this CutsceneStepInfo info)
    {
        info.DollTargetType = CutsceneStepDollTargetType.FOCUSED;
        return info;
    }

    /// <summary>Targets a random doll.</summary>
    public static CutsceneStepInfo TargetRandom(this CutsceneStepInfo info)
    {
        info.DollTargetType = CutsceneStepDollTargetType.RANDOM;
        return info;
    }

    /// <summary>Targets whichever doll currently represents the given girl definition.</summary>
    public static CutsceneStepInfo TargetGirl(this CutsceneStepInfo info, RelativeId targetGirlDefinitionId)
    {
        info.DollTargetType = CutsceneStepDollTargetType.GIRL_DEFINITION;
        info.TargetGirlDefinitionId = targetGirlDefinitionId;
        return info;
    }

    /// <summary>Targets whichever doll is at the given orientation slot.</summary>
    public static CutsceneStepInfo TargetOrientation(this CutsceneStepInfo info, DollOrientationType targetDollOrientation)
    {
        info.DollTargetType = CutsceneStepDollTargetType.ORIENTATION_TYPE;
        info.TargetDollOrientation = targetDollOrientation;
        return info;
    }

    /// <summary>
    /// Escape hatch for setting the doll target type directly without one of the payload-specific
    /// helpers above (e.g. if a new CutsceneStepDollTargetType value is ever added that doesn't need
    /// an accompanying payload field).
    /// </summary>
    public static CutsceneStepInfo WithDollTargetType(this CutsceneStepInfo info, CutsceneStepDollTargetType dollTargetType)
    {
        info.DollTargetType = dollTargetType;
        return info;
    }

    public static CutsceneStepInfo Proceed(this CutsceneStepInfo info)
    {
        info.ProceedBool = true;
        return info;
    }
}

/// <summary>
/// Fluent helpers for <see cref="CutsceneStepSubDefinition"/>, the runtime side of a cutscene step.
/// Mirrors <see cref="CutsceneStepInfoFluentExtensions"/> field-for-field; kept as a separate set of
/// extensions since CutsceneStepInfo and CutsceneStepSubDefinition share no common interface for these
/// fields (Info fields are nullable, SubDefinition fields are plain value types on a Unity object).
/// </summary>
public static class CutsceneStepSubDefinitionFluentExtensions
{
    /// <summary>Sets how the step advances to the next one.</summary>
    public static CutsceneStepSubDefinition Proceed(this CutsceneStepSubDefinition def, CutsceneStepProceedType proceedType)
    {
        def.proceedType = proceedType;
        return def;
    }

    /// <summary>Targets the currently-focused doll (the common case, and most factories' default).</summary>
    public static CutsceneStepSubDefinition TargetFocused(this CutsceneStepSubDefinition def)
    {
        def.dollTargetType = CutsceneStepDollTargetType.FOCUSED;
        return def;
    }

    /// <summary>Targets a random doll.</summary>
    public static CutsceneStepSubDefinition TargetRandom(this CutsceneStepSubDefinition def)
    {
        def.dollTargetType = CutsceneStepDollTargetType.RANDOM;
        return def;
    }

    /// <summary>Targets whichever doll currently represents the given girl definition.</summary>
    public static CutsceneStepSubDefinition TargetGirl(this CutsceneStepSubDefinition def, GirlDefinition targetGirlDefinition)
    {
        def.dollTargetType = CutsceneStepDollTargetType.GIRL_DEFINITION;
        def.targetGirlDefinition = targetGirlDefinition;
        return def;
    }

    /// <summary>Targets whichever doll is at the given orientation slot.</summary>
    public static CutsceneStepSubDefinition TargetOrientation(this CutsceneStepSubDefinition def, DollOrientationType targetDollOrientation)
    {
        def.dollTargetType = CutsceneStepDollTargetType.ORIENTATION_TYPE;
        def.targetDollOrientation = targetDollOrientation;
        return def;
    }

    /// <summary>
    /// Escape hatch for setting the doll target type directly without one of the payload-specific
    /// helpers above (e.g. if a new CutsceneStepDollTargetType value is ever added that doesn't need
    /// an accompanying payload field).
    /// </summary>
    public static CutsceneStepSubDefinition WithDollTargetType(this CutsceneStepSubDefinition def, CutsceneStepDollTargetType dollTargetType)
    {
        def.dollTargetType = dollTargetType;
        return def;
    }

    public static CutsceneStepSubDefinition Proceed(this CutsceneStepSubDefinition def)
    {
        def.proceedBool = true;
        return def;
    }
}