﻿// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a definition
    /// </summary>
    public abstract class DataMod
    {
        public RelativeId Id { get; }

        public int LoadPriority { get; }

        public InsertStyle InsertStyle { get; }

        /// <summary>
        /// Required for serialization, it is not recommended that you use this.
        /// Use the constructor with id parameters instead.
        /// </summary>
        public DataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The game data id.</param>
        /// <param name="insertStyle">The way in which mod data should be applied to the data instance.</param>
        public DataMod(RelativeId id, InsertStyle insertStyle, int loadPriority)
        {
            Id = id;
            InsertStyle = insertStyle;
            LoadPriority = loadPriority;
        }
    }
}
