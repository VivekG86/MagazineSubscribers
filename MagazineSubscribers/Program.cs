using MagazineSubscribers.Business_Layer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MagazineSubscribers
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> lstCategory = new List<string>();
            Dictionary<int, string> magCategory = new Dictionary<int, string>();
            HashSet<string> subscribers = new HashSet<string>();
            string token = string.Empty;
            string jsonSubscribers = string.Empty;
            string magSubscriberDetails = string.Empty;
            int totalCategoryCount = 0;

            try
            {
                // Generate Tokens

                token = MagazineSubscriberProcess.GenerateToken();

                if (!string.IsNullOrEmpty(token))
                {
                    // Get all Categories                 

                    lstCategory = MagazineSubscriberProcess.GetAllCategory(token);

                    if (lstCategory != null && lstCategory.Count() > 0)
                    {
                        totalCategoryCount = lstCategory.Count();

                        // Get all Categories and Magazines within each Category               

                        magCategory = MagazineSubscriberProcess.GetMagazinesofEachCategory(lstCategory, token).GetAwaiter().GetResult();

                        // Get Subscribers and Magazine details

                        magSubscriberDetails = MagazineSubscriberProcess.GetAllSubsribers(token).GetAwaiter().GetResult();

                        // Check if the subscriber has magazines in all the categories and post result to API

                        if (magCategory != null && magCategory.Count > 0 && !string.IsNullOrEmpty(magSubscriberDetails))
                        {
                            var respone = MagazineSubscriberProcess.ValidateAndPostCommonCategorySubscribers(magCategory, magSubscriberDetails, totalCategoryCount, token);
                            
                            Console.WriteLine(respone);
                        }
                        else
                        {
                            Console.WriteLine("Invalid data for Subscribers and Magazines");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No categories were found");
                    }
                }
                else
                {
                    Console.WriteLine("Token not generated");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occured in Main method: " + ex.Message);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
