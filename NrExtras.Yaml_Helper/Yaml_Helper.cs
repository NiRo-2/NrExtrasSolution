using YamlDotNet.RepresentationModel;

namespace NrExtras.Yaml_Helper
{
    public static class Yaml_Helper
    {
        /// <summary>
        /// Read yaml file to YamlStream
        /// </summary>
        /// <param name="yamlFilePath">yaml file path</param>
        /// <returns>YamlStream</returns>
        public static YamlStream readYamlToObject(string yamlFilePath)
        {
            try
            {
                using (var reader = new StreamReader(yamlFilePath))
                {
                    // Load the stream
                    var yaml = new YamlStream();
                    yaml.Load(reader);
                    return yaml;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Search for key and return it's value in yaml file. Note - search only for Scalar nodes
        /// </summary>
        /// <param name="yamlStream">yaml stream</param>
        /// <param name="searchedValue">searched key</param>
        /// <returns>value suits that key</returns>
        /// <exception cref="Exception">key not found</exception>
        public static string getYamlValue_AfterSpecificValue(YamlStream yamlStream, string searchedValue)
        {
            bool returnNode = false;

            foreach (var doc in yamlStream.Documents)
            {
                foreach (YamlNode node in doc.AllNodes)
                {
                    //if returnNode=true - we found the value we need, get the one after it
                    if (returnNode)
                        return node.ToString();
                    //still not found - continue looking for the right cell
                    if (node.NodeType == YamlNodeType.Scalar)
                        if (((string?)node) == searchedValue)
                            returnNode = true; //node found, get the one after it
                }
            }

            //not found
            throw new Exception("searched value not found");
        }
    }
}