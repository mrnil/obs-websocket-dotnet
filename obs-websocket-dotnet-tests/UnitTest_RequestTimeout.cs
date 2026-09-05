using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace OBSWebsocketDotNet.Tests
{
    [TestClass]
    public class UnitTest_RequestTimeout : OBSWebsocket
    {
        [TestMethod]
        public void WaitForResponse_NoResponseWithinTimeout_ThrowsRequestTimeoutException()
        {
            WSTimeout = TimeSpan.FromMilliseconds(50);
            var tcs = new TaskCompletionSource<JObject>();
            responseHandlers.TryAdd("msg-1", tcs);

            var stopwatch = Stopwatch.StartNew();
            Assert.ThrowsException<RequestTimeoutException>(
                () => WaitForResponse(tcs, "msg-1", "GetVersion"));
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 2000, "Should not block far past the configured timeout");
        }

        [TestMethod]
        public void WaitForResponse_NoResponseWithinTimeout_RemovesAbandonedHandler()
        {
            WSTimeout = TimeSpan.FromMilliseconds(50);
            var tcs = new TaskCompletionSource<JObject>();
            responseHandlers.TryAdd("msg-2", tcs);

            try { WaitForResponse(tcs, "msg-2", "GetVersion"); } catch (RequestTimeoutException) { }

            Assert.IsFalse(responseHandlers.ContainsKey("msg-2"));
        }

        [TestMethod]
        public void WaitForResponse_ResponseArrivesInTime_ReturnsResponseData()
        {
            WSTimeout = TimeSpan.FromSeconds(10);
            var tcs = new TaskCompletionSource<JObject>();
            tcs.SetResult(new JObject
            {
                { "requestStatus", new JObject { { "result", true } } },
                { "responseData", new JObject { { "obsVersion", "31.0.0" } } }
            });

            var result = WaitForResponse(tcs, "msg-3", "GetVersion");

            Assert.AreEqual("31.0.0", (string)result["obsVersion"]);
        }

        [TestMethod]
        public void WaitForResponse_NoResponseDataField_ReturnsEmptyObject()
        {
            var tcs = new TaskCompletionSource<JObject>();
            tcs.SetResult(new JObject
            {
                { "requestStatus", new JObject { { "result", true } } }
            });

            var result = WaitForResponse(tcs, "msg-4", "SetCurrentProgramScene");

            Assert.IsFalse(result.HasValues);
        }

        [TestMethod]
        public void WaitForResponse_ServerReportsFailure_ThrowsErrorResponseExceptionWithCode()
        {
            var tcs = new TaskCompletionSource<JObject>();
            tcs.SetResult(new JObject
            {
                {
                    "requestStatus", new JObject
                    {
                        { "result", false },
                        { "code", 604 },
                        { "comment", "No source was found by the name" }
                    }
                }
            });

            var ex = Assert.ThrowsException<ErrorResponseException>(
                () => WaitForResponse(tcs, "msg-5", "GetInputSettings"));

            Assert.AreEqual(604, ex.ErrorCode);
            StringAssert.Contains(ex.Message, "No source was found by the name");
        }

        [TestMethod]
        public void WaitForResponse_TaskAlreadyCanceled_ThrowsErrorResponseException()
        {
            var tcs = new TaskCompletionSource<JObject>();
            tcs.SetCanceled();

            var ex = Assert.ThrowsException<ErrorResponseException>(
                () => WaitForResponse(tcs, "msg-6", "GetVersion"));

            Assert.AreEqual("Request canceled", ex.Message);
        }

        [TestMethod]
        public void WaitForResponse_TimeoutSetToMaxValue_DoesNotThrowArgumentOutOfRangeException()
        {
            // Task.WaitAny only accepts [0, int.MaxValue] milliseconds or an infinite timeout;
            // TimeSpan.MaxValue is a plausible "no timeout" idiom that must not blow up.
            WSTimeout = TimeSpan.MaxValue;
            var tcs = new TaskCompletionSource<JObject>();
            tcs.SetResult(new JObject
            {
                { "requestStatus", new JObject { { "result", true } } }
            });

            var result = WaitForResponse(tcs, "msg-7", "GetVersion");

            Assert.IsFalse(result.HasValues);
        }

        [TestMethod]
        public void WaitForResponse_TimeoutAtIntMaxMilliseconds_IsPassedThroughUnclamped()
        {
            // Exactly at the boundary Task.WaitAny still accepts: must not be treated as "too large".
            WSTimeout = TimeSpan.FromMilliseconds(int.MaxValue);
            var tcs = new TaskCompletionSource<JObject>();
            tcs.SetResult(new JObject
            {
                { "requestStatus", new JObject { { "result", true } } }
            });

            var result = WaitForResponse(tcs, "msg-8", "GetVersion");

            Assert.IsFalse(result.HasValues);
        }
    }
}
