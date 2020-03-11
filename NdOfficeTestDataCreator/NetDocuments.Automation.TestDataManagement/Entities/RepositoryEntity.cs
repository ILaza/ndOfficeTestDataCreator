using System.Collections.Generic;

namespace NetDocuments.Automation.TestDataManagement.Entities
{
    public class RepositoryEntity
    {
        public string Id { get; set; }

        public string DefaultCabinetId { get; set; }

        public List<string> CabinetsIds { get; set; }
    }
}
