using Demo.Notes.Common.Configuration;
using Microsoft.Azure.Cosmos;

namespace Demo.Notes.Web.UserApi.Host.Controllers
{
    public class NotesDatabase
    {
        public static CosmosClient GetClient(CosmosOptions dbOptions)
        {
            CosmosClient cosmosClient = new CosmosClient(dbOptions.ConnectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
            return cosmosClient;
        }

        public static async Task<Container> GetNotesContainerAsync(CosmosOptions dbOptions)
        {
            var client = GetClient(dbOptions);
            Database db = await client.CreateDatabaseIfNotExistsAsync(dbOptions.Database);
            Container container = await db.CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = dbOptions.NotesContainer,
                PartitionKeyPath = "/id",
            }, ThroughputProperties.CreateAutoscaleThroughput(4000));
            return container;
        }
    }
}