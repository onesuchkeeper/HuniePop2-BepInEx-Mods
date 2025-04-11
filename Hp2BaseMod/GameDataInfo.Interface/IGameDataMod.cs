// Hp2BaseMod 2022, By OneSuchKeeper

using System.Collections.Generic;
using Hp2BaseMod.ModLoader;

namespace Hp2BaseMod.GameDataInfo.Interface
{
    public interface IGameDataMod<T>
    {
        RelativeId Id { get; }
        int LoadPriority { get; }

        /// <summary>
        /// Writes to the game data definition this represents
        /// </summary>
        /// <param name="def">The target game date definition to write to.</param>
        /// <param name="gameData">The game data.</param>
        /// <param name="assetProvider">The asset provider.</param>
        void SetData(T def, GameDefinitionProvider gameData, AssetProvider assetProvider);

        /// <summary>
        /// Returns an <see cref="IEnumerable{string}"/> of paths to internal sprite assets used in the definition.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetInternalSpriteRequests();

        /// <summary>
        /// Returns an <see cref="IEnumerable{string}"/> of paths to internal audio assets used in the definition.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetInternalAudioRequests();
    }
}
