namespace Microsoft.ConcurrencyVisualizer.Instrumentation;

internal sealed class MarkerTraceListener : TraceListener
{
    private static readonly Dictionary<string, MarkerWriter> writers;

    private static readonly object lockObject;

    private MarkerWriter writer;

    private MarkerSeries series;

    private bool isDisposed;

    static MarkerTraceListener()
    {
        lockObject = new object();
        writers = new Dictionary<string, MarkerWriter>();
        try
        {
            writers.Add(MarkerWriter.DefaultProviderGuid.ToString("D"), new MarkerWriter(MarkerWriter.DefaultProviderGuid));
        }
        catch (Exception)
        {
        }
    }

    public MarkerTraceListener()
        : this(null, null)
    {
    }

    public MarkerTraceListener(string initializeData)
        : base("MarkerTraceListener")
    {
        if (string.IsNullOrEmpty(initializeData))
        {
            this.InitializeProvider(null, null);
            return;
        }
        string[] array = initializeData.Split(';');
        this.InitializeProvider(array.Length > 1 ? array[1].Trim() : null, array[0].Trim());
    }

    public MarkerTraceListener(string providerId, string seriesName)
        : base("MarkerTraceListener") =>
        this.InitializeProvider(providerId, seriesName);

    public override void Write(string message) => this.series.WriteMessage(message);

    public override void WriteLine(string message) => this.Write(message);

    public override void Fail(string message) => this.series.WriteAlert(message);

    public override void Fail(string message, string detailMessage)
    {
        if (this.series.IsEnabled())
        {
            this.series.WriteAlert(message + "\r\nDetails:\r\n" + detailMessage);
        }
    }

    public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
    {
        if (this.series.IsEnabled() && (this.Filter is null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
        {
            StringBuilder stringBuilder = new (512);
            stringBuilder.Append(data is null ? "null" : data.ToString());
            if (this.IsStackTraceEnabled(eventCache))
            {
                stringBuilder.Append("\r\nCallstack:\r\n");
                stringBuilder.Append(eventCache.Callstack);
                this.WriteEvent(eventType, id, stringBuilder.ToString());
            }
            else
            {
                this.WriteEvent(eventType, id, stringBuilder.ToString());
            }
        }
    }

    public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
    {
        if (!this.series.IsEnabled() || this.Filter is not null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null))
        {
            return;
        }
        StringBuilder stringBuilder = new (512);
        if (data is not null)
        {
            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i] is not null ? data[i].ToString() : "null");
                stringBuilder.Append("\r\n");
            }
        }
        if (this.IsStackTraceEnabled(eventCache))
        {
            stringBuilder.Append("\r\nCallstack:\r\n");
            stringBuilder.Append(eventCache.Callstack);
            this.WriteEvent(eventType, id, stringBuilder.ToString());
        }
        else
        {
            this.WriteEvent(eventType, id, stringBuilder.ToString());
        }
    }

    public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
    {
        if (this.series.IsEnabled() && (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
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
        if (this.series.IsEnabled() && (this.Filter is null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
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
        if (this.series.IsEnabled() && (this.Filter is null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
        {
            StringBuilder stringBuilder = new (512);
            if (args is not null && args.Length != 0)
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, format, args);
            }
            else
            {
                stringBuilder.Append(format);
            }
            if (this.IsStackTraceEnabled(eventCache))
            {
                stringBuilder.Append("\r\nCallstack:\r\n");
                stringBuilder.Append(eventCache.Callstack);
                this.WriteEvent(eventType, id, stringBuilder.ToString());
            }
            else
            {
                this.WriteEvent(eventType, id, stringBuilder.ToString());
            }
        }
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

    private bool IsStackTraceEnabled(TraceEventCache eventCache) => 
        eventCache is not null && (this.TraceOutputOptions & TraceOptions.Callstack) != 0;

    private void WriteEvent(TraceEventType eventType, int id, string text)
    {
        switch (eventType)
        {
            case TraceEventType.Critical:
            case TraceEventType.Error:
                this.series.WriteFlag(Importance.Critical, id, text);
                break;
            case TraceEventType.Warning:
                this.series.WriteFlag(Importance.High, id, text);
                break;
            case TraceEventType.Information:
                this.series.WriteMessage(id, text);
                break;
            case TraceEventType.Verbose:
                this.series.WriteMessage(Importance.Low, id, text);
                break;
            default:
                this.series.WriteMessage(Importance.Low, id, text);
                break;
        }
    }

    private void InitializeProvider(string providerId, string seriesName)
    {
        Guid providerId2 = !string.IsNullOrEmpty(providerId) ? new Guid(providerId) : MarkerWriter.DefaultProviderGuid;
        string key = providerId2.ToString("D");
        if (!writers.TryGetValue(key, out this.writer))
        {
            lock (lockObject)
            {
                if (!writers.TryGetValue(key, out this.writer))
                {
                    this.writer = new MarkerWriter(providerId2);
                    writers[key] = this.writer;
                }
            }
        }

        this.series = !string.IsNullOrEmpty(seriesName) ? this.writer.CreateMarkerSeries(seriesName) : this.writer.DefaultSeries;
    }
}