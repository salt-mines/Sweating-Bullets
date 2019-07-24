using Lidgren.Network;
using UnityEngine;

namespace Networking
{
    public static class BufferUtils
    {
        public static void Write(this NetBuffer buf, Vector2 vec)
        {
            buf.Write(vec.x);
            buf.Write(vec.y);
        }

        public static void Write(this NetBuffer buf, Vector3 vec)
        {
            buf.Write(vec.x);
            buf.Write(vec.y);
            buf.Write(vec.z);
        }

        public static void Write(this NetBuffer buf, Quaternion quaternion)
        {
            buf.Write(quaternion.x);
            buf.Write(quaternion.y);
            buf.Write(quaternion.z);
            buf.Write(quaternion.w);
        }

        public static void Write(this NetBuffer buf, Color color, bool alpha)
        {
            buf.Write(color.r);
            buf.Write(color.g);
            buf.Write(color.b);

            if (alpha)
                buf.Write(color.a);
        }

        public static Vector2 ReadVector2(this NetBuffer buf)
        {
            return new Vector2(buf.ReadFloat(), buf.ReadFloat());
        }

        public static Vector3 ReadVector3(this NetBuffer buf)
        {
            return new Vector3(buf.ReadFloat(), buf.ReadFloat(), buf.ReadFloat());
        }

        public static Quaternion ReadQuaternion(this NetBuffer buf)
        {
            return new Quaternion(
                buf.ReadFloat(),
                buf.ReadFloat(),
                buf.ReadFloat(),
                buf.ReadFloat());
        }

        public static Color ReadColor(this NetBuffer buf, bool alpha)
        {
            return new Color(
                buf.ReadFloat(),
                buf.ReadFloat(),
                buf.ReadFloat(),
                alpha ? buf.ReadFloat() : 1);
        }
    }
}