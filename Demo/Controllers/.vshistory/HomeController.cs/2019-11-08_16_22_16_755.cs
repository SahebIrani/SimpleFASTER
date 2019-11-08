using System;
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
