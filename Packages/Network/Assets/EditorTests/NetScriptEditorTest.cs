using NUnit.Framework;

namespace Game.Networking
{
    public class NetScriptEditorTest
    {
        [Test]
        public void MyTest()
        {
            NetScript script = new NetScript();
            Assert.AreEqual(0, script.State);
            script.State = 72;
            Assert.AreEqual(72, script.State);
        }
    }
}
