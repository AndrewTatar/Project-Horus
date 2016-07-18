using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horus.Classes
{
    interface SMSService
    {
        string sendSMS();
        void setKey(string key);
        void setSecret(string secret);
        void setMobileNumber(string mobileNumber);
    }
}
