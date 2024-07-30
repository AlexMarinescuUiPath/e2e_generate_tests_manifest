using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

string rootDirectory = @"C:\Work\Studio\Qa\E2E";

if (!Directory.Exists(rootDirectory))
{
    Console.WriteLine("The provided directory does not exist.");
    return;
}

var projectDirectories = Directory.GetDirectories(rootDirectory).Where(s => s.EndsWith(".CodedTests"));

foreach (var projectDirectory in projectDirectories)
{
    string projectFilePath = Path.Combine(projectDirectory, "project.json");
    string testsManifestPath = Path.Combine(projectDirectory, "testsManifest.json");
    List<JsonObject> testsManifestList = new List<JsonObject>();

    if (File.Exists(projectFilePath))
    {
        try
        {
            string jsonContent = await File.ReadAllTextAsync(projectFilePath);
            var jsonNode = JsonNode.Parse(jsonContent);

            if (jsonNode != null && jsonNode["designOptions"]?["fileInfoCollection"] is JsonArray fileInfoCollection)
            {
                foreach (var fileInfo in fileInfoCollection)
                {
                    if (fileInfo != null && fileInfo["testCaseId"] != null)
                    {
                        fileInfo["testCaseId"] = JsonValue.Create(Guid.NewGuid().ToString());

                        var testManifestItem = new JsonObject
                        {
                            ["Name"] = fileInfo["fileName"]?.ToString(),
                            ["Category"] = "Extended Coverage",
                            ["Owner"] = " "
                        };
                        testsManifestList.Add(testManifestItem);
                    }
                }

                string updatedJsonContent = jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(projectFilePath, updatedJsonContent);

                Console.WriteLine($"Updated GUIDs in {projectFilePath}");
            }

            string updatedManifestContent = JsonSerializer.Serialize(testsManifestList, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(testsManifestPath, updatedManifestContent);
            Console.WriteLine($"Updated tests manifest in {testsManifestPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update {projectFilePath}: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine($"No project.json file found in {projectDirectory}");
    }
}

public class Project
{
    [JsonPropertyName("designOptions")]
    public DesignOptions DesignOptions { get; set; }
}

public class DesignOptions
{
    [JsonPropertyName("fileInfoCollection")]
    public List<FileInfo> FileInfoCollection { get; set; }
}

public class FileInfo
{
    [JsonPropertyName("testCaseId")]
    public string TestCaseId { get; set; }
}
