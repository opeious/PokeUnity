using SPICA.Formats.Common;
using SPICA.Serialization;
using SPICA.Serialization.Attributes;

namespace SPICA.Formats.CtrH3D
{
    public class H3DStringUtf16 : ICustomSerialization
    {
        [Ignore] private string Str;

        public H3DStringUtf16() { }

        public H3DStringUtf16(string Str)
        {
            this.Str = Str;
        }

        public override string ToString()
        {
            return Str ?? string.Empty;
        }

        void ICustomSerialization.Deserialize(BinaryDeserializer Deserializer)
        {
            Str = Deserializer.Reader.ReadString();
        }

        bool ICustomSerialization.Serialize(BinarySerializer Serializer)
        {
            Serializer.Writer.Write(Str);

            return true;
        }
    }
}
