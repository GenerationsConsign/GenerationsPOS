using GenerationsPOS.Utilities;
using GenerationsPOS.ViewModels.StatusPane;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale.Hardware.Metrics
{
    public class SystemStatus
    {
        private ObservableCollection<IMetric> _SystemMetrics = new();
        private ObservableCollection<Message> _SystemMessages = new();

        private Task MetricPoller;

        public event NotifyEventHandler Alert = delegate { };

        public SystemStatus()
        {
            // Spin off metric monitor
            MetricPoller = Task.Run(MetricMonitor);
        }

        public ObservableCollection<IMetric> SystemMetrics
        {
            get => _SystemMetrics;
        }

        public void AddMetric(IMetric metric)
        {
            _SystemMetrics.Add(metric);
        }

        public void AddMetrics(IEnumerable<IMetric> metrics)
        {
            foreach (var metric in metrics)
            {
                AddMetric(metric);
            }
        }

        public void RemoveMetrics(IEnumerable<IMetric> metrics)
        {
            foreach (var metric in metrics)
            {
                _SystemMetrics.Remove(metric);
            }
        }

        public ObservableCollection<Message> SystemMessages
        {
            get => _SystemMessages;
        }

        /// <summary>
        /// Adds a 'system message' 
        /// </summary>
        /// <param name="message">The message to be sent</param>
        /// <param name="notify">If the application user should be notified - calling Alert event</param>
        public void AddSystemMessage(string message, bool notify = false)
        {
            _SystemMessages.Insert(0, new(message, DateTime.Now));
            if (notify)
            {
                Alert();
            }
        }

        /// <summary>
        ///  Adds a 'system message' with the 'notify' flag set
        ///  <see cref="AddSystemMessage(string, bool)"/>
        /// </summary>
        public void Notify(string message) => AddSystemMessage(message, notify: true);

        public void NotifyDelayed(int millisecondsDelay, string message) => Task.Run(async () =>
        {
            await Task.Delay(millisecondsDelay);
            Notify(message);
        });

        private async void MetricMonitor()
        {
            // Run monitor for life of the application
            while(true)
            {
                foreach (var metric in _SystemMetrics)
                {
                    if (metric.Poller == null)
                    {
                        continue;
                    }
                    await metric.Poller();
                }
                await Task.Delay(1000);
            }
        }
    }

    /// <summary>
    /// Container class for system messages/error notifications
    /// </summary>
    /// <param name="Content">The actual message content</param>
    public readonly record struct Message(string Content, DateTime Timestamp)
    {
        public override readonly string? ToString() => $"{Timestamp:HH:mm} - {Content}";
    }
}
