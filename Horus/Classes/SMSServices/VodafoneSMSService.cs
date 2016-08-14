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

        public VodafoneSMSService(string MobileNumber)
        {
            base.userName = "HORUS Security";
            base.mobileNumber = MobileNumber;
        }
    }
}
