using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LeaderBoardCalcLD.Model
{
    class LockedDealEqualityComparer : IEqualityComparer<LockedDeal>
    {
        public bool Equals([AllowNull] LockedDeal x, [AllowNull] LockedDeal y)
        {
            return
                x.ChainId == y.ChainId &&
                x.PoolId == y.PoolId &&
                x.Token == y.Token &&
                x.FinishTime == y.FinishTime &&
                x.StartAmount == y.StartAmount &&
                x.Owner == y.Owner;
        }

        public int GetHashCode([DisallowNull] LockedDeal ld)
        {
            return
                ld.ChainId.GetHashCode() +
                ld.PoolId.GetHashCode() +
                ld.Token.GetHashCode() +
                ld.FinishTime.GetHashCode() +
                ld.StartAmount.GetHashCode() +
                ld.Owner.GetHashCode();
        }
    }
}
