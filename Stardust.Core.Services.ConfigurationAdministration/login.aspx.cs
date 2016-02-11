using System;
using System.Web.UI;

namespace Stardust.Core.Services.ConfigurationAdministration
{
    
    public partial class login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.SuppressFormsAuthenticationRedirect = true;
            if (!User.Identity.IsAuthenticated) Response.StatusCode = 401;
            else Response.Redirect("./Configuration");
        }
    }
}