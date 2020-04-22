using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebApplication1.Pages
{
    [AuthorizeForScopes(Scopes = new[] { "user.read" })]
    public class ProfileModel : PageModel
    {
        readonly ITokenAcquisition tokenAcquisition;
        readonly GraphSettings graphSettings;
        public ProfileModel(ITokenAcquisition tokenAcquisition, IOptions<GraphSettings> graphSettingsValue)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.graphSettings = graphSettingsValue.Value;
        }
        public async Task OnGetAsync()
        {
            GraphServiceClient graphClient = GetGraphServiceClient(new[] { "user.read" });

            var me = await graphClient.Me.Request().GetAsync();
            ViewData["Me"] = me;

            try
            {
                // Get user photo
                using (var photoStream = await graphClient.Me.Photo.Content.Request().GetAsync())
                {
                    byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                    ViewData["Photo"] = Convert.ToBase64String(photoByte);
                }
            }
            catch (System.Exception)
            {
                ViewData["Photo"] = null;
            }
        }

        private GraphServiceClient GetGraphServiceClient(string[] scopes)
        {
            return GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                string result = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                return result;
            }, graphSettings.GraphApiUrl);
        }
    }
}