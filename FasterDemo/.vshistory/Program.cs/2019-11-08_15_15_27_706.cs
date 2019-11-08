using System;

using FASTER.core;

namespace FasterDemo
{
      public class MyKey : IFasterEqualityComparer<MyKey>
      {
            public int key;

            public long GetHashCode64(ref MyKey key)
            {
                  return Utility.GetHashCode(key.key);
            }

            public bool Equals(ref MyKey key1, ref MyKey key2)
            {
                  return key1.key == key2.key;
            }
      }
      public class MyKeySerializer : BinaryObjectSerializer<MyKey>
      {
            public override void Serialize(ref MyKey key)
            {
                  writer.Write(key.key);
            }

            public override void Deserialize(ref MyKey key)
            {
                  key.key = reader.ReadInt32();
            }
      }
      public class MyValue
      {
            public int value;
      }
      public class MyValueSerializer : BinaryObjectSerializer<MyValue>
      {
            public override void Serialize(ref MyValue value)
            {
                  writer.Write(value.value);
            }

            public override void Deserialize(ref MyValue value)
            {
                  value.value = reader.ReadInt32();
            }
      }
      public class MyInput
      {
            public int value;
      }
      public class MyOutput
      {
            public MyValue value;
      }
      public class MyContext { }
      public class MyFunctions : IFunctions<MyKey, MyValue, MyInput, MyOutput, MyContext>
      {
            public void InitialUpdater(ref MyKey key, ref MyInput input, ref MyValue value) => value.value = input.value;
            public void CopyUpdater(ref MyKey key, ref MyInput input, ref MyValue oldValue, ref MyValue newValue) => newValue = oldValue;
            public bool InPlaceUpdater(ref MyKey key, ref MyInput input, ref MyValue value) { value.value += input.value; return true; }


            public void SingleReader(ref MyKey key, ref MyInput input, ref MyValue value, ref MyOutput dst) => dst.value = value;
            public void SingleWriter(ref MyKey key, ref MyValue src, ref MyValue dst) => dst = src;
            public void ConcurrentReader(ref MyKey key, ref MyInput input, ref MyValue value, ref MyOutput dst) => dst.value = value;
            public bool ConcurrentWriter(ref MyKey key, ref MyValue src, ref MyValue dst) { dst = src; return true; }

            public void ReadCompletionCallback(ref MyKey key, ref MyInput input, ref MyOutput output, MyContext ctx, Status status) { }
            public void UpsertCompletionCallback(ref MyKey key, ref MyValue value, MyContext ctx) { }
            public void RMWCompletionCallback(ref MyKey key, ref MyInput input, MyContext ctx, Status status) { }
            public void DeleteCompletionCallback(ref MyKey key, MyContext ctx) { }
            public void CheckpointCompletionCallback(Guid sessionId, long serialNum) { }
      }
















      class Program
      {
            static void Main(string[] args)
            {
                  Console.WriteLine("Hello World!");
            }
      }
}
