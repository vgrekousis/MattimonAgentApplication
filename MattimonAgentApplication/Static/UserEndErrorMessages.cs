using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattimonAgentApplication.Static
{
    public class UserEndErrorMessages
    {
        public const string SQLITE_ERROR_WRITE_CONNECTION_STRINGS =
            "Couldn't write local reference therefore we're unable to complete this request. User-end database may be corrupted.\n" +
            "Please let us know about this error.\n\nError details:\n";
    }
}
