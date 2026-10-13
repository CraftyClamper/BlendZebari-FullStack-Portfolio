using System;
using System.Collections.Generic;
using System.IO;

namespace Portfolio.Server.EnterpriseEcosystemSimulator
{
    public class AuditHistoryRecord
    {
        public Guid AuditId { get; set; } = Guid.NewGuid();
        public Guid RecordId { get; set; }
        public required string EntityName { get; set; }
        public required string Action { get; set; }
        public DateTime ChangedOn { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> ChangedAttributes { get; set; } = new();
    }

    public class CrmEntity
    {
        public string LogicalName { get; set; }
        public Guid Id { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new();

        public CrmEntity(string logicalName)
        {
            LogicalName = logicalName;
            Id = Guid.NewGuid();
        }
    }

    public class DataversePipeline
    {
        private readonly Action<string, string> _uiLogger;
        private readonly string _mockSharePointPath;

        public static List<AuditHistoryRecord> GlobalAuditTable { get; set; } = new();

        public DataversePipeline(Action<string, string> uiLogger, string mockSharePointPath)
        {
            _uiLogger = uiLogger;
            _mockSharePointPath = mockSharePointPath;
        }

        // Added '?' to CrmEntity to state that preImage can legally pass null values on Create calls
        public void ProcessTransaction(string messageType, CrmEntity entity, CrmEntity? preImage = null)
        {
            _uiLogger($"▶️ Starting Dataverse Transaction Execution [{messageType.ToUpper()}] for [{entity.LogicalName}]...", "info");

            _uiLogger("[Stage 10: Pre-Validation] Running automated data verification rules...", "info");
            if (entity.Attributes.ContainsKey("creditlimit") && (int)entity.Attributes["creditlimit"] < 0)
            {
                throw new InvalidOperationException("Dynamics 365 Exception: Credit Limit cannot be a negative value parameter.");
            }
            _uiLogger(" -> Success: Business validation criteria satisfied.", "success");

            _uiLogger("[Dataverse Core] Committing changes to relational tables...", "success");

            var auditRecord = new AuditHistoryRecord
            {
                RecordId = entity.Id,
                EntityName = entity.LogicalName,
                Action = messageType
            };

            if (messageType.Equals("Create", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var attr in entity.Attributes)
                {
                    auditRecord.ChangedAttributes.Add(attr.Key, $"[Created] Value set to: {attr.Value}");
                }
            }
            else if (messageType.Equals("Update", StringComparison.OrdinalIgnoreCase) && preImage != null)
            {
                foreach (var attr in entity.Attributes)
                {
                    var oldValue = preImage.Attributes.ContainsKey(attr.Key) ? preImage.Attributes[attr.Key]?.ToString() ?? "NULL" : "NULL";
                    var newValue = attr.Value?.ToString() ?? "NULL";

                    if (oldValue != newValue)
                    {
                        auditRecord.ChangedAttributes.Add(attr.Key, $"Modified: Changed from '{oldValue}' to '{newValue}'");
                        _uiLogger($"⚠️ [Audit Delta Detected] Field '{attr.Key}' was modified from '{oldValue}' to '{newValue}'", "warning");
                    }
                }
            }

            GlobalAuditTable.Insert(0, auditRecord);
            _uiLogger($"🟢 [Audit Trail Engine] Securely appended audit ledger row {auditRecord.AuditId.ToString().Substring(0, 8)}... to system audit table.", "success");

            if (messageType.Equals("Create", StringComparison.OrdinalIgnoreCase))
            {
                _uiLogger("[Stage 40: Post-Operation] Firing native enterprise infrastructure hooks...", "info");
                string accountName = entity.Attributes.GetValueOrDefault("name", "UnnamedCorp")?.ToString() ?? "UnnamedCorp";
                string uniqueSlug = entity.Id.ToString().Substring(0, 8);
                string targetFolder = Path.Combine(_mockSharePointPath, "SharePoint_Mock", "sites", "Finance", "Documents", $"{accountName}_{uniqueSlug}");

                Directory.CreateDirectory(targetFolder);
                File.WriteAllText(Path.Combine(targetFolder, "sharepoint_metadata.txt"), $"CRM_Origin_GUID: {entity.Id}\nCreated: {DateTime.UtcNow}");
                _uiLogger($" -> Success: SharePoint Document Location mapped and built at: {targetFolder}", "success");
            }
        }
    }
}
