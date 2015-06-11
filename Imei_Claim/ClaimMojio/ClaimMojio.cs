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

namespace Imei_Claim.ClaimMojio
{
    class ClaimMojio
    {
        public static void ClaimMojios(MojioClient client, string fileName)
        {
            Logger logger = Logger.Instance;
            logger.Log("Claiming Mojio Process Starting. . .");
            logger.Log("Filename: " + fileName);
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
                            var arry = row.Split(',', (char)34, (char)92); // Delimiter: ',', '/', '"'
                            arry = arry.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            var imei = arry[1]; //Imei
                            var regex = new Regex(@"^[0-9]+$");

                            if (regex.IsMatch(imei))
                            {
                                ClaimMojioByImei(client, imei);
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

        public static void ClaimMojioByImei(MojioClient client, string imei)
        {
            Logger logger = Logger.Instance;
            var regex = new Regex(@"^[0-9]+$");

            if (!regex.IsMatch(imei))
            {
                logger.Log(String.Format("{0} is not an IMEI number", imei));
            }
            try
            {
                // Claiming Mojio using C# SDK
                // NOTE: This is a restricted call and your developer account must be authorized
                var task = client.ClaimAsync(imei);
                var mojioResult = task.Result;
                var statusCode = mojioResult.StatusCode;
                var errorMessage = mojioResult.ErrorMessage;
                switch (statusCode)
                {
                    case HttpStatusCode.NotFound:
                        logger.LogErrorClaim(imei, errorMessage);
                        break;
                    case HttpStatusCode.Conflict:
                        logger.LogErrorClaim(imei, errorMessage);
                        break;
                    case HttpStatusCode.OK:
                        logger.LogSuccessfulClaim(imei);
                        break;
                    default:
                        logger.LogErrorClaim(imei, errorMessage);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(string.Format("An exception occured while claiming Mojio\nException Message: {0}",
                    ex.Message));
            }
        }
    }
}
