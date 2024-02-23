
namespace AngelHornetLibrary
{
    public static class AhRandom
    {
        public static int Roll0(int sides) => RollDie(sides) - 1;
        public static int RollD20() => RollDie(20);
        public static int RollD12() => RollDie(12);
        public static int RollD10() => RollDie(10);
        public static int RollD8() => RollDie(8);
        public static int RollD6() => RollDie(6);
        public static int RollD4() => RollDie(4);
        public static int RollD3() => RollDie(3);
        public static int RollD2() => RollDie(2);
        public static int RollDie(int sides = 6)
        {
            int die = (int)new Random().Next(1, sides + 1);
            return die;
        }
                
        public static bool FlipCoin(int sides = 2)
        {
            int coin = (int)new Random().Next(0, sides);
            if (coin == 0) return true;
            else return false;
        }


    }
}
