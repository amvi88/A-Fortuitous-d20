using System.Threading.Tasks;

namespace FortuitousD20.Providers
{
    public interface IDiceRoller
    {
        Task<int> RollDice(byte numberSides);
    }
}
