using System;
using System.Collections.Generic;
using System.Diagnostics;

using Demo.Models;

using FASTER.core;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Demo.Controllers
{
      public class MyLongKeySerializer : BinaryObjectSerializer<long>
      {
            public override void Serialize(ref long key) => writer.Write(key);
            public override void Deserialize(ref long key) => key = reader.ReadInt32();
      }
      public class MyLongValueSerializer : BinaryObjectSerializer<long>
      {
            public override void Serialize(ref long value) => writer.Write(value);
            public override void Deserialize(ref long value) => value = reader.ReadInt32();
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
      public class HomeController : Controller
      {
            private readonly ILogger<HomeController> _logger;

            public HomeController(ILogger<HomeController> logger)
            {
                  _logger = logger;
            }

            public IActionResult Index()
            {
                  IDevice log = Devices.CreateLogDevice("F:\\Temp\\SinjulMSBH.log", true, false, -1, false);
                  IDevice objlog = Devices.CreateLogDevice("F:\\Temp\\SinjulMSBH.obj.log", true, false, -1, false);

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

                  for (int i = 0; i <= 20000; i++)
                  {
                        key = i; value = i;
                        fht.Upsert(ref key, ref value, Empty.Default, 0);

                        if (i % 1024 == 0) fht.Refresh();
                  }

                  Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

                  for (int i = 0; i <= 20000; i++)
                  {
                        key = i; value = i;
                        Status readStatus = fht.Read(ref key, ref input, ref output, Empty.Default, 0);
                        if (readStatus == Status.OK && output == key)
                              keyValuePairs.Add("", "")
                              Console.WriteLine($"key: {key} and output: {output}");
                        else
                              keyValuePairs.Add($"key: {key}", "Status.NOTFOUND..!!!!");
                  }

                  fht.StopSession();

                  fht.Dispose();
                  log.Close();
                  objlog.Close();


                  return View();
            }

            public IActionResult Privacy()
            {
                  return View();
            }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                  return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
      }
}
