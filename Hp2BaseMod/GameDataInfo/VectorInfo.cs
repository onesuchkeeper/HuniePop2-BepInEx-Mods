﻿// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    public class VectorInfo : IGameDefinitionInfo<Vector2>
    {
        public float? Xpos;

        public float? Ypos;

        /// <summary>
        /// Constructor
        /// </summary>
        public VectorInfo() { }

        /// <summary>
        /// Constructor from 2 floats
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public VectorInfo(float x, float y)
        {
            Xpos = x;
            Ypos = y;
        }

        /// <summary>
        /// Constructor from a defined vector instance.
        /// </summary>
        /// <param name="vector"></param>
        public VectorInfo(Vector2 vector)
        {
            Xpos = vector.x;
            Ypos = vector.y;
        }

        /// <inheritdoc/>
        public void SetData(ref Vector2 def, GameDefinitionProvider _, AssetProvider __, InsertStyle insertStyle)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<Vector2>();
            }

            ValidatedSet.SetValue(ref def.x, Xpos);
            ValidatedSet.SetValue(ref def.y, Ypos);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            //noop
        }
    }
}
