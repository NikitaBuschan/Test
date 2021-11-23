using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LeaderBoardCalc.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LeaderBoardCalc
{
    public class Function
    {
        public ILambdaContext _context;
        public async Task<string> FunctionHandler(ILambdaContext context)
        {
            Lambda._context = context;
            _context = context;

            List<WalletsList> list = new List<WalletsList>();

            var etherscanLPs = await GetWallets("LeaderBoardCalcLP", "1");
            var binanceLPs = await GetWallets("LeaderBoardCalcLP", "56");

            _context.Logger.LogLine($"Leader Board Calc LP 1 return: {etherscanLPs.Count}, Leader Board Calc LP 56 return: {binanceLPs.Count}");
            list.AddRange(etherscanLPs);
            list.AddRange(binanceLPs);

            var etherscanLDs = await GetWallets("LeaderBoardCalcLD", "1");

            _context.Logger.LogLine($"Get {etherscanLDs.Count} LD wallets");

            list.AddRange(etherscanLDs);

            Lambda.Log($"Create leader boar list");
            var result = CraeteLeaderBoardList(list);

            Lambda.Log($"Run clear Leader Board");
            await ClearDb();

            result.Remove(result.FirstOrDefault(x => x.Owner == "0x663A5C229c09b049E36dCc11a9B0d4a8Eb9db214".ToLower()));

            var sortedList = result.OrderByDescending(x => x.Amount).ToList();

            var res = CreateLeaderBoardReadyObject(sortedList);

            Lambda.Log($"Set ranks");
            res = SetRanks(res);

            await SaveToDb(res);


            return $"All done, save {res.Count} rows";
        }

        public async Task SaveToDb(List<LeaderBoardReady> result)
        {
            var from = 0;
            var count = result.Count / 2;

            if (result.Count % 2 != 0)
                count = result.Count / 2 + 1;
            
            var send = result.GetRange(from, count);

            var write = await WriteWallets(send);


            from = count;
            count = result.Count / 2;
            send = result.GetRange(from, count);

            write = await WriteWallets(send);
        }

        public List<LeaderBoardReady> CreateLeaderBoardReadyObject(List<LeaderBoard> sortedList)
        {
            var res = new List<LeaderBoardReady>();
            foreach (var item in sortedList)
            {
                res.Add(new LeaderBoardReady() { Amount = item.Amount.ToString(), Owner = item.Owner });
            }

            return res;
        }

        public List<LeaderBoardReady> SetRanks(List<LeaderBoardReady> res)
        {
            var counter = 0;
            var from = 0;

            for (int i = 1; i < res.Count; i++)
            {
                counter++;
                var to = 0;

                if (res[i - 1].Amount == res[i].Amount)
                {
                    from = counter - 1;
                    for (int j = i; j < res.Count; j++)
                    {
                        if (res[j - 1].Amount != res[j].Amount)
                        {
                            to = counter;
                            break;
                        }
                        i++;
                        counter++;
                    }

                    for (int h = from; h < to; h++)
                    {
                        res[h].Rank = counter.ToString();
                    }
                }

                res[i - 1].Rank = counter.ToString();
                from = i;
            }

            res[from].Rank = (++counter).ToString();

            return res;
        }

        public List<LeaderBoard> CraeteLeaderBoardList(List<WalletsList> list)
        {
            List<LeaderBoard> result = new List<LeaderBoard>();

            for (int i = 0; i < list.Count; i++)
            {
                if (result.Where(x => x.Owner == list[i].Owner).Count() == 0)
                {
                    var wallet = new LeaderBoard()
                    {
                        Owner = list[i].Owner,
                        Amount = list[i].Amount
                    };

                    for (int j = i + 1; j < list.Count; j++)
                    {
                        if (wallet.Owner == list[j].Owner)
                        {
                            wallet.Amount += list[j].Amount;
                        }
                    }

                    result.Add(wallet);
                }
            }

            return result;
        }

        public async Task<string> ClearDb()
        {
            var dbObject = new DbObject()
            {
                Name = "leaderBoard",
                Data = ""
            };

            var result = await Lambda.Run("LambdaDbDeleter", System.Text.Json.JsonSerializer.Serialize(dbObject));

            return result;
        }

        public async Task<string> WriteWallets(List<LeaderBoardReady> list)
        {
            var dbObject = new DbObject()
            {
                Name = "leaderBoard",
                Data = System.Text.Json.JsonSerializer.Serialize(list)
            };

            var result = await Lambda.Run("DBWriter", System.Text.Json.JsonSerializer.Serialize(dbObject));

            return result;
        }

        public async Task<List<WalletsList>> GetWallets(string lambda, string chainId)
        {
            var result = await Lambda.Run(lambda, System.Text.Json.JsonSerializer.Serialize(chainId));

            var list = new List<WalletsList>();

            try
            {
                list = System.Text.Json.JsonSerializer.Deserialize<List<WalletsList>>(result);
            }
            catch (Exception ex)
            {
                Lambda.Log($"Get wallets get error: {ex.Message}");
                return null;
            }

            return list;
        }
    }
}
