using MagazineSubscribers.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MagazineSubscribers.Business_Layer
{
    public class MagazineSubscriberProcess
    {
        /// <summary>
        /// Generates Token
        /// </summary>
        /// <returns>Token value</returns>
        public static string GenerateToken()
        {
            string token = string.Empty;

            try
            {
                string tokenList = MagazineStoreAPI.GetApiData("GetToken", null, null).Result;

                if (!string.IsNullOrEmpty(tokenList))
                {
                    dynamic result = JsonConvert.DeserializeObject(tokenList);

                    token = result.token;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while generating token: " + ex.Message);
            }

            return token;
        }

        /// <summary>
        /// Get all the categories from Magazine store API
        /// </summary>
        /// <param name="token">token value</param>
        /// <returns>categories</returns>
        public static List<string> GetAllCategory(string token)
        {
            List<string> lstCategory = new List<string>();

            try
            {
                var categoryList = MagazineStoreAPI.GetApiData("GetCategories", token, null).Result;

                if (categoryList != null)
                {
                    lstCategory = JObject.Parse(categoryList)["data"].Select(x => x.ToString()).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while fetching the categories: " + ex.Message);
            }

            return lstCategory;
        }

        /// <summary>
        /// Get list of magazines under each category
        /// </summary>
        /// <param name="lstCategory">list of categories</param>
        /// <param name="token">token value</param>
        /// <returns>Collection of magazine id and category</returns>
        public async static Task<Dictionary<int, string>> GetMagazinesofEachCategory(List<string> lstCategory, string token)
        {
            Dictionary<int, string> magCategory = new Dictionary<int, string>();

            var exceptions = new ConcurrentQueue<Exception>();

            try
            {
                Parallel.ForEach(lstCategory, (category) =>
                {
                    try
                    {
                        var categoryMagazineList = MagazineStoreAPI.GetApiData("GetMagazinesCategory", token, category).Result;

                        if (categoryMagazineList != null)
                        {
                            foreach (var dt in JObject.Parse(categoryMagazineList)["data"])
                            {
                                magCategory.Add(Convert.ToInt32(dt["id"]), dt["category"].ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        exceptions.Enqueue(e);
                    }
                });

                if (exceptions.Count > 0)
                    throw new AggregateException(exceptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while fetching the magazine id of each category: " + ex.Message);
            }

            return magCategory;
        }

        /// <summary>
        /// Get all the subscribers and the subscribed magazines
        /// </summary>
        /// <param name="token">token value</param>
        /// <returns>subscribers list</returns>
        public async static Task<string> GetAllSubsribers(string token)
        {
            string magSubscriberDetails = string.Empty;
            try
            {
                magSubscriberDetails = MagazineStoreAPI.GetApiData("GetSubscribers", token, null).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while fetching the list of subscribers: " + ex.Message);
            }

            return magSubscriberDetails;
        }

        /// <summary>
        /// Validates the matching subscriber who had subscribed magazines in all categories and post the answer to API
        /// </summary>
        /// <param name="magCategory">List of Magazines and categories</param>
        /// <param name="magSubscriberDetails">Subscriber details</param>
        /// <param name="totalCategoryCount">Total number of categories</param>
        /// <param name="token">token value</param>
        /// <returns>response from POST API</returns>
        public static string ValidateAndPostCommonCategorySubscribers(Dictionary<int, string> magCategory, string magSubscriberDetails, int totalCategoryCount, string token)
        {
            HashSet<string> subscribers = new HashSet<string>();
            string jsonSubscribers = string.Empty;
            string response = string.Empty;
            var exceptions = new ConcurrentQueue<Exception>();

            try
            {
                Parallel.ForEach(JObject.Parse(magSubscriberDetails)["data"], (dataVal) =>
                {
                    try
                    {
                        HashSet<string> uniqueCategories = new HashSet<string>();

                        Parallel.ForEach(dataVal["magazineIds"], (dtVal) =>
                        {
                            uniqueCategories.Add(magCategory[Convert.ToInt32(dtVal)]);
                        });

                        if (uniqueCategories.Count == totalCategoryCount)
                        {
                            subscribers.Add(dataVal["id"].ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        exceptions.Enqueue(e);
                    }

                });

                if (exceptions.Count > 0)
                    throw new AggregateException(exceptions);

                jsonSubscribers = JsonConvert.SerializeObject(new { subscribers });

                var uniqstringContent = new StringContent(jsonSubscribers, UnicodeEncoding.UTF8, "application/json");

                response = MagazineStoreAPI.PostApiData(token, uniqstringContent).Result;

            }
            catch(Exception ex)
            {
                Console.WriteLine("Error occured while validating the common subscribers: " + ex.Message);
            }

            return response;
        }

    }
}
