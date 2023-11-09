using System.Reflection;
using System.Text.Json;

namespace Demo.Notes.Web.AdminApi.Host.Model
{
    public class TestUsers
    {
        public static List<AppUser> Users
        {
            get
            {
                try
                {
                    string binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string dataFilePath = Path.Combine(binDirectory, "TestUsers.json");
                    string json = File.ReadAllText(dataFilePath);
                    var testUsers = JsonSerializer.Deserialize<List<AppUser>>(json);
                    if (testUsers == null)
                    {
                        throw new InvalidOperationException();
                    }

                    return testUsers;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to load users", ex);
                }
            }
        }
    }
}