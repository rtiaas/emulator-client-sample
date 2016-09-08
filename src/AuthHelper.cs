namespace EmulatorClientSample
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;

    public class AuthHelper
    {
        private string _authServiceUri;

        public AuthHelper(string authServiceUri)
        {
            _authServiceUri = authServiceUri;
        }

        public AuthToken GetAuthToken(string clientId, string clientSecret, string scopes)
        {
            var authtoken = $"{clientId}:{clientSecret}";
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(authtoken));

            var authHeader = new AuthenticationHeaderValue("Basic", encoded);

            var msg = new HttpRequestMessage(HttpMethod.Post, _authServiceUri);
            msg.Headers.Authorization = authHeader;

            var formData = new List<KeyValuePair<string, string>>();
            formData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            formData.Add(new KeyValuePair<string, string>("scope", scopes));

            msg.Content = new FormUrlEncodedContent(formData);
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var client = new HttpClient();
            var result = client.SendAsync(msg).Result;

            if (!result.IsSuccessStatusCode)
            {
                var errmsg = result.Content.ReadAsStringAsync().Result;
                throw new Exception($"Failed to obtain auth token. Error {errmsg}.  Response Code: {result.StatusCode}");
            }

            var response = result.Content.ReadAsStringAsync().Result;

            return AuthToken.FromJwt(response);
        }
    }
}
