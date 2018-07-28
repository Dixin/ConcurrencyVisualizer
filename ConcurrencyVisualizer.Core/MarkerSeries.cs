namespace Microsoft.ConcurrencyVisualizer.Instrumentation
{
    public class MarkerSeries
    {
        public static readonly string DefaultSeriesName = string.Empty;
        public readonly string SeriesName;
        private const int CategoryDefault = 0;
        private const int CategoryFlagDefault = 1;
        public const int CategoryAlert = -1;

        internal MarkerSeries(MarkerWriter writer, string seriesName)
        {
            this.Writer = writer;
            this.SeriesName = seriesName;
        }

        public Span EnterSpan(string text) => 
            this.EnterSpan(Importance.Normal, 0, text);

        public Span EnterSpan(Importance level, string text) => 
            this.EnterSpan(level, 0, text);

        public Span EnterSpan(int category, string text) => 
            this.EnterSpan(Importance.Normal, category, text);

        public Span EnterSpan(string format, params object[] args) => 
            this.EnterSpan(string.Format(format, args));

        public Span EnterSpan(Importance level, int category, string text)
        {
            Span span = new Span(this, level, category, this.Writer.GetNewSpanId());
            this.Writer.WriteMarkerEvent(MarkerEventType.EnterSpan, this.SeriesName, level, category, span.SpanId, text);
            return span;
        }

        public Span EnterSpan(Importance level, string format, params object[] args) => 
            this.EnterSpan(level, string.Format(format, args));

        public Span EnterSpan(int category, string format, params object[] args) => 
            this.EnterSpan(category, string.Format(format, args));

        public Span EnterSpan(Importance level, int category, string format, params object[] args) => 
            this.EnterSpan(level, category, string.Format(format, args));

        public bool IsEnabled() => 
            this.Writer.IsEnabled();

        public bool IsEnabled(Importance level) => 
            this.Writer.IsEnabled(level);

        public bool IsEnabled(int category) => 
            this.Writer.IsEnabled(category);

        public bool IsEnabled(Importance level, int category) => 
            this.Writer.IsEnabled(level, category);

        internal void LeaveSpan(Span span)
        {
            this.Writer.WriteMarkerEvent(MarkerEventType.LeaveSpan, this.SeriesName, span.Level, span.Category, span.SpanId, string.Empty);
        }

        public void WriteAlert(string text)
        {
            this.WriteFlag(Importance.Critical, -1, text);
        }

        public void WriteAlert(string format, params object[] args)
        {
            this.WriteFlag(Importance.Critical, -1, format, args);
        }

        public void WriteFlag(string text)
        {
            this.WriteFlag(Importance.Normal, 1, text);
        }

        public void WriteFlag(Importance level, string text)
        {
            this.WriteFlag(level, 1, text);
        }

        public void WriteFlag(int category, string text)
        {
            this.WriteFlag(Importance.Normal, category, text);
        }

        public void WriteFlag(string format, params object[] args)
        {
            if (this.Writer.IsEnabled())
            {
                this.WriteFlag(string.Format(format, args));
            }
        }

        public void WriteFlag(Importance level, int category, string text)
        {
            this.Writer.WriteMarkerEvent(MarkerEventType.Flag, this.SeriesName, level, category, 0, text);
        }

        public void WriteFlag(Importance level, string format, params object[] args)
        {
            if (this.Writer.IsEnabled(level))
            {
                this.WriteFlag(level, string.Format(format, args));
            }
        }

        public void WriteFlag(int category, string format, params object[] args)
        {
            if (this.Writer.IsEnabled(category))
            {
                this.WriteFlag(category, string.Format(format, args));
            }
        }

        public void WriteFlag(Importance level, int category, string format, params object[] args)
        {
            if (this.Writer.IsEnabled(level, category))
            {
                this.WriteFlag(level, category, string.Format(format, args));
            }
        }

        public void WriteMessage(string text)
        {
            this.WriteMessage(Importance.Normal, 0, text);
        }

        public void WriteMessage(Importance level, string text)
        {
            this.WriteMessage(level, 0, text);
        }

        public void WriteMessage(int category, string text)
        {
            this.WriteMessage(Importance.Normal, category, text);
        }

        public void WriteMessage(string format, params object[] args)
        {
            if (this.Writer.IsEnabled())
            {
                this.WriteMessage(string.Format(format, args));
            }
        }

        public void WriteMessage(Importance level, int category, string text)
        {
            this.Writer.WriteMarkerEvent(MarkerEventType.Message, this.SeriesName, level, category, 0, text);
        }

        public void WriteMessage(Importance level, string format, params object[] args)
        {
            if (this.Writer.IsEnabled(level))
            {
                this.WriteMessage(level, string.Format(format, args));
            }
        }

        public void WriteMessage(int category, string format, params object[] args)
        {
            if (this.Writer.IsEnabled(category))
            {
                this.WriteMessage(category, string.Format(format, args));
            }
        }

        public void WriteMessage(Importance level, int category, string format, params object[] args)
        {
            if (this.Writer.IsEnabled(level, category))
            {
                this.WriteMessage(level, category, string.Format(format, args));
            }
        }

        public MarkerWriter Writer { get; }
    }
}

