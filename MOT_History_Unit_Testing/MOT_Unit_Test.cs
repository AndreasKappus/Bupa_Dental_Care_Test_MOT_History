using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;
using Bupa_Dental_Care_Test_MOT_History.Models;
using Microsoft.Extensions.Configuration;
using Bupa_Dental_Care_Test_MOT_History.Services;
using AngleSharp.Io;
using static System.Net.Mime.MediaTypeNames;

namespace MOT_History_Unit_Testing
{
    public class MOTWebAppUnitTest
    {
        [Fact]
        public async Task ReturnValidRegistration()
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockMessageHandler.Object);

            var inMemorySettings = new Dictionary<string, string> {
                {"api-key", "test"}
            };

            IConfiguration mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

            var expectedViewModel = new MOTHistoryViewModel
            {
                Registration = "LO61HWX",
                Make = "VAUXHALL",
                Model = "ASTRA",
                Colour = "Silver",
                Mileage = "101710",
                Odometer_Unit = "mi",
                ExpiryDate = "2024.12.03",
                ErrorMessage = "",
            };

            var response = JsonConvert.SerializeObject(new List<VehicleDetails> { new VehicleDetails
            {
                registration = expectedViewModel.Registration,
                make = expectedViewModel.Make,
                model = expectedViewModel.Model,
                primaryColour = expectedViewModel.Colour,
                motTests = new List<MOTTestHistory>
                {
                    new MOTTestHistory
                    {
                        odometerValue = expectedViewModel.Mileage,
                        odometerUnit = expectedViewModel.Odometer_Unit,
                        expiryDate = expectedViewModel.ExpiryDate
                    }
                }
            } });

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response)
            };

            mockMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

            var service = new MOT_History_API_Service(mockHttpClient, mockConfig);

            var actualResult = await service.GetVehicleDetails("LO61HWX");


            Assert.NotNull(actualResult);
            Assert.Equal(expectedViewModel.Registration, actualResult.Registration);
            Assert.Equal(expectedViewModel.Make, actualResult.Make);
            Assert.Equal(expectedViewModel.Model, actualResult.Model);
            Assert.Equal(expectedViewModel.Colour, actualResult.Colour);
            Assert.Equal(expectedViewModel.Mileage, actualResult.Mileage);
            Assert.Equal(expectedViewModel.Odometer_Unit, actualResult.Odometer_Unit);
            Assert.Equal(expectedViewModel.ExpiryDate, actualResult.ExpiryDate);
            //Assert.Empty(actualResult.ErrorMessage);

            
        }
        [Fact]
        public async Task ReturnInvalidRegistration()
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockMessageHandler.Object);
            var inMemorySettings = new Dictionary<string, string> {
                {"api-key", "test"}
            };

            IConfiguration mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

            var expectedViewModel = new MOTHistoryViewModel
            {
                Registration = "XX10ABD",
                Make = "",
                Model = "",
                Colour = "",
                Mileage = "",
                Odometer_Unit = "",
                ExpiryDate = "",
                ErrorMessage = "No MOT Tests found with vehicle registration : XX10ABD",
            };

            var response = JsonConvert.SerializeObject(new List<VehicleDetails> { new VehicleDetails
            {
                registration = expectedViewModel.Registration,
                make = expectedViewModel.Make,
                model = expectedViewModel.Model,
                primaryColour = expectedViewModel.Colour,
                motTests = new List<MOTTestHistory>
                {
                    new MOTTestHistory
                    {
                        odometerValue = expectedViewModel.Mileage,
                        odometerUnit = expectedViewModel.Odometer_Unit,
                        expiryDate = expectedViewModel.ExpiryDate
                    }
                }
            } });

            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("{\"errorMessage\": \"No MOT Tests found with vehicle registration : XX10ABD\"}")
            };

            mockMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

            var service = new MOT_History_API_Service(mockHttpClient, mockConfig);

            var actualResult = await service.GetVehicleDetails("XX10ABD");


            Assert.NotNull(actualResult);
            Assert.Null(actualResult.Registration);
            Assert.Null(actualResult.Make);
            Assert.Null(actualResult.Model);
            Assert.Null(actualResult.Colour);
            Assert.Null( actualResult.Mileage);
            Assert.Null(actualResult.Odometer_Unit);
            Assert. Null(actualResult.ExpiryDate);
            Assert.NotEmpty(actualResult.ErrorMessage);
            Assert.Equal(expectedViewModel.ErrorMessage, actualResult.ErrorMessage);


        }

        [Fact]
        public async Task GetMOTHistory_returnError_missing_MOT()
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockMessageHandler.Object);
            var inMemorySettings = new Dictionary<string, string> {
                {"api-key", "test"}
            };

            IConfiguration mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();


            var expectedDetails = new List<VehicleDetails>
            {
                new VehicleDetails
                {
                    registration = "ABC123",
                    make = "Make",
                    model = "Model",
                    primaryColour = "Colour",
                    motTests = null // No MOT tests
                }
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(expectedDetails))
    
            };
            mockMessageHandler
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

            var service = new MOT_History_API_Service(mockHttpClient, mockConfig);

            var actualResult = await service.GetVehicleDetails("ABC123");

            Assert.NotNull(actualResult);
            Assert.Equal("Error deserializing MOT details: Value cannot be null. (Parameter 'source')", actualResult.ErrorMessage);


        }
        [Fact]
        public async Task GetMOTHistoryReturnExpiryWhenMostRecentTestFailed()
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockMessageHandler.Object);
            var inMemorySettings = new Dictionary<string, string> {
                {"api-key", "test"}
            };

            IConfiguration mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

            var exprectedResult = new List<VehicleDetails>
            {
                new VehicleDetails
                {
                    registration = "TEST666",
                    make = "make",
                    model = "vroom vroom",
                    primaryColour = "magenta",
                    motTests = new List<MOTTestHistory>
                    {
                        new MOTTestHistory
                        {
                            odometerValue = "100009",
                            odometerUnit = "mi",
                            testResult = "FAILED",

                        },
                        new MOTTestHistory{
                            odometerValue = "98400",
                            testResult = "PASSED",
                            odometerUnit = "mi",
                            expiryDate = "2024.03.01",
                        }
                    }
                }
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(exprectedResult))

            };
            mockMessageHandler
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

            var service = new MOT_History_API_Service(mockHttpClient, mockConfig);

            var actualResult = await service.GetVehicleDetails("TEST666");

            Assert.NotNull(actualResult);
            Assert.Equal("make", actualResult.Make);
            Assert.Equal("vroom vroom", actualResult.Model);
            Assert.Equal("100009", actualResult.Mileage);
            Assert.Equal("2024.03.01", actualResult.ExpiryDate);



        }
        [Fact]
        public async Task GetMOTHstoryReturnTimeout()
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockMessageHandler.Object);
            var inMemorySettings = new Dictionary<string, string> {
                {"api-key", "test"}
            };

            IConfiguration mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

            mockMessageHandler
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TaskCanceledException("Request timed out"));

            var service = new MOT_History_API_Service(mockHttpClient, mockConfig);

            var actualResult = await service.GetVehicleDetails("TEST666");
            Assert.NotNull (actualResult);
            Assert.Equal("Error fetching MOT history: Request timed out", actualResult.ErrorMessage);
        }

        [Fact]
        public async Task GetMOTHistoryServerError()
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockMessageHandler.Object);
            var inMemorySettings = new Dictionary<string, string> {
                {"api-key", "test"}
            };

            IConfiguration mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

            var serverErrorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            mockMessageHandler
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(serverErrorResponse);

            var service = new MOT_History_API_Service(mockHttpClient, mockConfig);

            var actualResult = await service.GetVehicleDetails("TEST666");

            Assert.NotNull (actualResult);
            Assert.Equal("Error fetching MOT history: status code: InternalServerError", actualResult.ErrorMessage);
        }

    }
}