using System;
using System.Collections;
using System.Collections.Generic;

namespace Manos.IO
{
    public static class ManosExtensions
    {
        public static void Write(this Stream stream, byte[] data, Action action)
        {
            stream.Write(data, 0, data.Length, action);
        }

        public static void Write(this Stream stream, byte[] data, int start, Action action)
        {
            stream.Write(data, start, data.Length - start, action);
        }

        public static void Write(this Stream stream, byte[] data, int start, int count, Action action)
        {
            stream.Write(new ByteBuffer(data, start, count), action);
        }

        public static void Write(this Stream stream, ByteBuffer data, Action action)
        {
            CallbackCollection callbacks = new CallbackCollection();
            callbacks.Add(data, action);
            stream.Write(callbacks);
        }
    }

    public class CallbackCollection : IEnumerable<ByteBuffer>
    {
        protected List<Tuple<ByteBuffer, Action>> elements = new List<Tuple<ByteBuffer, Action>>();

        public CallbackCollection()
        {
        }

        public void Add(byte[] data, Action action)
        {
            Add(new ByteBuffer(data, 0, data.Length), action);
        }

        public void Add(ByteBuffer data, Action action)
        {
            elements.Add(Tuple.Create(data, action));
        }

        #region IEnumerable[ByteBuffer] implementation
        public IEnumerator<ByteBuffer> GetEnumerator()
        {
            foreach (var element in elements) {
                element.Item2();
                yield return element.Item1;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator)this.GetEnumerator();
        }
        #endregion
    }
}

