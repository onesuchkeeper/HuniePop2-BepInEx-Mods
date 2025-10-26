// Hp2BaseMod 2025, By OneSuchKeeper

using System.Collections.Generic;

namespace Hp2BaseMod
{
    public static class Locations
    {
        public readonly static RelativeId MassageSpa = new RelativeId(-1, 9);
        public readonly static RelativeId Aquarium = new RelativeId(-1, 10);
        public readonly static RelativeId SecludedCabana = new RelativeId(-1, 11);
        public readonly static RelativeId PoolsideBar = new RelativeId(-1, 12);
        public readonly static RelativeId GolfCourse = new RelativeId(-1, 13);
        public readonly static RelativeId CruiseShip = new RelativeId(-1, 14);
        public readonly static RelativeId RooftopLounge = new RelativeId(-1, 15);
        public readonly static RelativeId Casino = new RelativeId(-1, 16);
        public readonly static RelativeId PrivateTable = new RelativeId(-1, 17);
        public readonly static RelativeId SecretGrotto = new RelativeId(-1, 18);
        public readonly static RelativeId StripClub = new RelativeId(-1, 19);
        public readonly static RelativeId RoyalSuite = new RelativeId(-1, 20);

        public readonly static RelativeId AirplaneBathroom = new RelativeId(-1, 26);
        public readonly static RelativeId OuterSpace = new RelativeId(-1, 23);
        public readonly static RelativeId HotelRoom = new RelativeId(-1, 21);
        public readonly static RelativeId TouristPlaza = new RelativeId(-1, 1);
        public readonly static RelativeId Boardwalk = new RelativeId(-1, 2);
        public readonly static RelativeId SurfShack = new RelativeId(-1, 3);
        public readonly static RelativeId HotelCourtyard = new RelativeId(-1, 4);
        public readonly static RelativeId GiftShop = new RelativeId(-1, 5);
        public readonly static RelativeId Airport = new RelativeId(-1, 6);
        public readonly static RelativeId HotelLobby = new RelativeId(-1, 7);
        public readonly static RelativeId Marina = new RelativeId(-1, 8);
        public readonly static RelativeId YourApartment = new RelativeId(-1, 22);
        public readonly static RelativeId AirplaneCabin = new RelativeId(-1, 25);
        public readonly static RelativeId AirportSpecial = new RelativeId(-1, 28);
        public readonly static RelativeId HotelLobbySpecial = new RelativeId(-1, 29);
        public readonly static RelativeId VolcanoTop = new RelativeId(-1, 27);
        public readonly static RelativeId Poolside = new RelativeId(-1, 24);

        public static IEnumerable<RelativeId> NormalDateLocIds
        {
            get
            {
                yield return MassageSpa;
                yield return Aquarium;
                yield return SecludedCabana;
                yield return PoolsideBar;
                yield return GolfCourse;
                yield return CruiseShip;
                yield return RooftopLounge;
                yield return Casino;
                yield return PrivateTable;
                yield return SecretGrotto;
                yield return StripClub;
                yield return RoyalSuite;
            }
        }
    }
}