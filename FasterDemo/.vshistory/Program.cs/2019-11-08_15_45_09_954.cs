using System;
using System.Diagnostics;

using FASTER.core;

namespace FasterDemo
{
      public class MyKey : IFasterEqualityComparer<MyKey>
      {
            public long GetHashCode64(ref MyKey key) => Utility.GetHashCode(key.key);
            public int key;

            public bool Equals(ref MyKey key1, ref MyKey key2) => key1.key == key2.key;
      }
      public class MyKeySerializer : BinaryObjectSerializer<MyKey>
      {
            public override void Serialize(ref MyKey key) => writer.Write(key.key);
            public override void Deserialize(ref MyKey key) => key.key = reader.ReadInt32();
      }
      public class MyLongKeySerializer : BinaryObjectSerializer<long>
      {
            public override void Serialize(ref long key) => writer.Write(key);
            public override void Deserialize(ref long key) => key = reader.ReadInt32();
      }
      public class MyValue
      {
            public int value;
      }
      public class MyValueSerializer : BinaryObjectSerializer<MyValue>
      {
            public override void Serialize(ref MyValue value) => writer.Write(value.value);
            public override void Deserialize(ref MyValue value) => value.value = reader.ReadInt32();
      }
      public class MyLongValueSerializer : BinaryObjectSerializer<long>
      {
            public override void Serialize(ref long value) => writer.Write(value);
            public override void Deserialize(ref long value) => value = reader.ReadInt32();
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
      public class Funcs : IFunctions<long, long, long, long, Empty>
      {
            public void SingleReader(ref long key, ref long input, ref long value, ref long dst) => dst = value;
            public void SingleWriter(ref long key, ref long src, ref long dst) => dst = src;
            public void ConcurrentReader(ref long key, ref long input, ref long value, ref long dst) => dst = value;
            public void ConcurrentWriter(ref long key, ref long src, ref long dst) => dst = src;
            public void InitialUpdater(ref long key, ref long input, ref long value) => value = input;
            public void CopyUpdater(ref long key, ref long input, ref long oldv, ref long newv) => newv = oldv + input;
            public void InPlaceUpdater(ref long key, ref long input, ref long value) => value += input;
            public void UpsertCompletionCallback(ref long key, ref long value, Empty ctx) { }
            public void ReadCompletionCallback(ref long key, ref long input, ref long output, Empty ctx, Status s) { }
            public void RMWCompletionCallback(ref long key, ref long input, Empty ctx, Status s) { }
            public void CheckpointCompletionCallback(Guid sessionId, long serialNum) { }

            public void DeleteCompletionCallback(ref long key, Empty ctx) { }
            bool IFunctions<long, long, long, long, Empty>.InPlaceUpdater(ref long key, ref long input, ref long value) { return default; }
            bool IFunctions<long, long, long, long, Empty>.ConcurrentWriter(ref long key, ref long src, ref long dst) { return default; }
      }

      class Program
      {
            static void Main(string[] args)
            {
                  IDevice log = Devices.CreateLogDevice("F:\\Temp\\SinjulMSBH.log");
                  IDevice objlog = Devices.CreateLogDevice("F:\\Temp\\SinjulMSBH.obj.log");

                  var fht = new FasterKV<long, long, long, long, Empty, Funcs>
                    (1L << 20,
                                new Funcs(),
                                new LogSettings
                                {
                                      LogDevice = log,
                                      ObjectLogDevice = objlog,
                                      MemorySizeBits = 29
                                },
                                null,
                                new SerializerSettings<long, long>()
                                {
                                      keySerializer = () => new MyLongKeySerializer(),
                                      valueSerializer = () => new MyLongValueSerializer()
                                }
                    );

                  fht.StartSession();

                  long key = 1, value = 1, input = 10, output = 0;

                  Status upSert = fht.Upsert(ref key, ref value, Empty.Default, 0);

                  Status readStatus = fht.Read(ref key, ref input, ref output, Empty.Default, 0);
                  if (readStatus == Status.OK && output == key)
                        Console.WriteLine("Success!");
                  else
                        Console.WriteLine("Error!");

                  fht.Delete(ref key, Empty.Default, 0);
                  readStatus = fht.Read(ref key, ref input, ref output, Empty.Default, 0);
                  if (readStatus == Status.NOTFOUND)
                        Console.WriteLine("Success!");
                  else
                        Console.WriteLine("Error!");

                  Debug.Assert(output == value);
                  Console.WriteLine(output == value);

                  fht.RMW(ref key, ref input, Empty.Default, 0);
                  fht.RMW(ref key, ref input, Empty.Default, 0);

                  readStatus = fht.Read(ref key, ref input, ref output, Empty.Default, 0);
                  if (readStatus == Status.NOTFOUND)
                        Console.WriteLine("Success!");
                  else
                        Console.WriteLine("Error!");

                  Debug.Assert(output == value + 20);
                  Console.WriteLine(output == value + 20);

                  fht.StopSession();

                  fht.Dispose();
                  log.Close();
                  objlog.Close();

                  Console.ReadKey();
            }
      }
}
