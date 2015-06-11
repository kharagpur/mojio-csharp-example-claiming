using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Imei_Claim.Logging;
using Mojio.Client;
using Newtonsoft.Json.Linq;

namespace Imei_Claim.UnClaimingMojio
{
    class UnclaimMojio
    {
        public static void UnclaimMojios(MojioClient client, string fileName)
        {
            Logger logger = Logger.Instance;
            logger.Log("Un-clamining Mojio Process Starting. . .");
            logger.Log("Filename: " + fileName);
            try
            {
                using (var reader = new StreamReader(fileName))
                {

                    var claimedMojioList = GetAllClaimdMojio(client);

                    while (!reader.EndOfStream)
                    {
                        var row = reader.ReadLine();
                        // todo: fake some values here
                        // EXPECTED ROW: "TV156403609,\"123456789012345\""
                        // NOTE: if this row looks different than the string above please change the row.Split accordingly
                        if (row != null)
                        {
                            var arry = row.Split(',', (char)34, (char)92); // Delimiter: ',', '/', '"'
                            arry = arry.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            var imei = arry[1]; //Imei
                            var regex = new Regex(@"^[0-9]+$");

                            if (regex.IsMatch(imei))
                            {
                                // Get Mojio
                                var mojio = GetMojio(imei, claimedMojioList);

                                if (mojio != null)
                                {
                                    UnclaimMojios(client, mojio);
                                }
                                else
                                {
                                    logger.Log("Could not find Mojio with IMEI: " + imei);
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
            catch (Exception)
            {
                //eat it
            }
        }

        public static void UnclaimMojiosByImei(MojioClient client, string imei)
        {
            Logger logger = Logger.Instance;
            var mojio = GetMojio(imei, GetAllClaimdMojio(client));
            if (mojio != null)
            {
                UnclaimMojios(client, mojio);
            }
            else
            {
                logger.Log("Could not find Mojio with IMEI: " + imei);
            }
        }

        public static void UnclaimMojios(MojioClient client, Mojio.Mojio mojio)
        {
            Logger logger = Logger.Instance;

            // Unclaiming Mojio using our C# SDK
            // NOTE: This is a restricted call and your developer account must be authorized
            var response = client.UnclaimAsync(mojio.Id);
            var result = response.Result;
            var statusCode = result.StatusCode;
            var errorMessage = result.ErrorMessage;
            switch (statusCode)
            {
                case HttpStatusCode.NotFound:
                    logger.LogErrorUnclaim(mojio.Imei, errorMessage);
                    break;
                case HttpStatusCode.Conflict:
                    logger.LogErrorUnclaim(mojio.Imei, errorMessage);
                    break;
                case HttpStatusCode.OK:
                    logger.LogSuccessfulUnclaim(mojio.Imei);
                    break;
                default:
                    logger.LogErrorUnclaim(mojio.Imei, errorMessage);
                    break;
            }
        }

        private static List<Mojio.Mojio> GetAllClaimdMojio(MojioClient client)
        {
            // Get all the Mojios under the currented logged on user
            var mojiosResponse = client.GetAsync<Mojio.Mojio>();
            var result = mojiosResponse.Result;
            var data = result.Data;
            var mojios = data.Data;

            // Store all the Mojios
            return new List<Mojio.Mojio>(mojios);
        }

        private static Mojio.Mojio GetMojio(string imei, List<Mojio.Mojio> mojioList)
        {
            return mojioList.FirstOrDefault(mojio => mojio.Imei == imei);
        }
    }
}
