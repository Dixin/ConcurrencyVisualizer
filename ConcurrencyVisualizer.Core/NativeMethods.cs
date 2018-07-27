namespace Microsoft.ConcurrencyVisualizer.Instrumentation
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Eventing;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        internal const string Advapi32 = "advapi32.dll";
        internal const int ErrorSuccess = 0;

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventRegister([In] ref Guid providerId, [In] EtwEnableCallback enableCallback, [In] void* callbackContext, [In][Out] ref long registrationHandle);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern uint EventUnregister([In] long registrationHandle);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern unsafe uint EventWrite([In] long registrationHandle, [In] ref EventDescriptor eventDescriptor, [In] uint userDataCount, [In] EventData* userData);

        internal unsafe delegate void EtwEnableCallback([In] ref Guid sourceId, [In] int isEnabled, [In] byte level, [In] long matchAnyKeywords, [In] long matchAllKeywords, [In] EventFilterDescriptor* filterData, [In] void* callbackContext);

        [StructLayout(LayoutKind.Sequential)]
        internal struct EventData
        {
            internal ulong Ptr;
            internal uint Size;
            internal uint Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct EventFilterDescriptor
        {
            public long Ptr;
            public int Size;
            public int Type;
        }
    }
}

