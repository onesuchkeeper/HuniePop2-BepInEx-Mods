using System.Collections.Generic;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo.Interface
{
    public interface IGirlSubDataMod<T>
    {
        RelativeId Id { get; }
        int LoadPriority { get; }

        /// <summary>
        /// Writes to the definition this modifies
        /// </summary>
        /// <param name="def"></param>
        /// <param name="gameData"></param>
        /// <param name="assetProvider"></param>
        /// <param name="insertStyle"></param>
        /// <param name="girlId"></param>
        void SetData(ref T def,
                     GameDefinitionProvider gameData,
                     AssetProvider assetProvider,
                     InsertStyle insertStyle,
                     RelativeId girlId);

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
