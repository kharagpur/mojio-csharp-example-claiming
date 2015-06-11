using System;
using System.Diagnostics.Contracts;
using Mojio;

namespace Imei_Claim.Logging
{
    class Logger
    {
        // MEMBER VARIABLE
        private static Logger m_instance = null;
        private static readonly object LOCK = new object();

        #region PROPERTIES

        public static Logger Instance
        {
            get
            {
                lock (LOCK)
                {
                    if (m_instance == null)
                    {
                        m_instance = new Logger();
                    }
                    return m_instance;
                }
            }
        }

        #endregion PROPERTIES

        #region PRIVATE_METHODS

        private Logger()
        {
            // To Implement
        }

        #endregion PRIVATE_METHODS

        #region PUBLIC_METHODS

        public void Log(string str)
        {
            Console.WriteLine(str);
        }

        public void LineBreak()
        {
            Console.WriteLine(Environment.NewLine);
        }

        public void LogSuccessfulClaim(string imei)
        {
            Console.WriteLine(imei + " Successfully claimed");
        }

        public void LogErrorClaim(string imei, string errMsg)
        {
            Console.WriteLine("**Error**: {0} {1}", imei, errMsg);
        }

        public void LogSuccessfulUnclaim(string imei)
        {
            Console.WriteLine(imei + " Successfully Unclaimed");
        }

        public void LogErrorUnclaim(string imei, string errMsg)
        {
            Console.WriteLine("**Error**: {0} {1}", imei, errMsg);
        }

        public void logSuccessfulNewUser(User user)
        {
            Console.WriteLine(user.UserName + " Successfully Created");
        }

        public void LogErrorNewUser(string userName)
        {
            Console.WriteLine("**Error**: {0} {1}", userName, "Failed to be created");
        }
        #endregion PUBLIC_METHODS
    }
}
