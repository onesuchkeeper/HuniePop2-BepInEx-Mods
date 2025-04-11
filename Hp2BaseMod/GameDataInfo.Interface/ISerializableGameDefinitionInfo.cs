// Hp2BaseMod 2025, By OneSuchKeeper
using System;

namespace Hp2BaseMod.GameDataInfo.Interface;

public interface ISerializableGameDefinitionInfo<T> : IGameDefinitionInfo<T>
{
    void ReplaceRelativeIds(Func<RelativeId?, RelativeId?> getNewSource);
}