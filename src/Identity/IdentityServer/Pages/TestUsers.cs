// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Duende.IdentityServer.Test;
using System.Text.Json;
using System.Reflection;
using Duende.IdentityServer.Stores.Serialization;

namespace IdentityServer;

public class TestUsers
{
    public static List<TestUser> Users
    {
        get
        {
            string binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dataFilePath = Path.Combine(binDirectory, "TestUsers.json");
            string json = File.ReadAllText(dataFilePath);
            List<TestUser> testUsers = JsonSerializer.Deserialize<List<TestUser>>(json, new JsonSerializerOptions
            {
                Converters =
                {
                    new ClaimConverter()
                }
            });
            return testUsers;
        }
    }
}