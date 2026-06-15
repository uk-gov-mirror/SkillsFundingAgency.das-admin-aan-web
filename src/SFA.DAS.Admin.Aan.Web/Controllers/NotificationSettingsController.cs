using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Admin.Aan.Application.OuterApi.NotificationSettings;
using SFA.DAS.Admin.Aan.Application.Services;
using SFA.DAS.Admin.Aan.Web.Extensions;
using SFA.DAS.Admin.Aan.Web.Infrastructure;
using SFA.DAS.Admin.Aan.Web.Models.NotificationSettings;

namespace SFA.DAS.Admin.Aan.Web.Controllers
{
    [Authorize]
    [Route("notification-settings", Name = RouteNames.NotificationSettings)]

    public class NotificationSettingsController(IOuterApiClient outerApiClient, ISessionService sessionService, IValidator<NotificationSettingsPostRequest> validator) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = await GetViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(NotificationSettingsPostRequest request)
        {
            var adminMemberId = sessionService.GetMemberId();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                ModelState.AddValidationErrors(result.Errors);
                var model = await GetViewModel();
                model.ReceiveNotifications = request.ReceiveNotifications;
                return View(model);
            }
            var postRequest = new PostNotificationSettings
            {
                ReceiveNotifications = request.ReceiveNotifications!.Value
            };

            await outerApiClient.PostNotificationSettings(adminMemberId, postRequest, default);

            TempData.AddFlashMessage("Notification settings saved", TempDataDictionaryExtensions.FlashMessageLevel.Success);

            return RedirectToRoute(RouteNames.AdministratorHub);
        }

        private async Task<NotificationSettingsViewModel> GetViewModel()
        {
            var adminMemberId = sessionService.GetMemberId();
            var response = await outerApiClient.GetNotificationSettings(adminMemberId, default);
            var viewModel = (NotificationSettingsViewModel)response;
            return viewModel;
        }
    }
}
