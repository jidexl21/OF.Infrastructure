using System;
using System.Collections.Generic;
using System.Text;

namespace OF.Infrastructure.Auth
{
    public interface IAuthSettings
    {
        /// <summary>
        /// Secret JWT Key
        /// </summary>
        string Key { get; set; }
        /// <summary>
        /// Application Identifier
        /// </summary>
        string AppID { get; set; }
        /// <summary>
        /// URL of the authentication endpoint
        /// </summary>
        string AuthAppURL { get; set; }
    }
}
