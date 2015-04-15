using System;
using System.Text;
using Crestron.SimplSharp;              // For Basic SIMPL# Classes
using Crestron.SimplSharp.Net.Https;    // For access to HTTPS
using Crestron.SimplSharp.Net;          // For access to HTTPS
using Newtonsoft.Json;

namespace Wink
{
    public class WinkCommandProcessor
    {      
        public WinkCommandProcessor()
        { }

        public string str2JSON(string urlString)
        {
            urlString = urlString.StartsWith("?") ? urlString.Substring(1) : urlString;
            string resultString = "{";

            string[] urlKeyVal = urlString.Split('&');

            foreach (string kv in urlKeyVal)
            {
                string k = kv.Substring(0, kv.LastIndexOf('='));
                string v = kv.Substring(kv.LastIndexOf('=') + 1);
                resultString += "\"" + k + "\"" + ":" + "\"" + v + "\",";
            }
            resultString = resultString.Substring(0, resultString.Length - 1);
            resultString += "}";


            return resultString;
        }

        public string Authenticate(string client_id,string client_secret, string username, string password)
        {
            if(username != "" && password != "")
            {
                HttpsClient client = new HttpsClient();
                client.PeerVerification = false;
                client.HostVerification = false;
                client.Verbose = false;

                HttpsClientRequest request = new HttpsClientRequest();
                HttpsClientResponse response;
                //String url = "https://private-714ad-wink.apiary-mock.com/oauth2/token";
                String url = "https://winkapi.quirky.com/oauth2/token";
                
                try
                {
                    request.KeepAlive = true;
                    request.Url.Parse(url);
                    request.RequestType = Crestron.SimplSharp.Net.Https.RequestType.Post;
                    request.Header.SetHeaderValue("Content-Type", "application/json");
                    request.ContentString = "";
                    request.ContentString = str2JSON("?client_id=" + client_id + "&" + "client_secret=" + client_secret + "&" + "username=" + username + "&" + "password=" + password + "&" + "grant_type=" + "password");
                    
                    // Dispatch will actually make the request with the server
                    response = client.Dispatch(request);

                    if (response.Code >= 200 && response.Code < 300)
                    {
                        // A response code between 200 and 300 means it was successful.
                        //ErrorLog.Notice(response.ContentString.ToString());

                        string[] subStrings = response.ContentString.ToString().Split(',');

                        foreach (string str in subStrings)
                        {
                            if (str.Contains("access_token"))
                            {
                                string result = str.Substring(25, str.Length - 26);
                                return result;
                            }
                            else
                            {
                                ErrorLog.Notice("Not Found\n");
                            }
                        }
                  

                    }
                    else
                    {
                        // A reponse code outside this range means the server threw an error.
                        ErrorLog.Notice("Wink https response code: " + response.Code);
                        return "";
                    }
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Exception in Wink: " + e.ToString());
                    return "";
                }
                return "";
            }
            return "";
        }



        public ushort GetDevices(string access_code)
        {
            if (access_code == null)
            {
                // can't send without these fields being set.
                return 0;
            }
            else
            {
                HttpsClient client = new HttpsClient();
                //client.UserName = AccountSID;
                //client.Password = AuthToken;
                client.Verbose = false;

                /*
                 * The PeerVerification option determines whether our HTTPS class verifies the authenticity of 
                 * the peer's certificate. 
                 * 
                 * WARNING: disabling verification of the certificate allows bad guys to man-in-the-middle the
                 * communication without you knowing it. Disabling verification makes the communication insecure.
                 * Just having encryption on a transfer is not enough as you cannot be sure that you are 
                 * communicating with the correct end-point.
                 */
                client.PeerVerification = false;

                /*
                 * The HostVerification option determines whether our HTTPS verifies that the server cert is for
                 * the server it is known as.
                 */
                client.HostVerification = false;

                HttpsClientRequest request = new HttpsClientRequest();
                HttpsClientResponse response;
                String url = "https://winkapi.quirky.com/users/me/wink_devices";//SMS/Messages.json

                try
                {
                    request.KeepAlive = true;
                    request.Url.Parse(url);
                    request.RequestType = Crestron.SimplSharp.Net.Https.RequestType.Get;
                    request.Header.SetHeaderValue("Content-Type", "application/json");
                    request.Header.SetHeaderValue("Authorization", "Bearer " + access_code);
                    request.ContentString = "";
                    

                    // Dispatch will actually make the request with the server
                    response = client.Dispatch(request);

                    if (response.Code >= 200 && response.Code < 300)
                    {
                        // A response code between 200 and 300 means it was successful.
                        //ErrorLog.Notice(response.ContentString.ToString());
                        string[] subStrings = response.ContentString.ToString().Split(',');

                        foreach (string str in subStrings)
                        {
                           ErrorLog.Notice(str);
                        }

                        return 1;
                    }
                    else
                    {
                        // A reponse code outside this range means the server threw an error.
                        ErrorLog.Notice("Wink https response code: " + response.Code);
                        return 0;
                    }
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Exception in Wink: " + e.ToString());
                    return 0;
                }
            }
        }

        public string getLightBulbState(string access_code, string deviceID)
        {
            HttpsClient client = new HttpsClient();
            client.Verbose = false;
            client.PeerVerification = false;
            client.HostVerification = false;

            HttpsClientRequest request = new HttpsClientRequest();
            HttpsClientResponse response;
            String url = "https://winkapi.quirky.com/light_bulbs/" + deviceID;//SMS/Messages.json
            request.KeepAlive = true;
            request.Url.Parse(url);
            request.RequestType = Crestron.SimplSharp.Net.Https.RequestType.Get;
            request.Header.SetHeaderValue("Content-Type", "application/json");
            request.Header.SetHeaderValue("Authorization", "Bearer " + access_code);
            request.ContentString = "";
            response = client.Dispatch(request);
            ushort lr = 0;
            if (response.Code >= 200 && response.Code < 300)
            {
                //ErrorLog.Notice(response.ContentString.ToString());
                string[] subStrings = response.ContentString.ToString().Split(',');

                foreach (string str in subStrings)
                {
                    if(str.Contains("last_reading"))
                    {
                        lr = 1;
                    }
                    if (lr == 1)
                    {
                        if (str.Contains("{"))
                        {
                            string result = str.Substring(26, str.Length - 26);
                            //ErrorLog.Notice(result);
                            if (result == "false")
                            {
                                return "off";
                            }
                            else if (result == "true")
                            {
                                return "on";
                            }
                        }
                    }
                    if (str.Contains("connection"))
                    {
                        lr = 0;
                    }
                }
            }
            else
            {
                // A reponse code outside this range means the server threw an error.
                ErrorLog.Notice("Wink https response code: " + response.Code);
            }
            return "";
        }

        public void setLightBulbState(string access_code, string deviceID, string state)
        {
            HttpsClient client = new HttpsClient();
            client.Verbose = false;
            client.PeerVerification = false;
            client.HostVerification = false;
            //Testing
            //client.AllowAutoRedirect = true;

            HttpsClientRequest request = new HttpsClientRequest();
            HttpsClientResponse response;
            String url = "https://winkapi.quirky.com/light_bulbs/" + deviceID;//SMS/Messages.json
            //String url = "https://private-714ad-wink.apiary-mock.com/light_bulbs/" + deviceID;
            request.KeepAlive = true;



            request.Url.Parse(url);
            request.RequestType = Crestron.SimplSharp.Net.Https.RequestType.Put;
            request.Header.SetHeaderValue("Content-Type", "application/json");
            request.Header.SetHeaderValue("Authorization", "Bearer " + access_code);
            request.Header.SetHeaderValue("Expect", "");
            //string command = "{\n    \"name\":\"My Device\",\n}";
            string command = "{\n    \"desired_state\": {\n        \"powered\":" + state + "\n    }\n}";
            request.ContentString = command;
            request.Header.SetHeaderValue("Content-Length",command.Length.ToString());
            request.Header.SetHeaderValue("transfer-encoding", "");
                   
            response = client.Dispatch(request);
            if (response.Code >= 200 && response.Code < 300)
            {
                //ErrorLog.Notice("Wink https response code: " + response.Code);
                //ErrorLog.Notice(response.ContentString.ToString() + "\n");                
            }
            else
            {
                // A reponse code outside this range means the server threw an error.
                ErrorLog.Notice("Wink https response code: " + response.Code);
            }
        }
    }
}