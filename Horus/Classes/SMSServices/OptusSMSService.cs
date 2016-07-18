using Horus.OptusSMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horus.Classes
{
    public class OptusSMSService : AbstractSMSService
    {
        //optus service server if required
        private ISOAPServer soapServer = new OptusSMS.SOAPServerClient();

        public OptusSMSService()
        {
            base.serviceSecret = "7C4A1A5E-D630-412D-000A-375056AA9646";
        }

        public override string sendSMS()
        {
            //TODO: Review this section, not working with updated Service Reference

            //some of the values passed here are just temporary values for testing purposes
            //return soapServer.SendTextSMS(base.serviceSecret, base.serviceKey, base.mobileNumber, base.message, "s3116979@student.rmit.edu.au", "s3116979", 1, "s3116979").ToString();
            return "";
        }
    }
}
