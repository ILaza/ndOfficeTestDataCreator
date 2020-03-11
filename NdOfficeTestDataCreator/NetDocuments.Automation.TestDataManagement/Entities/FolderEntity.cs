namespace NetDocuments.Automation.TestDataManagement.Entities
{
    public class FolderEntity
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public (string nodeId, string nodeName)[] Path { get; set; }
    }
}