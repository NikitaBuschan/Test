using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LeaderBoardCalcLD.Model;
using Nethereum.Web3;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LeaderBoardCalcLD
{
    public class Function
    {
        public async Task<List<WalletsList>> FunctionHandler(string chainId, ILambdaContext context)
        {
            DateTime start = DateTime.UtcNow;
            Lambda._context = context;

            var list = await GetWallets();

            if (list == null)
            {
                Lambda.Log($"Get wallets return null. Working time: {DateTime.UtcNow - start}");
                return null;
            }
            Lambda.Log($"Get wallets list with {list.Count} wallets");
             
            // Distinct list
            list = list.Distinct(new LockedDealEqualityComparer()).ToList();

            var wallets = new Dictionary<string, WalletsList>();

            foreach (var item in list)
            {
                if (item.Owner == "0xaeb5b69c6452574c83954d6b4cab89b50184f24d")
                {
                    Console.WriteLine();
                }
                var amount = Web3.Convert.FromWei(BigInteger.Parse(item.StartAmount));

                if (wallets.ContainsKey(item.Owner))
                {
                    wallets[item.Owner].Amount += amount;
                }
                else
                {
                    wallets[item.Owner] = new WalletsList()
                    {
                        Owner = item.Owner,
                        Amount = amount,   
                    };
                }
            }

            var result = wallets.Values.OrderByDescending(x => x.Amount).ToList();

            Lambda.Log($"Working time: {DateTime.UtcNow - start}, return list with {wallets.Count} count");
            return result;
        }

        public async Task<List<LockedDeal>> GetWallets()
        {
            var dbObject = new DbObject()
            {
                Name = "LDList",
                Data = ""
            };

            var result = await Lambda.Run("DBReader", JsonSerializer.Serialize(dbObject));

            var list = JsonSerializer.Deserialize<List<LockedDeal>>(result);

            return list;
        }
    }
}
