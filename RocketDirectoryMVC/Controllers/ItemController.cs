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
using DotNetNuke.Collections;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.UI.UserControls;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Helpers;
using Nevoweb.RocketDirectoryMVC.Components;
using Nevoweb.RocketDirectoryMVC.Models;
using Rocket.AppThemes.Components;
using RocketContentAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.WebSockets;

namespace Nevoweb.RocketDirectoryMVC.Controllers
{
    [DnnHandleError]
    public class ItemController : DnnController
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
            Url = new DnnUrlHelper(requestContext, this);

            var context = requestContext.HttpContext;

            _hasEditAccess = false;
            if (User.UserID > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(ModuleContext.Configuration);

            _moduleId = ModuleContext.ModuleId;
            _tabId = ModuleContext.TabId;
            _portalId = ModuleContext.PortalId;
            _moduleRef = _portalId + "_ModuleID_" + _moduleId;

            var urlparams = new Dictionary<string, string>();
            var paramInfo = new SimplisityInfo();
            // get all query string params
            foreach (string key in context.Request.QueryString.AllKeys)
            {
                if (key != null)
                {
                    var keyValue = context.Request.QueryString[key];
                    paramInfo.SetXmlProperty("genxml/urlparams/" + key.ToLower(), keyValue);
                    urlparams.Add(key, keyValue);
                }
            }

            _sessionParam = new SessionParams(paramInfo);
            _sessionParam.TabId = ModuleContext.TabId;
            _sessionParam.ModuleId = ModuleContext.ModuleId;
            _sessionParam.ModuleRef = _moduleRef;
            _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

            _moduleSettings = new ModuleContentLimpet(_portalId, _moduleRef, _systemkey, ModuleContext.ModuleId, ModuleContext.TabId);

            var appThemeSystem = AppThemeUtils.AppThemeSystem(_portalId, _systemkey);
            var portalData = new PortalLimpet(_portalId);
            var appTheme = new AppThemeLimpet(_moduleSettings.PortalId, _moduleSettings.AppThemeAdminFolder, _moduleSettings.AppThemeAdminVersion, _moduleSettings.ProjectName);
            DNNrocketUtils.InjectDependacies(_moduleRef, DnnPage, appTheme, _moduleSettings.ECOMode, PortalSettings.ActiveTab.SkinSrc, portalData.EngineUrlWithProtocol, appThemeSystem.AppThemeVersionFolderRel);
            
            var strHeader2 = RocketContentAPIUtils.DisplayView(_portalId, _systemkey, _moduleRef, "", _sessionParam, "viewlastheader.cshtml", "", _moduleSettings.DisableCache);
            PageIncludes.IncludeTextInHeader(DnnPage, strHeader2);

        }

        public ActionResult Index()
        {
            var strOut = RocketContentAPIUtils.DisplayView(_portalId, _systemkey, _moduleRef, "", _sessionParam, "view.cshtml", "loadsettings", _moduleSettings.DisableCache);
            if (strOut == "loadsettings")
            {
                strOut = RocketContentAPIUtils.DisplaySystemView(_portalId, _moduleRef, _sessionParam, "ModuleSettingsMsg.cshtml");
                string[] parameters;
                parameters = new string[1];
                parameters[0] = string.Format("{0}={1}", "ModuleId", _moduleId.ToString());
                var redirectUrl = DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", _sessionParam.CultureCode, parameters).ToString();
                strOut = strOut.Replace("{redirecturl}", redirectUrl);
                CacheUtils.ClearAllCache(_moduleRef);
            }
            if (_hasEditAccess)
            {
                var editbuttonkey = "editbuttons" + _moduleRef + "_" + User.UserID + "_" + _sessionParam.CultureCode;
                var viewButtonsOut = CacheUtils.GetCache(editbuttonkey, _moduleRef);
                if (viewButtonsOut == null)
                {
                    string[] parameters;
                    parameters = new string[1];
                    parameters[0] = string.Format("{0}={1}", "ModuleId", _moduleId.ToString());
                    var settingsurl = DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", _sessionParam.CultureCode, parameters).ToString();

                    var userParams = new UserParams("ModuleID:" + _moduleId, true);
                    userParams.Set("editurl", ModuleContext.EditUrl());
                    userParams.Set("settingsurl", settingsurl);
                    userParams.Set("appthemeurl", ModuleContext.EditUrl("AppTheme"));
                    userParams.Set("adminpanelurl", ModuleContext.EditUrl("AdminPanel"));
                    userParams.Set("recyclebinurl", ModuleContext.EditUrl("RecycleBin"));
                    userParams.Set("viewurl", Url.ToString()); // Legacy
                    userParams.Set("viewtabid", this.PortalSettings.ActiveTab.TabID.ToString());

                    viewButtonsOut = RocketContentAPIUtils.DisplaySystemView(_portalId, _moduleRef, _sessionParam, "ViewEditButtons.cshtml", true, false);
                    CacheUtils.SetCache(editbuttonkey, viewButtonsOut, _moduleRef);
                }
                strOut = viewButtonsOut + strOut;
            }


            var s = new MvcData();
            s.SetSetting("mvc_index", strOut);
            return View(s);
        }
    }
}
