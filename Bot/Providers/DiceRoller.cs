using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FortuitousD20.Providers
{
    public class DiceRoller : IDiceRoller
    {
        private static RNGCryptoServiceProvider _rngCryptoProvider = new RNGCryptoServiceProvider();

        public async Task<int> RollDice(byte numberSides)
        {
            if (numberSides <= 0)
            {
                throw new ArgumentOutOfRangeException("numberSides");
            }

            byte[] randomNumber = new byte[1];
            do
            {
                _rngCryptoProvider.GetBytes(randomNumber);
            }
            while (!IsFairRoll(randomNumber[0], numberSides));

            return (int)((randomNumber[0] % numberSides) + 1);
        }

        private bool IsFairRoll(byte roll, byte numSides)
        {
            int fullSetsOfValues = Byte.MaxValue / numSides;
            return roll < numSides * fullSetsOfValues;
        }
    } 
}
