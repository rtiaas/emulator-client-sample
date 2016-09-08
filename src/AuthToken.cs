namespace EmulatorClientSample
{
    using Newtonsoft.Json.Linq;

    public class AuthToken
    {
        public string AccessToken { get; set; }

        public string TokenType { get; set; }

        public int ExpiresIn { get; set; }

        public static AuthToken FromJwt(string jwt)
        {
            var token = JObject.Parse(jwt);

            return new AuthToken
            {
                AccessToken = token.Value<string>("access_token"),
                TokenType = token.Value<string>("token_type"),
                ExpiresIn = token.Value<int>("expires_in")
            };
        }
    }
}
