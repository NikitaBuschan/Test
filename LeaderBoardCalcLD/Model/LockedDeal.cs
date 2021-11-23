using System.ComponentModel.DataAnnotations;

namespace LeaderBoardCalcLD.Model
{
    public class LockedDeal
    {
        public int Id { get; set; }
        public int? ChainId { get; set; }
        public int PoolId { get; set; }
        public string Token { get; set; }
        public string FinishTime { get; set; }
        public string StartAmount { get; set; }
        public string Owner { get; set; }
        public string LogId { get; set; }
    }
}
