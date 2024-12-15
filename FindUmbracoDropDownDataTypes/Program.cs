using Newtonsoft.Json.Linq;
using System.Xml;

string dataTypesDirectory = "C:\\Solutions\\Apply.Brobizz\\Apply.BroBizz.Web\\uSync\\v8\\DataTypes";


string jsonPath_DefinitionTheme = Path.Combine("C:\\Scripts\\ComponentsAndThemes_V2", "UDF_DTKEY.json");


if (!File.Exists(jsonPath_DefinitionTheme))
{
    File.WriteAllText(jsonPath_DefinitionTheme, "[]");
}

if (!Directory.Exists(dataTypesDirectory))
{
    Console.WriteLine($"The directory path '{dataTypesDirectory}' does not exist. Exiting script.");
    Environment.Exit(0);
}
//Hent mine filer
var dataTypeFiles = Directory.GetFiles(dataTypesDirectory);

List<string> definitionKeys = new List<string>();

if (dataTypeFiles.Length == 0)
{
    Console.WriteLine("No files found in the directory. Exiting script.");
    Environment.Exit(0);
}


//Loop at finde UmbracoDropDown typer

foreach (var file in dataTypeFiles)
{
    XmlDocument xmlDoc = new XmlDocument();
    xmlDoc.Load(file);


    XmlNode? dataTypeRootElement = xmlDoc?.SelectSingleNode("//DataType");
    XmlNode? editorAliasChild = xmlDoc?.SelectSingleNode("//DataType/Info/EditorAlias");
    XmlNode? dataTypeInfoName = xmlDoc?.SelectSingleNode("//DataType/Info/Name");
    XmlNode? configNode = xmlDoc?.SelectSingleNode("//DataType/Config");

    string? dataTypeInfoNameValue = dataTypeInfoName?.InnerText;
    string? editorAliasValue = editorAliasChild?.InnerText;


    if (dataTypeRootElement != null && dataTypeRootElement.Attributes != null)
    {
        string? dataTypeKeyAtt = dataTypeRootElement.Attributes["Key"]?.InnerText;
        if (!string.IsNullOrEmpty(dataTypeKeyAtt))
        {
            if (editorAliasValue != null && editorAliasValue == "Umbraco.DropDown.Flexible")
            {

                if (configNode != null && configNode.FirstChild is XmlCDataSection cdataSection)
                {
                    string? jsonString = cdataSection.Value;

                    try
                    {
                        var valuesJsonObject = JObject.Parse(jsonString);
                        var itemsArray = valuesJsonObject["Items"]?.ToObject<List<JObject>>();
                        var dataTypesValues = itemsArray?.Select(item => item["value"]?.ToString()).Where(value => value != null).ToList();

                        var jsonObject = new
                        {
                            DataTypeKeyAttribute = dataTypeKeyAtt,
                            DataTypeInfoName = dataTypeInfoNameValue,
                            DataTypeValues = dataTypesValues ?? new List<string>()
                        };
                        string existingJson = File.ReadAllText(jsonPath_DefinitionTheme);
                        var jsonArray = Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(existingJson) ?? new List<object>();

                        jsonArray.Add(jsonObject);

                        File.WriteAllText(jsonPath_DefinitionTheme, Newtonsoft.Json.JsonConvert.SerializeObject(jsonArray, Newtonsoft.Json.Formatting.Indented));

                    }
                    catch
                    {
                        Console.WriteLine("We're in the catch");
                    }
                }



            }
        }
    }
}


Console.WriteLine($"Processing completed.");
