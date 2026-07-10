namespace Hp2BaseMod.Analyzer
{
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

        public const string ID_EXPANSION_NAMING_MISMATCH = "HP006";
        public const string MESSAGE_EXPANSION_NAMING_MISMATCH = "Class '{0}' is an Expansion of '{1}' and must be named 'Expanded{1}' per the Expansion naming convention.";

        public const string ID_MEMBER_RULE_WITHOUT_EXPANSION = "HP009";
        public const string MESSAGE_MEMBER_RULE_WITHOUT_EXPANSION = "'{0}' on '{1}' declares a base-game member rule, but '{1}' is not marked with [Expansion]. Add [Expansion(typeof(BaseType))] to the containing class.";

        public const string ID_EXPANSION_CLASS_NOT_PARTIAL = "HP010";
        public const string MESSAGE_EXPANSION_CLASS_NOT_PARTIAL = "Expansion class '{0}' must be declared 'partial' so the analyzer can generate its shared Get/Destroy implementation.";

        public const string ID_EXPANSION_MEMBER_CONFLICTS_WITH_GENERATED = "HP012";
        public const string MESSAGE_EXPANSION_MEMBER_CONFLICTS_WITH_GENERATED = "'{0}' on '{1}' conflicts with a member the Expansion generator would otherwise emit here. Rename or remove it, or adjust the [Expansion] configuration (Fields/Methods/HasModId) that produces it.";

        public const string ID_EXPANSION_MEMBER_NOT_FOUND = "HP013";
        public const string MESSAGE_EXPANSION_MEMBER_NOT_FOUND = "'{0}' was not found on '{1}' or any of its base types. Check the Fields/Methods list on the [Expansion] attribute for a typo, or a version mismatch between the referenced assembly and whatever source you checked the name against.";

        public const string ID_EXPANSION_MEMBER_TYPE_INACCESSIBLE = "HP014";
        public const string MESSAGE_EXPANSION_MEMBER_TYPE_INACCESSIBLE = "'{0}' on '{1}' has a parameter or return type that isn't accessible from the generated code. Falling back to the raw {2} instead of a typed wrapper.";

        public const string ID_EXPANSION_MEMBER_KIND_MISMATCH = "HP015";
        public const string MESSAGE_EXPANSION_MEMBER_KIND_MISMATCH = "'{0}' on '{1}' exists, but is a {2}, not the kind of member this list expects. Fields expects a field; Methods expects an ordinary method (not a property/event accessor or explicit interface implementation).";
    }
}