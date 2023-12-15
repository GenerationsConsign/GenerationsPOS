using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace GenerationsPOS.PointOfSale.Hardware.Metrics
{
    public delegate Task MetricPoll();
 
    public interface IMetric
    {
        public MetricPoll? Poller { get; }
        public string Name { get; }
        public string Status { get; }
        public bool IsIssue { get; }
    }

    /// <summary>
    /// Container class for individual system metrics
    /// </summary>
    /// <param name="Name">The name of the metric (will be rendered)</param>
    public partial class Metric<T> : ObservableObject, IMetric
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Status), nameof(IsIssue))]
        private T _CurrentState;
        private readonly Func<T, string> Formatter;
        private readonly Predicate<T> IssueDetected;
        private readonly Action? AlertIssue;

        /// <summary>
        /// Create a new system hardware metric. 
        /// Will need to pass to UI layer to be useful for user.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="initialState"></param>
        /// <param name="formatter"></param>
        /// <param name="issuePredicate"></param>
        /// <param name="poller"></param>
        /// <param name="alertIssue"></param>
        public Metric(string name, T initialState, Func<T, string> formatter, Predicate<T> issuePredicate, MetricPoll? poller = null, Action? alertIssue = null)
        {
            Name = name;
            CurrentState = initialState;
            Formatter = formatter;
            IssueDetected = issuePredicate;
            Poller = poller;
            AlertIssue = alertIssue;

            OnCurrentStateChanged(initialState);
        }

        public string Name { get; private set; }

        public MetricPoll? Poller { get; private set; }

        partial void OnCurrentStateChanged(T value)
        {
            if(IsIssue && AlertIssue != null)
            {
                AlertIssue();
            }
        }

        public string Status
        {
            get => Formatter(CurrentState);
        }

        public bool IsIssue
        {
            get => IssueDetected(CurrentState);
        }

        public override bool Equals(object? obj)
        {
            return obj is Metric<T> metric &&
                   Name == metric.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
