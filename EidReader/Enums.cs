namespace Be.Mcq8.EidReader
{
    public enum ReadOption
    {
        IdOnly,
        IdAndAddress,
        All,
    }

    internal enum ReaderScope
    {
        User,
        Terminal,
        System
    }

    internal enum ReaderShare
    {
        Exclusive = 1,
        Shared,
        Direct
    }

    internal enum ReadProtocol
    {
        Undefined = 0x00000000,
        T0 = 0x00000001,
        T1 = 0x00000002,
        Raw = 0x00010000,
        Default = unchecked((int)0x80000000),
        T0orT1 = T0 | T1
    }

    internal enum CardState
    {
        UNAWARE = 0x00000000,
        IGNORE = 0x00000001,
        CHANGED = 0x00000002,
        UNKNOWN = 0x00000004,
        UNAVAILABLE = 0x00000008,
        EMPTY = 0x00000010,
        PRESENT = 0x00000020,
        ATRMATCH = 0x00000040,
        EXCLUSIVE = 0x00000080,
        INUSE = 0x00000100,
        MUTE = 0x00000200,
        UNPOWERED = 0x00000400
    }

    internal enum FileID : long
    {
        Id = 0x3F00DF014031,
        IdSign = 0x3F00DF014032,
        Address = 0x3F00DF014033,
        AddressSign = 0x3F00DF014034,
        Photo = 0x3F00DF014035,
        RnCert = 0x3F00DF00503c
    }

    internal enum Scard : int
    {
        SCARD_S_SUCCESS = 0,
        SCARD_E_SERVICE_STOPPED = -2146435042,
        SCARD_E_NO_SERVICE = -2146435043,
        SCARD_E_TIMEOUT = -2146435062,
        SCARD_W_REMOVED_CARD = -2146434967,
    }
}
