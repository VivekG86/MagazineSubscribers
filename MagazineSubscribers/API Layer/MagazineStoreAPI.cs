using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace MagazineSubscribers.API
{
    public class MagazineStoreAPI
    {
        /// <summary>
        /// Get URL and Endpoint for all the API calls
        /// </summary>
        /// <param name="apiName">Endpoint name</param>
        /// <param name="token">token value</param>
        /// <param name="category">category if applicable</param>
        /// <returns>Uri value</returns>
        private static Uri GetEndpointForAPI(string apiName, string token, string category)
        {
            Uri apiUri = null;

            switch (apiName)
            {
                case "GetToken":
                    apiUri = new Uri(new Uri(ConfigurationManager.AppSettings["url"]), new Uri(ConfigurationManager.AppSettings["GetToken"], UriKind.Relative));
                    break;
                case "GetCategories":
                    apiUri = new Uri(new Uri(ConfigurationManager.AppSettings["url"]), new Uri(string.Format("{0}/{1}", ConfigurationManager.AppSettings["GetCategories"], token), UriKind.Relative));
                    break;
                case "GetMagazinesCategory":
                    apiUri = new Uri(new Uri(ConfigurationManager.AppSettings["url"]), new Uri(string.Format("{0}/{1}/{2}", ConfigurationManager.AppSettings["GetMagazinesCategory"], token, category), UriKind.Relative));
                    break;
                case "GetSubscribers":
                    apiUri = new Uri(new Uri(ConfigurationManager.AppSettings["url"]), new Uri(string.Format("{0}/{1}", ConfigurationManager.AppSettings["GetSubscribers"], token), UriKind.Relative));
                    break;
                case "PostAnswer":
                    apiUri = new Uri(new Uri(ConfigurationManager.AppSettings["url"]), new Uri(string.Format("{0}/{1}", ConfigurationManager.AppSettings["PostAnswer"], token), UriKind.Relative));
                    break;
            }
            return apiUri;
        }

        /// <summary>
        /// Calls the Magazine Store API endpoints 
        /// </summary>
        /// <param name="apiUri">Contains url with corresponding endpoint</param>
        /// <returns>value from API</returns>
        public async static Task<string> GetApiData(string apiName, string token, string category)
        {
            string result = string.Empty;
            try
            {
                Uri apiUri = GetEndpointForAPI(apiName, token, category);

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(apiUri);

                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Error Occured in API Call: {0}, the status code is: {1}", apiName, response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error Occured in API Call: {0}, Exception: {1}", apiName, ex.Message));
            }

            return result;

        }

        /// <summary>
        /// Posts data into the Magazine Store API
        /// </summary>
        /// <param name="apiUri">post api url</param>
        /// <param name="content">contains the subscriber ids</param>
        /// <returns>values from the API call</returns>
        public async static Task<string> PostApiData(string token, StringContent content)
        {
            HttpResponseMessage response = null;

            string result = string.Empty;

            try
            {
                Uri apiUri = GetEndpointForAPI("PostAnswer", token, null);

                using (var client = new HttpClient())
                {
                    response = await client.PostAsync(apiUri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Error Occured in POST API Call. The status code is: {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error Occured in POST API Call. Exception: {0}", ex.Message));
            }

            return result;
        }
    }
}
