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
using System.Web.Mvc;
using System.Web.Routing;

namespace Nevoweb.RocketDirectoryMVC.Controllers
{
    [DnnHandleError]
    public class AdminPanelController : DnnController
    {
        public string _systemkey;
        public string _moduleRef;
        public SessionParams _sessionParam;
        public ModuleContentLimpet _moduleSettings;
        public int _tabId;
        public int _moduleId;
        public int _portalId;
        private string _articleId;

        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [DnnHandleError]
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            var context = requestContext.HttpContext;

            // Get systemkey from module name. (remove mod/mvc, add "API")
            var moduleName = ModuleContext.Configuration.DesktopModule.ModuleName;
            _systemkey = moduleName.ToLower().Substring(0, moduleName.Length - 3) + "api";


            _articleId = DNNrocketUtils.RequestParam(context, "articleid");
            string skinSrcAdmin = "?SkinSrc=rocketadmin";
            if (DNNrocketUtils.RequestParam(context, "SkinSrc") == "")
            {
                if (_articleId == null || _articleId == "")
                    Response.Redirect(ModuleContext.EditUrl("AdminPanel") + skinSrcAdmin, false);
                else
                    Response.Redirect(ModuleContext.EditUrl("articleid", _articleId, "AdminPanel") + skinSrcAdmin, false);
                context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
            }

            _moduleId = ModuleContext.ModuleId;
            _tabId = ModuleContext.TabId;
            _portalId = ModuleContext.PortalId;
            _moduleRef = _portalId + "_ModuleID_" + _moduleId;

            _sessionParam = new SessionParams(new SimplisityInfo());
            _sessionParam.TabId = _tabId;
            _sessionParam.ModuleId = _moduleId;
            _sessionParam.ModuleRef = _moduleRef;
            _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
            _sessionParam.Set("articleid", _articleId);

            PageIncludes.RemoveCssFile(DnnPage, "skin.css"); //DNN always tries to load a skin.css, even if it does not exists.
            var strHeader1 = RocketDirectoryAPIUtils.DisplaySystemView(_portalId, _systemkey, _moduleRef, _sessionParam, "AdminPanelHeader.cshtml");
            PageIncludes.IncludeTextInHeader(DnnPage, strHeader1);


        }

        [HttpGet]
        public ActionResult AdminPanel()
        {
            var strOut = RocketDirectoryAPIUtils.DisplaySystemView(_portalId, _systemkey, _moduleRef, _sessionParam, "AdminPanelLoad.cshtml", true);

            var s = new MvcData();
            s.SetSetting("mvc_adminpanel", strOut);
            return View(s);
        }
    }
}
