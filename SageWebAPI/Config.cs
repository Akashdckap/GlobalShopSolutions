using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace GlobalSolutions
{
    public static class Config
    {
        public static IEnumerable<Client> Clients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    // scopes that client has access to
                    AllowedScopes = { "sagewebapi" }
                }
            };
       
        }

        public static IEnumerable<IdentityResource> IdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };
        }

        public static IEnumerable<ApiResource> Apis()
        {
            return new List<ApiResource>
            {
                new ApiResource("sagewebapi", "Sage Web API")
            };
        }

        public static IEnumerable<ApiScope> Apiscope => new List<ApiScope>
        {
            new ApiScope("sagewebapi", "Sage Web API")
        };
    }
}
