namespace System.Diagnostics.Eventing
{
    using Runtime.InteropServices;
    using Security.Permissions;

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public struct EventDescriptor
    {
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

        /// <summary>Retrieves the event identifier value from the event descriptor.</summary>
        /// <returns>The event identifier.</returns>
        public int EventId => this.m_id;

        /// <summary>Retrieves the version value from the event descriptor.</summary>
        /// <returns>The version of the event. </returns>
        public byte Version => this.m_version;

        /// <summary>Retrieves the channel value from the event descriptor.</summary>
        /// <returns>The channel that defines a potential target for the event.</returns>
        public byte Channel => this.m_channel;

        /// <summary>Retrieves the level value from the event descriptor.</summary>
        /// <returns>The level of detail included in the event.</returns>
        public byte Level => this.m_level;

        /// <summary>Retrieves the operation code value from the event descriptor.</summary>
        /// <returns>The operation being performed at the time the event is written.</returns>
        public byte Opcode => this.m_opcode;

        /// <summary>Retrieves the task value from the event descriptor.</summary>
        /// <returns>The task that identifies the logical component of the application that is writing the event.</returns>
        public int Task => this.m_task;

        /// <summary>Retrieves the keyword value from the event descriptor.</summary>
        /// <returns>The keyword, which is a bit mask, that specifies the event category.</returns>
        public long Keywords => this.m_keywords;

        /// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.EventDescriptor" /> class.</summary>
        /// <param name="id">The event identifier.</param>
        /// <param name="version">Version of the event. The version indicates a revision to the event definition. You can use this member and the Id member to identify a unique event.</param>
        /// <param name="channel">Defines a potential target for the event.</param>
        /// <param name="level">Specifies the level of detail included in the event.</param>
        /// <param name="opcode">Operation being performed at the time the event is written.</param>
        /// <param name="task">Identifies a logical component of the application that is writing the event.</param>
        /// <param name="keywords">Bit mask that specifies the event category. The keyword can contain one or more provider-defined keywords, standard keywords, or both.</param>
        public EventDescriptor(int id, byte version, byte channel, byte level, byte opcode, int task, long keywords)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Non negative number is required.");
            }
            if (id > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"The ID parameter must be in the range {1} through {ushort.MaxValue}.");
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
            if (task > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(task), $"The ID parameter must be in the range {1} through {ushort.MaxValue}.");
            }

            this.m_task = (ushort)task;
        }
    }
}
