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
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Helpers;
using Nevoweb.RocketDirectoryMVC.Components;
using Rocket.AppThemes.Components;
using RocketDirectoryAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nevoweb.RocketDirectoryMVC.Controllers
{
    [DnnHandleError]
    public class ItemController : DnnController
    {
        public string _systemkey;
        public bool _hasEditAccess;
        public string _moduleRef;
        public SessionParams _sessionParam;
        public ModuleContentLimpet _moduleSettings;
        public int _tabId;
        public int _moduleId;
        public int _portalId;
        public Dictionary<string, string> _urlparams;
        public HttpContextBase _context;

        protected override void Initialize(RequestContext requestContext)
        {
            try
            {

                base.Initialize(requestContext);
                Url = new DnnUrlHelper(requestContext, this);

                _context = requestContext.HttpContext;

                _hasEditAccess = false;
                if (User.UserID > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(ModuleContext.Configuration);

                _moduleId = ModuleContext.ModuleId;
                _tabId = ModuleContext.TabId;
                _portalId = ModuleContext.PortalId;
                _moduleRef = _portalId + "_ModuleID_" + _moduleId;

                // Get systemkey from module name. (remove mod/mvc, add "API")
                var moduleName = ModuleContext.Configuration.DesktopModule.ModuleName;
                _systemkey = moduleName.ToLower().Substring(0, moduleName.Length - 3) + "api";


                _urlparams = new Dictionary<string, string>();
                var paramInfo = new SimplisityInfo();
                // get all query string params
                foreach (string key in _context.Request.QueryString.AllKeys)
                {
                    if (key != null)
                    {
                        var keyValue = _context.Request.QueryString[key];
                        paramInfo.SetXmlProperty("genxml/urlparams/" + key.ToLower(), keyValue);
                        _urlparams.Add(key.ToLower(), keyValue);
                    }
                }

                var jsonparams = DNNrocketUtils.GetCookieValue("simplisity_sessionparams");
                if (jsonparams != "")
                {
                    try
                    {
                        var simplisity_sessionparams = SimplisityJson.DeserializeJson(jsonparams, "cookie");
                        paramInfo.AddXmlNode(simplisity_sessionparams.XMLData, "cookie", "genxml");
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
                _sessionParam = new SessionParams(paramInfo);
                _sessionParam.TabId = _tabId;
                _sessionParam.ModuleId = _moduleId;
                _sessionParam.ModuleRef = _moduleRef;
                _sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();
                _sessionParam.Url = _context.Request.Url.AbsoluteUri;
                _sessionParam.UrlFriendly = DNNrocketUtils.NavigateURL(_tabId, _urlparams);
                _sessionParam.SearchText = DNNrocketUtils.RequestParam(_context, "search");
                var pageParam = DNNrocketUtils.RequestParam(_context, "page");
                if (GeneralUtils.IsNumeric(pageParam))  _sessionParam.Page = Convert.ToInt32(pageParam);

                _moduleSettings = new ModuleContentLimpet(_portalId, _moduleRef, _systemkey, _sessionParam.ModuleId, _sessionParam.TabId);

                var appThemeSystem = AppThemeUtils.AppThemeSystem(_portalId, _systemkey);
                var portalData = new PortalLimpet(_portalId);
                var appTheme = new AppThemeLimpet(_moduleSettings.PortalId, _moduleSettings.AppThemeAdminFolder, _moduleSettings.AppThemeAdminVersion, _moduleSettings.ProjectName);
                DNNrocketUtils.InjectDependacies(_moduleRef, DnnPage, appTheme, _moduleSettings.ECOMode, PortalSettings.ActiveTab.SkinSrc, portalData.EngineUrlWithProtocol, appThemeSystem.AppThemeVersionFolderRel);

                var strHeader2 = RocketDirectoryAPIUtils.ViewHeader(_portalId, _systemkey, _moduleRef, _sessionParam, "viewlastheader.cshtml");
                PageIncludes.IncludeTextInHeader(DnnPage, strHeader2);

                if (_hasEditAccess)
                {
                    // Set langauge, so editing with simplity gets correct language
                    var lang = DNNrocketUtils.GetCurrentCulture();
                    DNNrocketUtils.SetCookieValue("simplisity_editlanguage", lang);
                    var qlang = DNNrocketUtils.RequestParam(_context, "language");
                    if (qlang != null && DNNrocketUtils.ValidCulture(qlang)) lang = qlang;
                    DNNrocketUtils.SetCookieValue("simplisity_language", lang);
                }
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
            }

        }

        public ActionResult Index()
        {
            var strOut = RocketDirectoryAPIUtils.DisplayView(_portalId, _systemkey, _moduleRef, _sessionParam, "", "loadsettings");
            if (strOut == "loadsettings")
            {
                strOut = RocketDirectoryAPIUtils.DisplaySystemView(_portalId, _systemkey, _moduleRef, _sessionParam, "ModuleSettingsMsg.cshtml", false);
                string[] parameters;
                parameters = new string[1];
                parameters[0] = string.Format("{0}={1}", "ModuleId", _moduleId.ToString());
                var redirectUrl = DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", _sessionParam.CultureCode, parameters).ToString();
                strOut = strOut.Replace("{redirecturl}", redirectUrl);
                CacheUtils.ClearAllCache(_systemkey + _portalId);
            }
            if (_hasEditAccess)
            {
                var editbuttonkey = "editbuttons" + _moduleRef + "_" + User.UserID + "_" + _sessionParam.CultureCode;
                var viewButtonsOut = CacheUtils.GetCache(editbuttonkey, _moduleRef);
                if (viewButtonsOut == null)
                {
                    var articleUrlKey = RocketDirectoryAPIUtils.UrlQueryArticleKey(_portalId, _systemkey);
                    var articleid = DNNrocketUtils.RequestParam(_context, articleUrlKey);
                    string[] parameters;
                    parameters = new string[1];
                    parameters[0] = string.Format("{0}={1}", "ModuleId", _moduleId.ToString());
                    var settingsurl = DNNrocketUtils.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Module", _sessionParam.CultureCode, parameters).ToString();

                    var userParams = new UserParams("ModuleID:" + _moduleId, true);
                    if (GeneralUtils.IsNumeric(articleid))
                    {
                        _sessionParam.Set("articleid", articleid);
                        userParams.Set("editurl", ModuleContext.EditUrl("articleid", articleid, "AdminPanel"));
                    }
                    userParams.Set("settingsurl", settingsurl);
                    userParams.Set("appthemeurl", ModuleContext.EditUrl("AppTheme"));
                    userParams.Set("adminpanelurl", ModuleContext.EditUrl("AdminPanel"));
                    userParams.Set("viewurl", Url.ToString()); // Legacy
                    userParams.Set("viewtabid", this.PortalSettings.ActiveTab.TabID.ToString());

                    viewButtonsOut = RocketDirectoryAPIUtils.DisplaySystemView(_portalId, _systemkey, _moduleRef, _sessionParam, "ViewEditButtons.cshtml");
                    CacheUtils.SetCache("editbuttons" + _moduleRef, viewButtonsOut, _moduleRef);
                }
                strOut = viewButtonsOut + strOut;
            }
            var s = new MvcData();
            s.SetSetting("mvc_index", strOut);
            return View(s);
        }
    }
}
