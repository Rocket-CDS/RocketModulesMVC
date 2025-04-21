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
using DotNetNuke.Security;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using Nevoweb.RocketDirectoryMVC.Components;
using RocketContentAPI.Components;
using Simplisity;
using System.Reflection;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Nevoweb.RocketDirectoryMVC.Controllers
{
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
    [DnnHandleError]
    public class SettingsController : DnnController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Settings()
        {
            var moduleRef = ModuleContext.PortalId + "_ModuleID_" + ModuleContext.ModuleId;
            var sessionParam = new SessionParams(new SimplisityInfo());
            sessionParam.TabId = ModuleContext.TabId;
            sessionParam.ModuleId = ModuleContext.ModuleId;
            sessionParam.ModuleRef = moduleRef;
            sessionParam.CultureCode = DNNrocketUtils.GetCurrentCulture();

            var strOut = RocketContentAPIUtils.DisplaySystemView(ModuleContext.PortalId, moduleRef, sessionParam, "ModuleSettingsLoad.cshtml", true, false);
            var s = new MvcData();
            s.SetSetting("mvc_settings", strOut);
            return View(s);
        }
    }
}