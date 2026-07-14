using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polyglossy.Interactive.SqlServer;

namespace Polyglossy.Interactive.Kql;

internal static class ToolsServiceClientExtensions
{
    internal static async Task<bool> ConnectAsync(this ToolsServiceClient serviceClient, Uri ownerUri, KqlConnectionDetails kqlDetails)
    {
        var connectionOptions = new Dictionary<string, string>
        {
            {"server", kqlDetails.Cluster},
            {"database", kqlDetails.Database},
            {"azureAccountToken", kqlDetails.Token},
            {"authenticationType", kqlDetails.AuthenticationType}
        };
            
        var connectionParams = new ConnectParams
        {
            OwnerUri = ownerUri.AbsolutePath, 
            Connection = new ConnectionDetails
            {
                Options = connectionOptions
            }
        };
            
        return await serviceClient.ConnectAsync(connectionParams);
    }
}