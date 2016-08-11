using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horus.Classes
{
    public class AbstractSMSService : SMSService
    {
        protected string serviceKey;
        protected string serviceSecret;
        protected string message = "HORUS Security: User verification failed. Please review snapshot uploaded to your Google Drive account.";
        protected string userName;
        protected string mobileNumber;

        public virtual string sendSMS()
        {
            throw new NotImplementedException();
        }

        public void setKey(string key)
        {
            this.serviceKey = key;
        }

        public void setSecret(string secret)
        {
            this.serviceSecret = secret;
        }

        public void setMobileNumber(string mobileNumber)
        {
            this.mobileNumber = mobileNumber;
        }

        public void setUserName(string userName)
        {
            this.userName = userName;
        }

        public void setMessage(string message)
        {
            this.message = message;
        }
    }
}
