internal static class DiagnosticStrings
{
    public const string ID_DEPRECIATED_MEMBER = "HP001";
    public const string MESSAGE_PREFIX_DEPRECIATED = "{1}.{0} has been depreciated.";

    public const string ID_OVERWRITTEN_METHOD = "HP002";
    public const string MESSAGE_PREFIX_OVERWRITTEN = "{1}.{0} has been overwritten by the base mod.";

    public const string ID_REPURPOSED_FIELD = "HP003";
    public const string MESSAGE_REPURPOSED_FIELD = "{1}.{0} has been repurposed.";

    public const string ID_INVALID_INTEROP_METHOD = "HP004";
    public const string MESSAGE_INVALID_INTEROP_METHOD = "InteropMethod attribute on '{0}' can only be used on methods in classes that inherit from Hp2BaseModPlugin.";

    public const string ID_CLASS_NOT_PARTIAL_INTEROP = "HP005";
    public const string MESSAGE_CLASS_NOT_PARTIAL_INTEROP = "Class '{0}' contains InteropMethod methods but is not declared as partial. Add the 'partial' keyword to the class declaration.";
}