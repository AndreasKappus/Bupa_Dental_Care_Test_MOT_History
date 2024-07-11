using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunit;
using Moq;
using Xunit;
using Bupa_Dental_Care_Test_MOT_History.Services;
using Bupa_Dental_Care_Test_MOT_History.Models;
using Microsoft.Extensions.DependencyInjection;
using Bupa_Dental_Care_Test_MOT_History.Components.Pages;
using Xunit.Sdk;
using Microsoft.Extensions.Configuration;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace MOT_History_Unit_Testing
{
    public class FrontendUnitTests
    {

        [Fact]
        public void FetchAPI_EmptyTextbox()
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockMessageHandler.Object);
            var inMemorySettings = new Dictionary<string, string> {
                {"api-key", "test"}
            };

            IConfiguration mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
            using var context = new TestContext();

            var apiServiceMock = new Mock<MOT_History_API_Service>(mockHttpClient,mockConfig);
            context.Services.AddSingleton(apiServiceMock.Object);

            var component = context.RenderComponent<Home>();

            component.Find("button").Click();


            var errorMessage = component.Find("p").TextContent;
            Assert.Equal("Textbox cannot be empty!", errorMessage);
        }


    }
}
