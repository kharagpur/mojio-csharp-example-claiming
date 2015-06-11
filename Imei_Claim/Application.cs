using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Imei_Claim.Logging;
using Mojio;
using Mojio.Client;

namespace Imei_Claim
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // APP INFO
            Guid appId = new Guid("00000000-0000-0000-0000-000000000000"); // Application Id
            Guid secretKey = new Guid("00000000-0000-0000-0000-000000000000"); // Sandbox
            //Guid secretKey = new Guid("00000000-0000-0000-0000-000000000000"); // Live

            // LOG-IN CREDENTIAL
            // USER ACCOUNT YOU WANT TO CLAIM/UN-CLAIM UNDER
            var adminUserOrEmail = "yourEmail@email.com";
            var adminPassword = "yourPassword";

            // SET UP
            var logger = Logger.Instance;

            // AUTHENTICATING TO MOJIO
            var client = new MojioClient(appId, secretKey, adminUserOrEmail, adminPassword);
            logger.Log("Aquiring Token From Mojio....");

            if (client.Token == null)
            {
                logger.Log("Failed to aquire token.");
                logger.Log("Press any key to quit...");
                Console.ReadKey();
            }
            else
            {
                logger.Log("Log In: Successful");
            }

            // GET FILE PATH
            var fileName = GetFilePathFromUser();

            // PROCESS FILE AND CLAIM MOJIO
            ClaimMojio.ClaimMojio.ClaimMojios(client, fileName);

            // PROCESS FILE AND UNCLAIM MOJIO
            UnClaimingMojio.UnclaimMojio.UnclaimMojios(client, fileName);

            // PROCESS FILE AND ASSIGN MOJIO TO NEW USER
            CreateAndAssignUserToUnclaimedMojio(client, fileName);

            // END OF PROGRAM
            logger.Log("Press any key to stop...");
            Console.ReadKey();
        }

        private static string GetFilePathFromUser()
        {
            var fileName = string.Empty;
            var logger = Logger.Instance;
            var isValid = false;

            while (!isValid)
            {
                logger.Log("Please enter the file path of your CSV file of IMEI you want to claim");

                fileName = Console.ReadLine(); // THIS CAN BE HARD CODED
                if (File.Exists(fileName))
                {
                    var extension = Path.GetExtension(fileName);
                    if (extension != null && extension.Contains("csv"))
                    {
                        isValid = true;
                    }
                    else
                    {
                        logger.Log("Error: Please select \"csv\" file.");
                    }
                }
                else
                {
                    logger.Log("Error: Invalid File Path.");
                }
            }
            return fileName;
        }

        private static void CreateAndAssignUserToUnclaimedMojio(MojioClient client, string fileName)
        {
            // ASSUMPTION: THE IMEI IN THE FILE ARE ALL UNCLAIMED

            // NEW USER ACCOUNT CREATION PATTERN
            var newUserNameFormat = "UserName"; //expected: wizardUser0, wizardUser1 ... etc.
            var newUserPasswordFormat = "Us3rpaSsw0rd"; // expected: wIzpaSsw0rd0, wIzpaSsw0rd1 ... etc.
            var newUserEmailFormat = "@youemail.com"; // expected: wizardUser0@thecarwizard.net, wizardUser1@thecarwizard.net ... etc.
            var currentUserNumber = 0;
            var totalImei = GetTotalImei(fileName); // Number of user account needs to be created
            Logger logger = Logger.Instance;
            
            List<User> newUserList = CreateUserAccounts(client, newUserNameFormat, newUserPasswordFormat, newUserEmailFormat,
                totalImei);

            logger.Log("Assigning User To Mojio Starting. . .");
            try
            {
                using (var reader = new StreamReader(fileName))
                {
                    while (!reader.EndOfStream)
                    {
                        var row = reader.ReadLine();

                        // EXPECTED ROW: "TV156403609,\"123456789012345\""
                        // NOTE: if this row looks different than the string above please change the row.Split accordingly
                        if (row != null)
                        {
                            var arry = row.Split(',', (char) 34, (char) 92); // Delimiter: ',', '/', '"'
                            arry = arry.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            var imei = arry[1]; //Imei
                            var regex = new Regex(@"^[0-9]+$");

                            if (regex.IsMatch(imei))
                            {
                                var user = newUserList[currentUserNumber];
                                var task = client.SetUserAsync(user.Email, newUserPasswordFormat + currentUserNumber);
                                var result = task.Result;
                                if (result.Data != null)
                                {
                                    ClaimMojio.ClaimMojio.ClaimMojioByImei(client, imei);
                                    currentUserNumber++;
                                }
                            }
                            else
                            {
                                logger.Log(String.Format("{0} is not an IMEI number", imei));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(string.Format("An exception occured while claiming Mojio\nException Message: {0}",
                    ex.Message));
            }
        }

        private static int GetTotalImei(string fileName)
        {
            if (fileName!=string.Empty)
            {
                try
                {
                    using (StreamReader r = new StreamReader(fileName))
                    {
                        int count = 0;
                        while (r.ReadLine() != null)
                        {
                            count++;
                        }
                        return count;
                    }
                }
                catch (Exception)
                {
                    //eat it
                }
            }
            return 0;
        }

        private static List<User> CreateUserAccounts(MojioClient client, string newUserNameFormat, string newUserPasswordFormat,
            string newUserEmailFormat, int numberOfAccount)
        {
            Logger logger = Logger.Instance;
            logger.Log("Creating User Accounts Starting . . .");
            var newUserList = new List<User>();
            for (int currentUserNumber = 0; currentUserNumber < numberOfAccount; currentUserNumber++)
            {
                // Create a new user
                var userName = newUserNameFormat + currentUserNumber;
                var userPassword = newUserPasswordFormat + currentUserNumber;
                var userEmail = userName + newUserEmailFormat;
                var user = Accounts.MojioAccount.CreateNewUser(client, userName, userPassword, userEmail);
                if (user != null)
                {
                    newUserList.Add(user);
                    logger.logSuccessfulNewUser(user);
                }
                else
                {
                    logger.LogErrorNewUser(userName);
                }
            }
            logger.Log(string.Format("Created {0} user accounts", newUserList.Count));
            return newUserList;
        }
    }
}