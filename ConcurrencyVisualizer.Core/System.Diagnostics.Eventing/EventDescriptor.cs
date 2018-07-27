namespace System.Diagnostics.Eventing
{
    using Runtime.InteropServices;
    using Security.Permissions;

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public struct EventDescriptor
    {
        // Fields
        [FieldOffset(0)]
        private ushort m_id;
        [FieldOffset(2)]
        private byte m_version;
        [FieldOffset(3)]
        private byte m_channel;
        [FieldOffset(4)]
        private byte m_level;
        [FieldOffset(5)]
        private byte m_opcode;
        [FieldOffset(6)]
        private ushort m_task;
        [FieldOffset(8)]
        private long m_keywords;

        // Methods
        public EventDescriptor(int id, byte version, byte channel, byte level, byte opcode, int task, long keywords)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Non negative number is required.");
            }
            if (id > 0xffff)
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"The ID parameter must be in the range {1} through {0xffff}.");
            }

            this.m_id = (ushort)id;
            this.m_version = version;
            this.m_channel = channel;
            this.m_level = level;
            this.m_opcode = opcode;
            this.m_keywords = keywords;
            if (task < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(task), "Non negative number is required.");
            }
            if (task > 0xffff)
            {
                object[] args = new object[] { 1, (ushort)0xffff };
                throw new ArgumentOutOfRangeException(nameof(task), $"The ID parameter must be in the range {1} through {0xffff}.");
            }

            this.m_task = (ushort)task;
        }

        // Properties
        public int EventId =>
            this.m_id;

        public byte Version =>
            this.m_version;

        public byte Channel =>
            this.m_channel;

        public byte Level =>
            this.m_level;

        public byte Opcode =>
            this.m_opcode;

        public int Task =>
            this.m_task;

        public long Keywords =>
            this.m_keywords;
    }
}
