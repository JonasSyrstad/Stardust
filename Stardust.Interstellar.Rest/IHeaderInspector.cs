using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stardust.Interstellar.Rest
{
    public interface IHeaderInspector
    {

        IHeaderHandler[] GetHandlers();
    }
}
