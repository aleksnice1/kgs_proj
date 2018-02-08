using Kadastr.CommonUtils;
using System.Web.Mvc;
using System.Web.Security;

namespace Kadastr.WebApp.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return Redirect(FormsAuthentication.LoginUrl);
        }

        public ActionResult ChangePassword()
        {
            return Redirect("~/PasswordEditor.aspx");
        }
    }
}
