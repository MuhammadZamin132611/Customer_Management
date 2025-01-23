using LearnAPI.Repos;
using LearnAPI.Repos.Models;
using LearnAPI.Service;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace LearnAPI.Container
{
    public class RefreshHandler : IRefreshHandler
    {
        private readonly LearndataContext context;

        public RefreshHandler(LearndataContext context)
        {
            this.context = context;
        }
        public async Task<string> GenerateToken(string username)
        {
            var randomnumber = new byte[32];
            using (var randomnumbergenerator = RandomNumberGenerator.Create())
            {
                randomnumbergenerator.GetBytes(randomnumber);
                string refreshtoken = Convert.ToBase64String(randomnumber);
                var ExistingToken = await this.context.TblRefreshtokens.FirstOrDefaultAsync(item 
                    => item.UserId == username);
                if (ExistingToken != null)
                {
                    ExistingToken.Refreshtoken = refreshtoken;
                    this.context.TblRefreshtokens.Update(ExistingToken);
                    await this.context.SaveChangesAsync();
                }
                else
                {
                    await this.context.TblRefreshtokens.AddAsync(new TblRefreshtoken
                    {
                        UserId = username,
                        Tokenid = new Random().Next().ToString(),
                        Refreshtoken = refreshtoken
                    });
                    //await this.context.TblRefreshtokens.AddAsync(newtoken);
                    //await this.context.SaveChangesAsync();
                }
                await this.context.SaveChangesAsync();

                return refreshtoken;
            }
        }
    }
}
