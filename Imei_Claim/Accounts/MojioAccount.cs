using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojio;
using Mojio.Client;

namespace Imei_Claim.Accounts
{
    class MojioAccount
    {
        public static User CreateNewUser(MojioClient client, string userName, string password, string email)
        {
            // Create a new Mojio user
            var task = client.RegisterUserAsync(userName, email, password);
            var result = task.Result;
            var mojioUser = result.Data;
            return mojioUser;
        }
    }
}
