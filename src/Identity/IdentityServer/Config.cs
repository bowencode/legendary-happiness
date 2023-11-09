using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Address(),
            new IdentityResource("thirdParty", new []{ "externalAccessToken" })
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("read:user-details"),
            new ApiScope("read:username"),
            new ApiScope("read:users"),
            new ApiScope("read:notes"),
            new ApiScope("write:notes"),
            new ApiScope("list:notes"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials client for User to Admin API calls
            new Client
            {
                ClientId = "m2m.api",
                ClientName = "Web API Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                AllowedScopes = { "read:username" }
            },

            // m2m client credentials client for server-side application to Admin API calls
            new Client
            {
                ClientId = "m2m.web-admin",
                ClientName = "Web App Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("7954d1dd-49c0-4b9e-acd9-780c78a5570e".Sha256()) },

                AllowedScopes = { "read:users" }
            },

            // m2m client credentials client for web client admin BFF to User API calls
            new Client
            {
                ClientId = "m2m.web-admin-app",
                ClientName = "Web Client Admin App Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("2e7a8484-8fdc-4458-9222-95f3b369421c".Sha256()) },

                AllowedScopes = { "list:notes" }
            },

            // interactive client for server-side application
            new Client
            {
                ClientId = "web-ui",
                ClientSecrets = { new Secret("1f668bf6-5ef5-4e77-ae84-28614dfc9d2d".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                // where to redirect to after login
                RedirectUris =
                {
                    "https://localhost:7163/signin-oidc",
                    "http://localhost:5163/signin-oidc",
                },

                // where to redirect to after logout
                PostLogoutRedirectUris =
                {
                    "https://localhost:7163/signout-callback-oidc",
                    "http://localhost:5163/signout-callback-oidc",
                },
                
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "read:notes",
                    "write:notes",
                }
            },

            // interactive client for web client admin application
            new Client
            {
                ClientId = "web-admin-ui",
                ClientSecrets = { new Secret("71d2e259-a6ef-4f7e-af25-ae6f2c8ba54f".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                EnableLocalLogin = false,
                IdentityProviderRestrictions = { "aad" },
                AllowOfflineAccess = true,

                // where to redirect to after login
                RedirectUris =
                {
                    "https://localhost:7044/signin-oidc",
                    "http://localhost:5044/signin-oidc",
                },

                // where to redirect to after logout
                PostLogoutRedirectUris =
                {
                    "https://localhost:7044/signout-callback-oidc",
                    "http://localhost:5044/signout-callback-oidc",
                },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                }
            },

            // interactive client for desktop client admin application
            new Client
            {
                ClientId = "desktop-admin-ui",
                ClientSecrets = { new Secret("1cf6de3f-0b06-4457-a114-3a7c00658878".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                EnableLocalLogin = false,
                IdentityProviderRestrictions = { "aad" },
                AllowOfflineAccess = true,

                // where to redirect to after login
                RedirectUris =
                {
                    "http://127.0.0.1/wpf-notes-admin-app",
                },

                // where to redirect to after logout
                PostLogoutRedirectUris =
                {
                    "http://127.0.0.1/wpf-notes-admin-app/signout-callback-oidc",
                },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "read:user-details",
                    "list:notes",
                }
            },

            // interactive client for SPA client application
            new Client
            {
                ClientId = "spa-user-ui",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,

                RedirectUris =           { "https://localhost:3000/callback.html" },
                PostLogoutRedirectUris = { "https://localhost:3000/index.html" },
                AllowedCorsOrigins =     { "https://localhost:3000" },

                AlwaysIncludeUserClaimsInIdToken = true,
                
                IdentityProviderRestrictions = { "none" },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "read:notes",
                    "list:notes",
                }
            },

            // interactive client for desktop client admin application
            new Client
            {
                ClientId = "mobile-user-ui",
                RequireClientSecret = false,

                AllowedGrantTypes = GrantTypes.Code,
                AllowOfflineAccess = true,
                IdentityProviderRestrictions = { "auth0" },

                // where to redirect to after login
                RedirectUris =
                {
                    "notesmobiledemo://callback",
                },

                // where to redirect to after logout
                PostLogoutRedirectUris =
                {
                    "notesmobiledemo://callback",
                },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "thirdParty",
                    "read:notes",
                    "list:notes",
                    "write:notes",
                }
            },
        };
}
