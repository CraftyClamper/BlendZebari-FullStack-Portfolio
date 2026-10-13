using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Portfolio.Server.EnterpriseEcosystemSimulator
{
    public class AutomationJob
    {
        public required string FlowName { get; set; }
        public Guid RecordId { get; set; }
    }


    public class PowerAutomateEngine
    {
        private readonly ConcurrentQueue<AutomationJob> _workQueue = new();
        private readonly Action<string, string> _uiLogger;
        private bool _isRunning = true;

        public PowerAutomateEngine(Action<string, string> uiLogger)
        {
            _uiLogger = uiLogger;
            StartWorkerLoop();
        }

        public void TriggerCloudFlow(string name, Guid id)
        {
            _workQueue.Enqueue(new AutomationJob { FlowName = name, RecordId = id });
            _uiLogger($"[Power Automate] Webhook Hook intercepted. Asynchronous Flow '{name}' added to cloud queue.", "warning");
        }

        private void StartWorkerLoop()
        {
            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    if (_workQueue.TryDequeue(out var job))
                    {
                        _uiLogger($"⚡ [Power Automate Run] Executing Cloud Flow: {job.FlowName}", "warning");
                        await Task.Delay(1200);
                        _uiLogger($"⚡ [Flow Completed] Automated processing completed for CRM Record {job.RecordId}.", "success");
                    }
                    await Task.Delay(200);
                }
            });
        }

        public void Shutdown() => _isRunning = false;
    }
}
