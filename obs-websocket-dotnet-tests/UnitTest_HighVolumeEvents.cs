using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet.Types.Events;

namespace OBSWebsocketDotNet.Tests
{
    [TestClass]
    public class UnitTest_HighVolumeEvents : OBSWebsocket
    {
        [TestMethod]
        public void ProcessEventType_InputVolumeMeters_RaisesTypedEvent()
        {
            var body = new JObject
            {
                {
                    "eventData", new JObject
                    {
                        {
                            "inputs", new JArray
                            {
                                new JObject
                                {
                                    { "inputName", "Mic/Aux" },
                                    { "inputLevelsMul", new JArray { new JArray { 0.1, 0.2, 0.3 } } }
                                }
                            }
                        }
                    }
                }
            };

            InputVolumeMetersEventArgs received = null;
            InputVolumeMeters += (sender, args) => received = args;

            ProcessEventType(nameof(InputVolumeMeters), body);

            Assert.IsNotNull(received);
            Assert.AreEqual(1, received.inputs.Count);
            Assert.AreEqual("Mic/Aux", received.inputs[0].InputName);
            Assert.AreEqual(0.1f, received.inputs[0].InputLevels[0].MagnitudeWithVolume);
        }
    }
}
