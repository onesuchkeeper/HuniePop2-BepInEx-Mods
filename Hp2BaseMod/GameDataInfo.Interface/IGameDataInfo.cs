// Hp2BaseMod 2022, By OneSuchKeeper

using System.Collections.Generic;
using Hp2BaseMod.ModLoader;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo.Interface
{
    public interface IGameDefinitionInfo<T>
    {
        /// <summary>
        /// Writes to the game data definition this represents
        /// </summary>
        /// <param name="def">The target game date definition to write to.</param>
        /// <param name="gameData">The game data.</param>
        /// <param name="assetProvider">The asset provider.</param>
        /// <param name="insertStyle">The insert style.</param>
        void SetData(ref T def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle);

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
