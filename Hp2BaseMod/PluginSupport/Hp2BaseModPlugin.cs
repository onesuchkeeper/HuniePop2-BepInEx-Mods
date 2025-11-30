using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;

#pragma warning disable BepInEx001

namespace Hp2BaseMod;

/// <summary>
/// Inheriting this class is NOT nesisary for compatibility with the Hp2BaseMod.
/// 
/// Implements behaviors common in Hp2BaseMod plugin development.
/// Use <see cref="ConfigPropertyAttribute"/> and <see cref="InteropMethodAttribute"/>
/// </summary>
public abstract class Hp2BaseModPlugin : BaseUnityPlugin
{
    public const string CONFIG_GENERAL = "general";

    private static MethodInfo m_Bind =
    typeof(ConfigFile)
        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .FirstOrDefault(m => m.Name == "Bind"
            && m.IsGenericMethodDefinition
            && m.GetGenericArguments().Length == 1
            && m.GetParameters() is var parameters
            && parameters.Length == 4
            && parameters[0].ParameterType == typeof(string)
            && parameters[1].ParameterType == typeof(string)
            && parameters[3].ParameterType == typeof(string));

    private static Dictionary<Type, MethodInfo> GenericBinds = new();

    public int ModId => _modId;
    private int _modId;

    public Hp2BaseModPlugin(string pluginGuid)
    {
        using (ModInterface.Log.MakeIndent())
        {
            _modId = ModInterface.GetSourceId(pluginGuid);

            var type = GetType();

            // config properties
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                var configPropertyAtt = property.GetCustomAttribute<ConfigPropertyAttribute>(true);
                if (configPropertyAtt == null) continue;

                var propertyType = property.PropertyType;

                if (!GenericBinds.TryGetValue(propertyType, out var genericBind))
                {
                    genericBind = m_Bind.MakeGenericMethod(propertyType);
                    GenericBinds.Add(propertyType, genericBind);
                }

                genericBind.Invoke(Config, [
                    configPropertyAtt.CategoryName,
                    property.Name,
                    configPropertyAtt.DefaultValue,
                    configPropertyAtt.Description
                ]);
            }

            // interop methods
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                var interopMethodAtt = method.GetCustomAttribute<InteropMethodAttribute>(true);
                if (interopMethodAtt == null)
                    continue;

                bool isStatic = method.IsStatic;

                // Build the correct delegate signature (no implicit "this" because we never bind open instance)
                var delegateType = BuildDelegateType(method);

                Delegate del;

                if (isStatic)
                {
                    // Static → open delegate → no target instance
                    del = Delegate.CreateDelegate(delegateType, method);
                }
                else
                {
                    // Instance → closed delegate → bind "this"
                    del = Delegate.CreateDelegate(delegateType, this, method);
                }

                ModInterface.RegisterInterModValue(_modId, method.Name, del);
            }
        }
    }

    /// <summary>
    /// Builds Action<> or Func<> type dynamically for the method signature.
    /// </summary>
    private static Type BuildDelegateType(MethodInfo m)
    {
        var paramTypes = m.GetParameters().Select(p => p.ParameterType).ToArray();

        if (m.ReturnType == typeof(void))
        {
            // Example: void M(int, string) → Action<int,string>
            return Expression.GetActionType(paramTypes);
        }
        else
        {
            // Example: int M(int,string) → Func<int,string,int>
            var typeArgs = paramTypes.Append(m.ReturnType).ToArray();
            return Expression.GetDelegateType(typeArgs);
        }
    }

    public T GetConfigProperty<T>([CallerMemberName] string propertyName = null)
        => GetConfigProperty<T>(CONFIG_GENERAL, default, propertyName);

    public T GetConfigProperty<T>(string categoryName, [CallerMemberName] string propertyName = null)
        => GetConfigProperty<T>(categoryName, default, propertyName);

    public T GetConfigProperty<T>(string categoryName, T defaultValue, [CallerMemberName] string propertyName = null)
    {
        return Config.TryGetEntry<T>(categoryName, propertyName, out var config)
            ? config.Value
            : defaultValue;
    }

    public void SetConfigProperty<T>(T value, [CallerMemberName] string propertyName = null)
    => SetConfigProperty(CONFIG_GENERAL, value, propertyName);

    public void SetConfigProperty<T>(string categoryName, T value, [CallerMemberName] string propertyName = null)
    {
        if (Config.TryGetEntry<T>(categoryName, propertyName, out var config))
        {
            config.Value = value;
        }
        else
        {
            Logger.LogWarning($"Failed to find binding for config property {propertyName}");
        }
    }
}
