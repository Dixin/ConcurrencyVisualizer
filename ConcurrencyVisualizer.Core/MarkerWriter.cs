namespace Microsoft.ConcurrencyVisualizer.Instrumentation
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;

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

        private static System.Diagnostics.Eventing.EventDescriptor enterSpanEvent;

        private static System.Diagnostics.Eventing.EventDescriptor leaveSpanEvent;

        private static System.Diagnostics.Eventing.EventDescriptor flagEvent;

        private static System.Diagnostics.Eventing.EventDescriptor messageEvent;

        private byte traceLevel;

        private long anyKeywordMask;

        private long allKeywordMask;

        private bool enabled;

        private volatile int currentSpanId = -2147483648;

        private volatile int manifestWritten;

        private volatile int disposed;

        public MarkerSeries DefaultSeries
        {
            get;
        }

        static MarkerWriter()
        {
            DefaultProviderGuid = new Guid("8D4925AB-505A-483b-A7E0-6F824A07A6F0");
            enterSpanEvent = new System.Diagnostics.Eventing.EventDescriptor(1, 1, 16, 4, 1, 1, -9223372036854775808L);
            leaveSpanEvent = new System.Diagnostics.Eventing.EventDescriptor(2, 1, 16, 4, 2, 1, -9223372036854775808L);
            flagEvent = new System.Diagnostics.Eventing.EventDescriptor(3, 1, 16, 4, 11, 2, -9223372036854775808L);
            messageEvent = new System.Diagnostics.Eventing.EventDescriptor(4, 1, 16, 4, 12, 3, -9223372036854775808L);
        }

        public unsafe MarkerWriter(Guid providerId)
        {
            this.ProviderId = providerId;
            using (StreamReader streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.ConcurrencyVisualizer.Instrumentation.Resources.ConcurrencyVisualizerMarkers.man")))
            {
                this.manifest = Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, streamReader.ReadToEnd(), new object[1]
                {
                providerId
                }));
            }

            this.etwCallback = this.EtwEnableCallback;
            uint num = NativeMethods.EventRegister(ref this.ProviderId, this.etwCallback, null, ref this.regHandle);
            if (num != 0)
            {
                throw new Win32Exception((int)num, string.Format(CultureInfo.InvariantCulture, "Failed to register ETW provider: {0}.", new object[1]
                {
                providerId
                }));
            }

            this.DefaultSeries = this.CreateMarkerSeries(MarkerSeries.DefaultSeriesName);
        }

        ~MarkerWriter()
        {
            this.Dispose(false);
        }

        public MarkerSeries CreateMarkerSeries(string seriesName)
        {
            return new MarkerSeries(this, seriesName);
        }

        public bool IsEnabled()
        {
            return this.enabled;
        }

        public bool IsEnabled(Importance level)
        {
            if (this.enabled)
            {
                return this.IsLevelEnabled(level);
            }
            return false;
        }

        public bool IsEnabled(int category)
        {
            if (this.enabled)
            {
                return this.IsCategoryEnabled(category);
            }
            return false;
        }

        public bool IsEnabled(Importance level, int category)
        {
            if (this.enabled && this.IsLevelEnabled(level))
            {
                return this.IsCategoryEnabled(category);
            }
            return false;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal int GetNewSpanId()
        {
            return Interlocked.Increment(ref this.currentSpanId);
        }

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
            switch (eventType)
            {
                case MarkerEventType.EnterSpan:
                    return this.WriteMarkerEvent(ref enterSpanEvent, level, category, spanId, seriesName, text);
                case MarkerEventType.LeaveSpan:
                    return this.WriteMarkerEvent(ref leaveSpanEvent, level, category, spanId, seriesName, string.Empty);
                case MarkerEventType.Flag:
                    return this.WriteMarkerEvent(ref flagEvent, level, category, 0, seriesName, text);
                case MarkerEventType.Message:
                    return this.WriteMarkerEvent(ref messageEvent, level, category, 0, seriesName, text);
                default:
                    return false;
            }
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed != 1 && Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0)
            {
                this.enabled = false;
                NativeMethods.EventUnregister(this.regHandle);
            }
        }

        private bool IsLevelEnabled(Importance level)
        {
            if ((byte)level > this.traceLevel)
            {
                return this.traceLevel == 0;
            }
            return true;
        }

        private bool IsCategoryEnabled(int category)
        {
            long num = FromCategoryToKeyword(category);
            if ((this.anyKeywordMask & num) != 0L)
            {
                return (this.allKeywordMask & num) == this.allKeywordMask;
            }
            return false;
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
            System.Diagnostics.Eventing.EventDescriptor eventDescriptor = new System.Diagnostics.Eventing.EventDescriptor(65534, 1, 0, 0, 254, 0, -1L);
            ManifestEnvelope manifestEnvelope = default(ManifestEnvelope);
            manifestEnvelope.Format = ManifestEnvelope.ManifestFormats.SimpleXmlFormat;
            manifestEnvelope.MajorVersion = 1;
            manifestEnvelope.MinorVersion = 0;
            manifestEnvelope.Magic = 91;
            manifestEnvelope.TotalChunks = 1;
            manifestEnvelope.ChunkNumber = 0;
            ManifestEnvelope manifestEnvelope2 = manifestEnvelope;
            NativeMethods.EventData* ptr = stackalloc NativeMethods.EventData[2];
            ptr->Ptr = (ulong)&manifestEnvelope2;
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

        private unsafe bool WriteMarkerEvent(ref System.Diagnostics.Eventing.EventDescriptor sourceDescriptor, Importance level, int category, int spanId, string markerSeries, string text)
        {
            int userDataCount = sourceDescriptor.EventId == 1 || sourceDescriptor.EventId == 2 ? 7 : 6;
            System.Diagnostics.Eventing.EventDescriptor eventDescriptor = new System.Diagnostics.Eventing.EventDescriptor(sourceDescriptor.EventId, sourceDescriptor.Version, sourceDescriptor.Channel, (byte)level, sourceDescriptor.Opcode, sourceDescriptor.Task, FromCategoryToKeyword(category));
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
            return -9223372036854775808L | 1L << num;
        }
    }
}

