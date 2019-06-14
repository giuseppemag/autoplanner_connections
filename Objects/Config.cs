using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RestSharp;

namespace AutoplannerConnections
{
    class Config
    {
        public string twSecretBase;
        public string twAccessToken;
        public string twRefreshToken;
    }
}