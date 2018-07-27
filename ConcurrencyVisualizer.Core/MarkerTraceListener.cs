namespace Microsoft.ConcurrencyVisualizer.Instrumentation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    internal sealed class MarkerTraceListener : TraceListener
    {
        private static readonly Dictionary<string, MarkerWriter> writers = new Dictionary<string, MarkerWriter>();
        private static readonly object lockObject = new object();
        private MarkerWriter writer;
        private MarkerSeries series;
        private bool isDisposed;

        static MarkerTraceListener()
        {
            try
            {
                writers.Add(MarkerWriter.DefaultProviderGuid.ToString("D"), new MarkerWriter(MarkerWriter.DefaultProviderGuid));
            }
            catch (Exception)
            {
            }
        }

        public MarkerTraceListener() : this(null, null)
        {
        }

        public MarkerTraceListener(string initializeData) : base("MarkerTraceListener")
        {
            if (string.IsNullOrEmpty(initializeData))
            {
                this.InitializeProvider(null, null);
            }
            else
            {
                char[] separator = new char[] { ';' };
                string[] strArray = initializeData.Split(separator);
                this.InitializeProvider((strArray.Length > 1) ? strArray[1].Trim() : null, strArray[0].Trim());
            }
        }

        public MarkerTraceListener(string providerId, string seriesName) : base("MarkerTraceListener")
        {
            this.InitializeProvider(providerId, seriesName);
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                this.series = null;
                this.writer.Dispose();
                this.isDisposed = true;
            }
            base.Dispose(disposing);
        }

        public override void Fail(string message)
        {
            this.series.WriteAlert(message);
        }

        public override void Fail(string message, string detailMessage)
        {
            if (this.series.IsEnabled())
            {
                this.series.WriteAlert(message + "\r\nDetails:\r\n" + detailMessage);
            }
        }

        private void InitializeProvider(string providerId, string seriesName)
        {
            Guid guid = !string.IsNullOrEmpty(providerId) ? new Guid(providerId) : MarkerWriter.DefaultProviderGuid;
            string key = guid.ToString("D");
            if (!writers.TryGetValue(key, out this.writer))
            {
                object lockObject = MarkerTraceListener.lockObject;
                lock (lockObject)
                {
                    if (!writers.TryGetValue(key, out this.writer))
                    {
                        this.writer = new MarkerWriter(guid);
                        writers[key] = this.writer;
                    }
                }
            }

            this.series = !string.IsNullOrEmpty(seriesName) ? this.writer.CreateMarkerSeries(seriesName) : this.writer.DefaultSeries;
        }

        private bool IsStackTraceEnabled(TraceEventCache eventCache)
        {
            return ((eventCache != null) && ((this.TraceOutputOptions & TraceOptions.Callstack) > TraceOptions.None));
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (this.series.IsEnabled() && ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                StringBuilder builder = new StringBuilder(0x200);
                builder.Append((data != null) ? data.ToString() : "null");
                if (this.IsStackTraceEnabled(eventCache))
                {
                    builder.Append("\r\nCallstack:\r\n");
                    builder.Append(eventCache.Callstack);
                    this.WriteEvent(eventType, id, builder.ToString());
                }
                else
                {
                    this.WriteEvent(eventType, id, builder.ToString());
                }
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if (this.series.IsEnabled() && ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                StringBuilder builder = new StringBuilder(0x200);
                if (data != null)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        builder.Append((data[i] != null) ? data[i].ToString() : "null");
                        builder.Append("\r\n");
                    }
                }
                if (this.IsStackTraceEnabled(eventCache))
                {
                    builder.Append("\r\nCallstack:\r\n");
                    builder.Append(eventCache.Callstack);
                    this.WriteEvent(eventType, id, builder.ToString());
                }
                else
                {
                    this.WriteEvent(eventType, id, builder.ToString());
                }
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (this.series.IsEnabled() && ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                if (this.IsStackTraceEnabled(eventCache))
                {
                    this.WriteEvent(eventType, id, "Callstack:\r\n" + eventCache.Callstack);
                }
                else
                {
                    this.WriteEvent(eventType, id, string.Empty);
                }
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (this.series.IsEnabled() && ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                if (this.IsStackTraceEnabled(eventCache))
                {
                    this.WriteEvent(eventType, id, message + "\r\nCallstack:\r\n" + eventCache.Callstack);
                }
                else
                {
                    this.WriteEvent(eventType, id, message);
                }
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (this.series.IsEnabled() && ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                StringBuilder builder = new StringBuilder(0x200);
                if ((args != null) && (args.Length != 0))
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, format, args);
                }
                else
                {
                    builder.Append(format);
                }
                if (this.IsStackTraceEnabled(eventCache))
                {
                    builder.Append("\r\nCallstack:\r\n");
                    builder.Append(eventCache.Callstack);
                    this.WriteEvent(eventType, id, builder.ToString());
                }
                else
                {
                    this.WriteEvent(eventType, id, builder.ToString());
                }
            }
        }

        public override void Write(string message)
        {
            this.series.WriteMessage(message);
        }

        private void WriteEvent(TraceEventType eventType, int id, string text)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    this.series.WriteFlag(Importance.Critical, id, text);
                    return;

                case TraceEventType.Warning:
                    this.series.WriteFlag(Importance.High, id, text);
                    return;

                case TraceEventType.Information:
                    this.series.WriteMessage(id, text);
                    return;

                case TraceEventType.Verbose:
                    this.series.WriteMessage(Importance.Low, id, text);
                    return;
            }

            this.series.WriteMessage(Importance.Low, id, text);
        }

        public override void WriteLine(string message)
        {
            this.Write(message);
        }
    }
}

