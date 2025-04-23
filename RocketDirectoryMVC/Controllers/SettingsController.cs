/*
' Copyright (c) 2025 David Lee
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using DNNrocketAPI.Components;
using DotNetNuke.Security;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using Nevoweb.RocketDirectoryMVC.Components;
using RocketDirectoryAPI.Components;
using Simplisity;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nevoweb.RocketDirectoryMVC.Controllers
{
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
    [DnnHandleError]
    public class SettingsController : DnnController
    {
        public string _systemkey;
        public string _moduleRef;
        public SessionParams _sessionParam;
        public ModuleContentLimpet _moduleSettings;
        public int _tabId;
        public int _moduleId;
        public int _portalId;
        private string _articleId;
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            var context = requestContext.HttpContext;

            // Get systemkey from module name. (remove mod/mvc, add "API")
            var moduleName = ModuleContext.Configuration.DesktopModule.ModuleName;
            _systemkey = moduleName.ToLower().Substring(0, moduleName.Length - 3) + "api";

            _moduleId = ModuleContext.ModuleId;
            _tabId = ModuleContext.TabId;
            _portalId = ModuleContext.PortalId;
            _moduleRef = _portalId + "_ModuleID_" + _moduleId;

            _sessionParam = new SessionParams(new SimplisityInfo());
            _sessionParam.TabId = _tabId;
            _sessionParam.ModuleId = _moduleId;
            _sessionParam.ModuleRef = _moduleRef;
            _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Settings()
        {
            var strOut = RocketDirectoryAPIUtils.DisplaySystemView(ModuleContext.PortalId, _systemkey, _moduleRef, _sessionParam, "ModuleSettingsLoad.cshtml", true);
            var s = new MvcData();
            s.SetSetting("mvc_settings", strOut);
            return View(s);
        }
    }
}