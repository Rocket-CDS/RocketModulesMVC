﻿/*
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
using Nevoweb.RocketFormMVC.Components;
using RocketContentAPI.Components;
using RocketForms.Components;
using Simplisity;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nevoweb.RocketFormMVC.Controllers
{
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
    [DnnHandleError]
    public class AppThemeController : DnnController
    {
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

            var context = requestContext.HttpContext;

            string skinSrcAdmin = "?SkinSrc=rocketedit";
            if (DNNrocketUtils.RequestParam(context, "SkinSrc") == "")
            {
                Response.Redirect(ModuleContext.EditUrl("AppTheme") + skinSrcAdmin, false);
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
            DNNrocketUtils.SetCookieValue("simplisity_language", _sessionParam.CultureCode);

            //PageIncludes.RemoveCssFile(DnnPage, "skin.css"); //DNN always tries to load a skin.css, even if it does not exists.

            var strHeader1 = RocketFormsUtils.DisplaySystemView(_portalId, _moduleRef, _sessionParam, "AdminHeader.cshtml");
            PageIncludes.IncludeTextInHeader(DnnPage, strHeader1);

        }

        public ActionResult AppTheme()
        {
            var strOut = RocketFormsUtils.DisplaySystemView(_portalId, _moduleRef, _sessionParam, "AppThemeAdmin.cshtml");

            var s = new MvcData();
            s.SetSetting("mvc_apptheme", strOut);
            return View(s);
        }
    }
}
