namespace Microsoft.ConcurrencyVisualizer.Instrumentation;

using EventDescriptor = System.Diagnostics.Eventing.EventDescriptor;

public sealed class MarkerWriter : IDisposable
{
    internal struct ManifestEnvelope
    {
        public enum ManifestFormats : byte
        {
            SimpleXmlFormat = 1
        }

        public const int MaxChunkSize = 65280;

        public ManifestFormats Format;

        public byte MajorVersion;

        public byte MinorVersion;

        public byte Magic;

        public ushort TotalChunks;

        public ushort ChunkNumber;
    }

    public static readonly Guid DefaultProviderGuid;

    public readonly Guid ProviderId;

    private const int EnterSpanEventId = 1;

    private const int LeaveSpanEventId = 2;

    private const int FlagEventId = 3;

    private const int MessageEventId = 4;

    private readonly long regHandle;

    private readonly byte[] manifest;

    private readonly NativeMethods.EtwEnableCallback etwCallback;

    private static EventDescriptor enterSpanEvent;

    private static EventDescriptor leaveSpanEvent;

    private static EventDescriptor flagEvent;

    private static EventDescriptor messageEvent;

    private byte traceLevel;

    private long anyKeywordMask;

    private long allKeywordMask;

    private bool enabled;

    private volatile int currentSpanId = int.MinValue;

    private volatile int manifestWritten;

    private volatile int disposed;

    public MarkerSeries DefaultSeries
    {
        get;
    }

    static MarkerWriter()
    {
        DefaultProviderGuid = new Guid("8D4925AB-505A-483b-A7E0-6F824A07A6F0");
        enterSpanEvent = new EventDescriptor(1, 1, 16, 4, 1, 1, long.MinValue);
        leaveSpanEvent = new EventDescriptor(2, 1, 16, 4, 2, 1, long.MinValue);
        flagEvent = new EventDescriptor(3, 1, 16, 4, 11, 2, long.MinValue);
        messageEvent = new EventDescriptor(4, 1, 16, 4, 12, 3, long.MinValue);
    }

    public unsafe MarkerWriter(Guid providerId)
    {
        this.ProviderId = providerId;
        using (StreamReader streamReader = new (Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.ConcurrencyVisualizer.Instrumentation.Resources.ConcurrencyVisualizerMarkers.man")))
        {
            this.manifest = Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, streamReader.ReadToEnd(), providerId));
        }

        this.etwCallback = this.EtwEnableCallback;
        uint num = NativeMethods.EventRegister(ref this.ProviderId, this.etwCallback, null, ref this.regHandle);
        if (num != 0)
        {
            throw new Win32Exception((int)num, string.Format(CultureInfo.InvariantCulture, "Failed to register ETW provider: {0}.", providerId));
        }

        this.DefaultSeries = this.CreateMarkerSeries(MarkerSeries.DefaultSeriesName);
    }

    ~MarkerWriter() => this.Dispose(disposing: false);

    public MarkerSeries CreateMarkerSeries(string seriesName) => new (this, seriesName);

    public bool IsEnabled() => this.enabled;

    public bool IsEnabled(Importance level) => this.enabled && this.IsLevelEnabled(level);

    public bool IsEnabled(int category) => this.enabled && this.IsCategoryEnabled(category);

    public bool IsEnabled(Importance level, int category) => this.enabled && this.IsLevelEnabled(level) && this.IsCategoryEnabled(category);

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal int GetNewSpanId() => Interlocked.Increment(ref this.currentSpanId);

    internal bool WriteMarkerEvent(MarkerEventType eventType, string seriesName, Importance level, int category, int spanId, string text)
    {
        if (level == Importance.Undefined)
        {
            return true;
        }
        if (!this.IsEnabled(level, category))
        {
            return true;
        }
        if (Interlocked.CompareExchange(ref this.manifestWritten, 1, 0) == 0)
        {
            this.WriteManifestEvent();
        }

        return eventType switch
        {
            MarkerEventType.EnterSpan => this.WriteMarkerEvent(ref enterSpanEvent, level, category, spanId, seriesName, text),
            MarkerEventType.LeaveSpan => this.WriteMarkerEvent(ref leaveSpanEvent, level, category, spanId, seriesName, string.Empty),
            MarkerEventType.Flag => this.WriteMarkerEvent(ref flagEvent, level, category, 0, seriesName, text),
            MarkerEventType.Message => this.WriteMarkerEvent(ref messageEvent, level, category, 0, seriesName, text),
            _ => false
        };
    }

    private void Dispose(bool disposing)
    {
        if (this.disposed != 1 && Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0)
        {
            this.enabled = false;
            NativeMethods.EventUnregister(this.regHandle);
        }
    }

    private bool IsLevelEnabled(Importance level) => (byte) level <= this.traceLevel || this.traceLevel == 0;

    private bool IsCategoryEnabled(int category)
    {
        long num = FromCategoryToKeyword(category);
        return (this.anyKeywordMask & num) != 0L && (this.allKeywordMask & num) == this.allKeywordMask;
    }

    private unsafe void EtwEnableCallback(ref Guid sourceId, int isEnabled, byte setLevel, long anyKeyword, long allKeyword, NativeMethods.EventFilterDescriptor* filterData, void* callbackContext)
    {
        this.enabled = isEnabled != 0;
        this.traceLevel = setLevel;
        this.anyKeywordMask = anyKeyword;
        this.allKeywordMask = allKeyword;
        if (!this.IsEnabled())
        {
            this.manifestWritten = 0;
        }
    }

    private unsafe bool WriteManifestEvent()
    {
        EventDescriptor eventDescriptor = new (65534, 1, 0, 0, 254, 0, -1L);
        ManifestEnvelope manifestEnvelope = new ()
        {
            Format = ManifestEnvelope.ManifestFormats.SimpleXmlFormat,
            MajorVersion = 1,
            MinorVersion = 0,
            Magic = 91,
            TotalChunks = 1,
            ChunkNumber = 0
        };
        NativeMethods.EventData* ptr = stackalloc NativeMethods.EventData[2];
        ptr->Ptr = (ulong)&manifestEnvelope;
        ptr->Size = (uint)sizeof(ManifestEnvelope);
        ptr[1].Size = (uint)this.manifest.Length;
        bool result;
        fixed (byte* ptr2 = this.manifest)
        {
            ptr[1].Ptr = (ulong)ptr2;
            result = NativeMethods.EventWrite(this.regHandle, ref eventDescriptor, 2u, ptr) == 0;
        }
        return result;
    }

    private unsafe bool WriteMarkerEvent(ref EventDescriptor sourceDescriptor, Importance level, int category, int spanId, string markerSeries, string text)
    {
        int userDataCount = sourceDescriptor.EventId == 1 || sourceDescriptor.EventId == 2 ? 7 : 6;
        EventDescriptor eventDescriptor = new (sourceDescriptor.EventId, sourceDescriptor.Version, sourceDescriptor.Channel, (byte)level, sourceDescriptor.Opcode, sourceDescriptor.Task, FromCategoryToKeyword(category));
        NativeMethods.EventData* ptr = stackalloc NativeMethods.EventData[7];
        byte b = (byte)sourceDescriptor.EventId;
        ptr->Ptr = (ulong)&b;
        ptr->Size = 1u;
        ptr[1].Ptr = (ulong)&level;
        ptr[1].Size = 1u;
        ptr[2].Ptr = (ulong)&category;
        ptr[2].Size = 1u;
        int num = 3;
        if (sourceDescriptor.EventId == 1 || sourceDescriptor.EventId == 2)
        {
            ptr[3].Ptr = (ulong)&spanId;
            ptr[3].Size = 4u;
            num = 4;
        }
        ptr[num].Size = (uint)(((!string.IsNullOrEmpty(markerSeries) ? markerSeries.Length : 0) + 1) * 2);
        ptr[num + 1].Size = (uint)(((!string.IsNullOrEmpty(text) ? text.Length : 0) + 1) * 2);
        ptr[num + 2].Size = 1u;
        bool result;
        fixed (char* ptr2 = string.IsNullOrEmpty(markerSeries) ? string.Empty : markerSeries)
        {
            fixed (char* ptr3 = string.IsNullOrEmpty(text) ? string.Empty : text)
            {
                fixed (char* ptr4 = string.Empty)
                {
                    ptr[num].Ptr = (ulong)ptr2;
                    ptr[num + 1].Ptr = (ulong)ptr3;
                    ptr[num + 2].Ptr = (ulong)ptr4;
                    result = NativeMethods.EventWrite(this.regHandle, ref eventDescriptor, (uint)userDataCount, ptr) == 0;
                }
            }
        }
        return result;
    }

    internal static long FromCategoryToKeyword(int category)
    {
        int num = category == -1 ? 62 : (category >= 0 ? category : 0) % 48;
        return long.MinValue | (1L << num);
    }
}