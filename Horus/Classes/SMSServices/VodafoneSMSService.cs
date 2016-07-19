using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horus.Classes
{
    public class VodafoneSMSService : AbstractSMSService
    {
        public VodafoneSMSService()
        {

        }

        public VodafoneSMSService(string Username, string MobileNumber)
        {
            base.userName = Username;
            base.mobileNumber = MobileNumber;
        }
    }
}
