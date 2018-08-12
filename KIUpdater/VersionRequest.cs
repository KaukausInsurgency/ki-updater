using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KIUpdater
{
    class VersionRequest
    {
        public VersionResponse Response { private set; get; }

        public VersionRequest(string uri)
        {
            Response = Get(uri);
        }

        private VersionResponse Get(string uri)
        {
            var client = new RestSharp.RestClient(uri);
            var request = new RestSharp.RestRequest("api/version");
            var response = client.Execute<VersionResponse>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                if (response.StatusCode == 0)
                    Console.WriteLine("Error communicating with server - unable to connect to server");
                else
                    Console.WriteLine("Error communicating with server (status code: " + response.StatusCode + ")");
                
                return null;
            }
            else
            {
                return client.Execute<VersionResponse>(request).Data;
            }
            
        }
    }

    public class VersionResponse
    {
        public string Version { get; set; }
        public string GUID { get; set; }
        public string DownloadURL { get; set; }
    }
}
