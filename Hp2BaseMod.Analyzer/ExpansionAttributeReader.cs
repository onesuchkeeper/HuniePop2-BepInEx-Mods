using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Hp2BaseMod.Analyzer;

// Centralizes all [Expansion]/[Deprecates]/[Overwrites]/[Repurposes] attribute-reading logic so
// ExpansionAnalyzer (diagnostics) and ExpansionGenerator (codegen) share one implementation
// instead of drifting copies.
internal static class ExpansionAttributeReader
{
    private const string ExpansionAttributeName = "ExpansionAttribute";
    private const string ExpansionShortName = "Expansion";
    private const string DeprecatesAttributeName = "DeprecatesAttribute";
    private const string DeprecatesShortName = "Deprecates";
    private const string OverwritesAttributeName = "OverwritesAttribute";
    private const string OverwritesShortName = "Overwrites";
    private const string RepurposesAttributeName = "RepurposesAttribute";
    private const string RepurposesShortName = "Repurposes";

    public static AttributeData FindExpansionAttribute(INamedTypeSymbol classSymbol)
    {
        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (attribute.AttributeClass == null)
            {
                continue;
            }

            if (IsAttributeNamed(attribute.AttributeClass, ExpansionAttributeName, ExpansionShortName))
            {
                return attribute;
            }
        }

        return null;
    }

    public static INamedTypeSymbol GetExpansionBaseType(INamedTypeSymbol classSymbol)
    {
        var attribute = FindExpansionAttribute(classSymbol);
        if (attribute == null || attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        return attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
    }

    public static ImmutableArray<string> GetStringArrayNamedArgument(AttributeData attribute, string name)
    {
        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (namedArgument.Key != name)
            {
                continue;
            }

            if (namedArgument.Value.Kind != TypedConstantKind.Array || namedArgument.Value.Values.IsDefault)
            {
                return ImmutableArray<string>.Empty;
            }

            var builder = ImmutableArray.CreateBuilder<string>(namedArgument.Value.Values.Length);
            foreach (var element in namedArgument.Value.Values)
            {
                if (element.Value is string text && !string.IsNullOrEmpty(text))
                {
                    builder.Add(text);
                }
            }

            return builder.ToImmutable();
        }

        return ImmutableArray<string>.Empty;
    }

    public static bool GetBoolNamedArgument(AttributeData attribute, string name)
    {
        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (namedArgument.Key == name && namedArgument.Value.Value is bool flag)
            {
                return flag;
            }
        }

        return false;
    }

    public static MemberRuleKind ClassifyMemberRuleAttribute(INamedTypeSymbol attributeClass)
    {
        if (IsAttributeNamed(attributeClass, DeprecatesAttributeName, DeprecatesShortName))
        {
            return MemberRuleKind.Deprecates;
        }

        if (IsAttributeNamed(attributeClass, OverwritesAttributeName, OverwritesShortName))
        {
            return MemberRuleKind.Overwrites;
        }

        if (IsAttributeNamed(attributeClass, RepurposesAttributeName, RepurposesShortName))
        {
            return MemberRuleKind.Repurposes;
        }

        return MemberRuleKind.None;
    }

    private static bool IsAttributeNamed(INamedTypeSymbol attributeClass, string fullName, string shortName)
    {
        var name = attributeClass.Name;

        if (name == fullName || name == shortName)
        {
            return true;
        }

        if (name.Length > 9 && name.EndsWith("Attribute", System.StringComparison.Ordinal))
        {
            var trimmed = name.Substring(0, name.Length - 9);
            if (trimmed == shortName)
            {
                return true;
            }
        }

        return false;
    }
}