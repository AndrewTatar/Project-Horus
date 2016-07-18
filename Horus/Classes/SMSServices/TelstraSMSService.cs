using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Horus.Classes
{
    public class TelstraSMSService : AbstractSMSService
    {
        public TelstraSMSService()
        {
            base.serviceKey = "GoEiEpAOv0KPqe03kcV7AuX8FGyuRM5u";
            base.serviceSecret = "BzbZMIwfQrfQusa1";
        }

        public override string sendSMS()
        {
            Console.WriteLine("...sending sms...");
            return SendSms(GetAccessToken(), base.mobileNumber, base.message);
        }

        private string GetAccessToken()
        {
            string url = string.Format("https://api.telstra.com/v1/oauth/token?client_id={0}&client_secret={1}&grant_type=client_credentials&scope=SMS", base.serviceKey, base.serviceSecret);

            using (var webClient = new System.Net.WebClient())
            {
                var json = webClient.DownloadString(url);
                var obj = JObject.Parse(json);
                return obj.GetValue("access_token").ToString();
            }
        }

        private string SendSms(string token, string recipientNumber, string message)
        {
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    webClient.Headers.Clear();
                    webClient.Headers.Add(HttpRequestHeader.ContentType, @"application/json");
                    webClient.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);

                    string data = "{\"to\":\"" + recipientNumber + "\", \"body\":\"" + message + "\"}";
                    var response = webClient.UploadData("https://api.telstra.com/v1/sms/messages", "POST", Encoding.Default.GetBytes(data));
                    var responseString = Encoding.Default.GetString(response);
                    var obj = JObject.Parse(responseString);
                    return obj.GetValue("messageId").ToString();
                    
                    // Now parse with JSON.Net
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return string.Empty;
        }
    }
}
