using System;
using System.IO;
using UnityEngine;

namespace Extensions
{
    public static class FileStreamExtensions
    {
        // Метод для записи int в FileStream
        public static void WriteInt(this FileStream fileStream, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            fileStream.Write(buffer, 0, buffer.Length);
        }

        // Метод для чтения int из FileStream
        public static int ReadInt(this FileStream fileStream)
        {
            byte[] buffer = new byte[sizeof(int)];
            fileStream.Read(buffer, 0, buffer.Length);
            return BitConverter.ToInt32(buffer, 0);
        }

        // Метод для записи float в FileStream
        public static void WriteFloat(this FileStream fileStream, float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            fileStream.Write(buffer, 0, buffer.Length);
        }

        // Метод для чтения float из FileStream
        public static float ReadFloat(this FileStream fileStream)
        {
            byte[] buffer = new byte[sizeof(float)];
            fileStream.Read(buffer, 0, buffer.Length);
            return BitConverter.ToSingle(buffer, 0);
        }

        // Метод для записи Vector3 в FileStream
        public static void WriteVector3(this FileStream fileStream, Vector3 value)
        {
            fileStream.WriteFloat(value.x);
            fileStream.WriteFloat(value.y);
            fileStream.WriteFloat(value.z);
        }

        // Метод для чтения Vector3 из FileStream
        public static Vector3 ReadVector3(this FileStream fileStream)
        {
            float x = fileStream.ReadFloat();
            float y = fileStream.ReadFloat();
            float z = fileStream.ReadFloat();
            return new Vector3(x, y, z);
        }

        // Метод для записи Vector3Int в FileStream
        public static void WriteVector3Int(this FileStream fileStream, Vector3Int value)
        {
            fileStream.WriteInt(value.x);
            fileStream.WriteInt(value.y);
            fileStream.WriteInt(value.z);
        }

        // Метод для чтения Vector3Int из FileStream
        public static Vector3Int ReadVector3Int(this FileStream fileStream)
        {
            int x = fileStream.ReadInt();
            int y = fileStream.ReadInt();
            int z = fileStream.ReadInt();
            return new Vector3Int(x, y, z);
        }
    }
}