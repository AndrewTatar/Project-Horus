using Horus.OptusWebServiceReference;


namespace Horus.Classes
{
    public class OptusSMSService : AbstractSMSService
    {
        //optus service server if required
        private ISOAPServer soapServer = new OptusWebServiceReference.SOAPServerClient();
        const string SERVICE_SECRET = "7C4A1A5E-D630-412D-000A-375056AA9646";

        public OptusSMSService()
        {
            base.serviceSecret = SERVICE_SECRET;
        }

        public OptusSMSService(string MobileNumber)
        {
            base.serviceSecret = SERVICE_SECRET;
            base.mobileNumber = MobileNumber;
            base.userName = "HORUS Security";
        }

        public override string sendSMS()
        {
            
            App.WriteMessage("Sending SMS Notification (Optus)");
            //some of the values passed here are just temporary values for testing purposes
            SendTextSMSRequest req = new SendTextSMSRequest(base.serviceSecret, base.serviceKey, base.mobileNumber, base.message, "s3116979@student.rmit.edu.au", base.userName, 1, "s3116979");
            return soapServer.SendTextSMS(req).ToString();
            
        }
    }
}
