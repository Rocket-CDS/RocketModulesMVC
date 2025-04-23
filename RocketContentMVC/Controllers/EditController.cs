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
using Nevoweb.RocketContentMVC.Components;
using RocketContentAPI.Components;
using Simplisity;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nevoweb.RocketContentMVC.Controllers
{
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
    [DnnHandleError]
    public class EditController : DnnController
    {
        public const string _systemkey = "rocketcontentapi";
        public bool _hasEditAccess;
        public string _moduleRef;
        public SessionParams _sessionParam;
        public ModuleContentLimpet _moduleSettings;
        public int _tabId;
        public int _moduleId;
        public int _portalId;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            _moduleId = ModuleContext.ModuleId;
            _tabId = ModuleContext.TabId;
            _portalId = ModuleContext.PortalId;
            _moduleRef = _portalId + "_ModuleID_" + _moduleId;

            var context = requestContext.HttpContext;
            var urlparams = new Dictionary<string, string>();
            foreach (string key in context.Request.QueryString.AllKeys)
            {
                if (key != null)
                {
                    var keyValue = context.Request.QueryString[key];
                    urlparams.Add(key, keyValue);
                }
            }

            if (DNNrocketUtils.RequestParam(context, "SkinSrc") == "")
            {
                var editParam = new string[1];
                editParam[0] = string.Format("{0}={1}", "mid", _moduleId.ToString());
                var editurl = DNNrocketUtils.NavigateURL(_tabId, "Edit", DNNrocketUtils.GetCurrentCulture(), editParam).ToString() + "?SkinSrc=rocketedit";
                Response.Redirect(editurl, false);
                context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
            }

            _sessionParam = new SessionParams(new SimplisityInfo());
            _sessionParam.TabId = _tabId;
            _sessionParam.ModuleId = _moduleId;
            _sessionParam.ModuleRef = _moduleRef;
            _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
            DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCode);

            PageIncludes.RemoveCssFile(DnnPage, "skin.css"); //DNN always tries to load a skin.css, even if it does not exists.

            var strHeader1 = RocketContentAPIUtils.DisplayAdminView(_portalId, _moduleRef, "", _sessionParam, "adminfirstheader.cshtml");
            PageIncludes.IncludeTextInHeader(DnnPage, strHeader1);

            var strHeader2 = RocketContentAPIUtils.DisplayAdminView(_portalId, _moduleRef, "", _sessionParam, "adminlastheader.cshtml");
            PageIncludes.IncludeTextInHeaderAt(DnnPage, strHeader2, 0);

            var strHeaderAdmin = RocketContentAPIUtils.DisplaySystemView(_portalId, _moduleRef, _sessionParam, "AdminHeader.cshtml", true, false);
            PageIncludes.IncludeTextInHeader(DnnPage, strHeaderAdmin);

        }

        public ActionResult Edit()
        {
            var strOut = RocketContentAPIUtils.DisplaySystemView(_portalId, _moduleRef, _sessionParam, "AdminDetailLoad.cshtml", true, false);

            var s = new MvcData();
            s.SetSetting("mvc_edit", strOut);
            return View(s);
        }
    }
}
