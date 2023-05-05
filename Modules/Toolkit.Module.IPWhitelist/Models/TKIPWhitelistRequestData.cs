#if NETFULL
using System.Net.Http;
using System.Web;
#endif

#if NETCORE
using Microsoft.AspNetCore.Http;
#endif

namespace QoDL.Toolkit.Module.IPWhitelist.Models;

/// <summary></summary>
public class TKIPWhitelistRequestData
{
    /// <summary>Resolved client ip.</summary>
    public string IP { get; set; }

    /// <summary>Full path without querystring</summary>
    public string Path { get; set; }

    /// <summary>Full path and querystring</summary>
    public string PathAndQuery { get; set; }

#if NETCORE
    /// <summary></summary>
    public HttpContext Context { get; set; }
#endif

#if NETFULL
    /// <summary>Current request (non-webapi)</summary>
    public HttpRequest Request { get; set; }
    
    /// <summary>Current request (webapi)</summary>
    public HttpRequestMessage WebApiRequest { get; set; }
#endif
}
