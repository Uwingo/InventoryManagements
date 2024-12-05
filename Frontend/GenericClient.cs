using System.Net.Http.Headers;

namespace Frontend
{
    public static class GenericClient
    {
        public static HttpClient Client { get; set; }
        //public static IConfiguration _configuration{get; }

        public static HttpClient InitializeClientBaseAddress(this IServiceCollection services, IConfiguration configuration)
        {
            var result = "http://localhost:5251/";
            Client = new HttpClient();

            Client.BaseAddress = new Uri(result.ToString());

            return Client;
        }
        public static string UriAddress(string uri)
        {
            Client = new HttpClient();
            string url = Client.BaseAddress + uri;
            return url;
        }
    }
}
