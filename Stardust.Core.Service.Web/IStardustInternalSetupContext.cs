using System.ComponentModel;

namespace Stardust.Core.Service.Web
{
    internal interface IStardustInternalSetupContext
    {
        /// <summary>
        /// Hooks up the MVC dependency resolver and controller factory.
        /// </summary>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        ISetupContext BindItAll();
    }
}