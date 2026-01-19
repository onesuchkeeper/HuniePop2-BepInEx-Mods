// Hp2BaseMod 2025, By OneSuchKeeper

using System.Collections.Generic;

namespace Hp2BaseMod
{
    public static class Girls
    {
        public readonly static RelativeId Lola = new RelativeId(-1, 1);
        public readonly static RelativeId Jessie = new RelativeId(-1, 2);
        public readonly static RelativeId Lillian = new RelativeId(-1, 3);
        public readonly static RelativeId Zoey = new RelativeId(-1, 4);
        public readonly static RelativeId Sarah = new RelativeId(-1, 5);
        public readonly static RelativeId Lailani = new RelativeId(-1, 6);
        public readonly static RelativeId Candace = new RelativeId(-1, 7);
        public readonly static RelativeId Nora = new RelativeId(-1, 8);
        public readonly static RelativeId Brooke = new RelativeId(-1, 9);
        public readonly static RelativeId Ashley = new RelativeId(-1, 10);
        public readonly static RelativeId Abia = new RelativeId(-1, 11);
        public readonly static RelativeId Polly = new RelativeId(-1, 12);
        public readonly static RelativeId Kyu = new RelativeId(-1, 13);
        public readonly static RelativeId Moxie = new RelativeId(-1, 14);
        public readonly static RelativeId Jewn = new RelativeId(-1, 15);

        public static IEnumerable<RelativeId> AllGirls
        {
            get
            {
                yield return Lola;
                yield return Jessie;
                yield return Lillian;
                yield return Zoey;
                yield return Sarah;
                yield return Lailani;
                yield return Candace;
                yield return Nora;
                yield return Brooke;
                yield return Ashley;
                yield return Abia;
                yield return Polly;
            }
        }

        public static IEnumerable<RelativeId> NormalGirls
        {
            get
            {
                yield return Lola;
                yield return Jessie;
                yield return Lillian;
                yield return Zoey;
                yield return Sarah;
                yield return Lailani;
                yield return Candace;
                yield return Nora;
                yield return Brooke;
                yield return Ashley;
                yield return Abia;
                yield return Polly;
                yield return Kyu;
                yield return Moxie;
                yield return Jewn;
            }
        }

        public static IEnumerable<RelativeId> SpecialGirls
        {
            get
            {
                yield return Kyu;
                yield return Moxie;
                yield return Jewn;
            }
        }

        public static IEnumerable<RelativeId> Nymphojinn
        {
            get
            {
                yield return Moxie;
                yield return Jewn;
            }
        }
    }
}