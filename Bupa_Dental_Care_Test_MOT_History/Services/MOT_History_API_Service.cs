using System.Net.Http;
using Bupa_Dental_Care_Test_MOT_History.Models;
using Newtonsoft.Json;
using static Bupa_Dental_Care_Test_MOT_History.Components.Pages.Home;

namespace Bupa_Dental_Care_Test_MOT_History.Services
{
    public class MOT_History_API_Service
    {
        private HttpClient _httpClient;
        private readonly string _apiKey;
        public MOT_History_API_Service(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _apiKey = config["api-key"].ToString();
        }


        public async Task<MOTHistoryViewModel> GetVehicleDetails(string reg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://beta.check-mot.service.gov.uk/trade/vehicles/mot-tests?registration={reg}");
            request.Headers.Add("x-api-key", _apiKey);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch(Exception ex)
            {
                return new MOTHistoryViewModel
                {
                    ErrorMessage = $"Error fetching MOT history: {ex.Message}"
                };
            }
            
            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return await ReturnRegNotFound(response);
            }

            if (!response.IsSuccessStatusCode) 
            {
                return new MOTHistoryViewModel()
                {
                    ErrorMessage = $"Error fetching MOT history: status code: {response.StatusCode}"
                };
            
            }

            
            return await ReturnSuccessfulRegistration(response);
        }

        public async Task<MOTHistoryViewModel> ReturnRegNotFound(HttpResponseMessage response)
        {

            var error_content = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<MOTHistoryViewModel>(error_content);
                return errorResponse;
            }
            catch (Exception ex)
            {
                return new MOTHistoryViewModel
                {
                    ErrorMessage = "Vehicle not found and failed to provide response"
                };
            }
        }

        public async Task<MOTHistoryViewModel> ReturnSuccessfulRegistration(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var data = JsonConvert.DeserializeObject<List<VehicleDetails>>(content);
                if (data == null)
                {
                    return new MOTHistoryViewModel
                    {
                        ErrorMessage = "No MOT data found"
                    };
                }
                

                return new MOTHistoryViewModel
                {
                    Registration = data.First().registration,
                    Make = data.First().make,
                    Model = data.First().model,
                    Colour = data.First().primaryColour,
                    Mileage = data.First().motTests.First().odometerValue,
                    Odometer_Unit = data.First().motTests.First().odometerUnit,
                    ExpiryDate = data.First().motTests.Where(x => !string.IsNullOrEmpty(x.expiryDate)).FirstOrDefault()?.expiryDate //if MOT fails then the expiration data is not populated unless MOT has passed.

                };
            }
            catch (Exception ex)
            {
                return new MOTHistoryViewModel()
                {
                    ErrorMessage = $"Error deserializing MOT details: {ex.Message}"
                };
            }

        }
    }
}
