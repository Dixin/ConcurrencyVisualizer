namespace Microsoft.ConcurrencyVisualizer.Instrumentation;

using EventDescriptor = System.Diagnostics.Eventing.EventDescriptor;

[SuppressUnmanagedCodeSecurity]
internal static class NativeMethods
{
    internal unsafe delegate void EtwEnableCallback([In] ref Guid sourceId, [In] int isEnabled, [In] byte level, [In] long matchAnyKeywords, [In] long matchAllKeywords, [In] EventFilterDescriptor* filterData, [In] void* callbackContext);

    internal struct EventData
    {
        internal ulong Ptr;

        internal uint Size;

        internal uint Reserved;
    }

    internal struct EventFilterDescriptor
    {
        public long Ptr;

        public int Size;

        public int Type;

    }

    internal const string Advapi32 = "advapi32.dll";

    internal const int ErrorSuccess = 0;

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
    internal static extern unsafe uint EventRegister([In] ref Guid providerId, [In] EtwEnableCallback enableCallback, [In] void* callbackContext, [In][Out] ref long registrationHandle);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
    internal static extern uint EventUnregister([In] long registrationHandle);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
    internal static extern unsafe uint EventWrite([In] long registrationHandle, [In] ref EventDescriptor eventDescriptor, [In] uint userDataCount, [In] EventData* userData);
}