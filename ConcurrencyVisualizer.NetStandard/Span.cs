namespace Microsoft.ConcurrencyVisualizer.Instrumentation
{
    using System;
    using System.ComponentModel;

    public sealed class Span : IDisposable
    {
        private readonly MarkerSeries series;

        internal Span(MarkerSeries series, Importance level, int category, int spanId)
        {
            this.series = series;
            this.Level = level;
            this.Category = category;
            this.SpanId = spanId;
        }

        internal Importance Level { get; }

        internal int Category { get; }

        internal int SpanId { get; }

        public void Leave() => this.Dispose();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Dispose() => this.series.LeaveSpan(this);
    }
}
