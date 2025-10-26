// Hp2BaseMod 2025, By OneSuchKeeper

using System.Collections.Generic;

namespace Hp2BaseMod
{
    public static class Girls
    {
        public readonly static RelativeId LolaId = new RelativeId(-1, 1);
        public readonly static RelativeId JessieId = new RelativeId(-1, 2);
        public readonly static RelativeId LillianId = new RelativeId(-1, 3);
        public readonly static RelativeId ZoeyId = new RelativeId(-1, 4);
        public readonly static RelativeId SarahId = new RelativeId(-1, 5);
        public readonly static RelativeId LailaniId = new RelativeId(-1, 6);
        public readonly static RelativeId CandaceId = new RelativeId(-1, 7);
        public readonly static RelativeId NoraId = new RelativeId(-1, 8);
        public readonly static RelativeId BrookeId = new RelativeId(-1, 9);
        public readonly static RelativeId AshleyId = new RelativeId(-1, 10);
        public readonly static RelativeId AbiaId = new RelativeId(-1, 11);
        public readonly static RelativeId PollyId = new RelativeId(-1, 12);
        public readonly static RelativeId KyuId = new RelativeId(-1, 13);
        public readonly static RelativeId MoxieId = new RelativeId(-1, 14);
        public readonly static RelativeId JewnId = new RelativeId(-1, 15);

        public static IEnumerable<RelativeId> AllGirls
        {
            get
            {
                yield return LolaId;
                yield return JessieId;
                yield return LillianId;
                yield return ZoeyId;
                yield return SarahId;
                yield return LailaniId;
                yield return CandaceId;
                yield return NoraId;
                yield return BrookeId;
                yield return AshleyId;
                yield return AbiaId;
                yield return PollyId;
            }
        }

        public static IEnumerable<RelativeId> NormalGirls
        {
            get
            {
                yield return LolaId;
                yield return JessieId;
                yield return LillianId;
                yield return ZoeyId;
                yield return SarahId;
                yield return LailaniId;
                yield return CandaceId;
                yield return NoraId;
                yield return BrookeId;
                yield return AshleyId;
                yield return AbiaId;
                yield return PollyId;
                yield return KyuId;
                yield return MoxieId;
                yield return JewnId;
            }
        }

        public static IEnumerable<RelativeId> SpecialGirls
        {
            get
            {
                yield return KyuId;
                yield return MoxieId;
                yield return JewnId;
            }
        }

        public static IEnumerable<RelativeId> Nymphojinn
        {
            get
            {
                yield return MoxieId;
                yield return JewnId;
            }
        }
    }
}