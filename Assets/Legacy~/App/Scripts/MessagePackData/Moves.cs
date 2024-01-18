using System.Runtime.Serialization;

namespace ACAM2.MessagePack
{
    public class Move
    {
        public bool isWhite;
        public uint fromFieldId;
        public uint toFieldId;
        public string fromString;
        public string toString;

        public static Move[] ConvertMoveArray(object[] fields)
        {
            byte count = (byte)fields[3];
            if (count == 0)
                return null;

            var array = new Move[count];
            for (int i = 0; i < count; i++)
            {
                int start = 4 + (i * 5);
                array[i] = new Move();
                array[i].isWhite = (byte)fields[start + 0] > 0;
                array[i].fromFieldId = (byte)fields[start + 1];
                array[i].toFieldId = (byte)fields[start + 2];
                array[i].fromString = (string)fields[start + 3];
                array[i].toString = (string)fields[start + 4];
            }
            return array;
        }
    }
}