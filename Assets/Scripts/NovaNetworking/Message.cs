using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NovaNetworking {
    public class Message {
        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;


        public Message() {
            buffer = new List<byte>();
            readPos = 0;
        }

        public Message(int id) {
            buffer = new List<byte>();
            readPos = 0;

            Write(id);
        }

        public Message(byte[] data) {
            buffer = new List<byte>();
            readPos = 0;

            SetBytes(data);
        }


        #region Functions
        public void SetBytes(byte[] data) {
            Write(data);
            readableBuffer = buffer.ToArray();
        }

        public void WriteLength() {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
        }

        public void InsertInt(int value) {
            buffer.InsertRange(0, BitConverter.GetBytes(value));
        }

        public byte[] ToArray() {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        public int Length() {
            return buffer.Count;
        }

        public int UnreadLength() {
            return Length() - readPos;
        }

        public void Reset(bool shouldReset = true) {
            if (shouldReset) {
                buffer.Clear();
                readableBuffer = null;
                readPos = 0;
            }
            else {
                readPos -= 4;
            }
        }
        #endregion


        #region Write Data
        public void Write(byte value) {
            buffer.Add(value);
        }

        public void Write(byte[] value) {
            buffer.AddRange(value);
        }

        public void Write(short value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(int value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(long value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(float value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(bool value) {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(string value) {
            Write(value.Length);
            buffer.AddRange(Encoding.ASCII.GetBytes(value));
        }
        #endregion


        #region Read Data
        public byte ReadByte(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                byte value = readableBuffer[readPos];

                if (moveReadPos) readPos += 1;
                return value;
            }
            else throw new Exception("Could not read value of type 'byte'!");
        }

        public byte[] ReadBytes(int length, bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                byte[] value = buffer.GetRange(readPos, length).ToArray();
                
                if (moveReadPos) readPos += length;
                return value;
            }
            else throw new Exception("Could not read value of type 'byte[]'!");
        }

        public short ReadShort(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                short value = BitConverter.ToInt16(readableBuffer, readPos);
                
                if (moveReadPos) readPos += 2;
                return value;
            }
            else throw new Exception("Could not read value of type 'short'!");
        }

        public int ReadInt(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                int value = BitConverter.ToInt32(readableBuffer, readPos);

                if (moveReadPos) readPos += 4;
                return value;
            }
            else throw new Exception("Could not read value of type 'int'!");
        }

        public long ReadLong(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                long value = BitConverter.ToInt64(readableBuffer, readPos);

                if (moveReadPos) readPos += 8;
                return value;
            }
            else throw new Exception("Could not read value of type 'long'!");
        }

        public float ReadFloat(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                float value = BitConverter.ToSingle(readableBuffer, readPos);

                if (moveReadPos) readPos += 4;
                return value;
            }
            else throw new Exception("Could not read value of type 'float'!");
        }

        public bool ReadBool(bool moveReadPos = true) {
            if (buffer.Count > readPos) {
                bool value = BitConverter.ToBoolean(readableBuffer, readPos);

                if (moveReadPos) readPos += 1;
                return value;
            }
            else throw new Exception("Could not read value of type 'bool'!");
        }

        public string ReadString(bool moveReadPos = true) {
            try {
                int length = ReadInt();
                string value = Encoding.ASCII.GetString(readableBuffer, readPos, length);

                if (moveReadPos && value.Length > 0) readPos += length;
                return value;
            }
            catch {
                throw new Exception("Could not read value of type 'string'!");
            }
        }
        #endregion
    }
}
