using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

namespace Game.Networking
{
    public class NetReplicatorUnitTest
    {
        public class MockNetReplicator : NetReplicator
        {
            public List<ReplicatedObject> Objects { get { return m_Objects; } }
            public List<ReplicationType> Types { get { return m_Types; } }
        }

        public struct ComplexA
        {
            public int InnerFieldA;
            public int InnerFieldB;

            public void NetSerialize(NetStream ns)
            {
                ns.Serialize(ref InnerFieldA);
                ns.Serialize(ref InnerFieldB);
            }
        }

        public struct ComplexB
        {
            public int InnerFieldA;
            public long InnerFieldB;

            public void NetSerialize(NetStream ns)
            {
                ns.Serialize(ref InnerFieldA);
                ns.Serialize(ref InnerFieldB);
            }
        }

        public struct StubArgA : IReplicatedArgument
        {
            public int FieldA;
            public int FieldB;

            public void NetSerialize(NetStream ns)
            {
                ns.Serialize(ref FieldA);
                ns.Serialize(ref FieldB);
            }

            public bool TestCompare(StubArgA other)
            {
                return FieldA == other.FieldA && FieldB == other.FieldB;
            }
        }

        public struct StubArgB : IReplicatedArgument
        {
            public int FieldA;
            public long FieldB;

            public void NetSerialize(NetStream ns)
            {
                ns.Serialize(ref FieldA);
                ns.Serialize(ref FieldB);
            }

            public bool TestCompare(StubArgB other)
            {
                return FieldA == other.FieldA && FieldB == other.FieldB;
            }
        }

        public struct StubArgC : IReplicatedArgument
        {
            public float FieldA;
            public ComplexA FieldB;

            public void NetSerialize(NetStream ns)
            {
                ns.Serialize(ref FieldA);
                FieldB.NetSerialize(ns);
            }

            public bool TestCompare(StubArgC other)
            {
                return FieldA == other.FieldA && FieldB.InnerFieldA == other.FieldB.InnerFieldA && FieldB.InnerFieldB == other.FieldB.InnerFieldB;
            }
        }

        public struct StubArgD : IReplicatedArgument
        {
            public short FieldA;
            public ComplexB FieldB;

            public void NetSerialize(NetStream ns)
            {
                ns.Serialize(ref FieldA);
                FieldB.NetSerialize(ns);
            }

            public bool TestCompare(StubArgD other)
            {
                return FieldA == other.FieldA && FieldB.InnerFieldA == other.FieldB.InnerFieldA && FieldB.InnerFieldB == other.FieldB.InnerFieldB;
            }
        }

        public class StubReplicatedA : IReplicated
        {
            public int NetworkID { get; set; }
            public ReplicationType NetworkType { get; set; }

            public uint MethodACalled = 0;
            public uint MethodBCalled = 0;

            public StubArgA ArgA = new StubArgA();
            public StubArgB ArgB = new StubArgB();

            private static void StaticNetInitialize(ReplicationType type)
            {
                type.RegisterMethod<StubReplicatedA, StubArgA>("MethodA");
                type.RegisterMethod<StubReplicatedA, StubArgB>("MethodB");
            }

            public void MethodA(StubArgA arg)
            {
                arg.TestCompare(ArgA);
                MethodACalled++;
            }

            public void MethodB(StubArgB arg)
            {
                arg.TestCompare(ArgB);
                MethodBCalled++;
            }

        }

        public class StubReplicatedB : IReplicated
        {
            public int NetworkID { get; set; }
            public ReplicationType NetworkType { get; set; }

            public uint MethodACalled = 0;
            public uint MethodBCalled = 0;

            public StubArgC ArgA = new StubArgC();
            public StubArgD ArgB = new StubArgD();

            private static void StaticNetInitialize(ReplicationType type)
            {
                type.RegisterMethod<StubReplicatedB, StubArgC>("MethodA");
                type.RegisterMethod<StubReplicatedB, StubArgD>("MethodB");
            }

            public void MethodA(StubArgC arg)
            {
                arg.TestCompare(ArgA);
                MethodACalled++;
            }

            public void MethodB(StubArgD arg)
            {
                arg.TestCompare(ArgB);
                MethodBCalled++;
            }
        }

        private static byte[] CreatePacketBytes<T>(int objectID, uint methodID, T arg) where T : IReplicatedArgument
        {
            NetStream ns = new NetStream();
            ns.Open();
            arg.NetSerialize(ns);
            byte[] data = ns.Close();

            RemoteMethodPacket packet = new RemoteMethodPacket()
            {
                ProtocolType = Protocol.RemoteMethod,
                Flags = ProtocolFlags.None,
                ObjectID = objectID,
                MethodID = methodID,
                Crc32 = 0,
                Data = data
            };

            return packet.Write();
        }

        /// <summary>
        /// Test that makes sure the replicator can in fact invoke remote methods and all data is correct.
        /// </summary>
        [Test]
        public void ProcessPacketTest()
        {
            MockNetReplicator replicator = new MockNetReplicator();
            replicator.Init();

            Assert.AreEqual(2, replicator.Types.Count);
            Assert.AreEqual(0, replicator.Objects.Count);

            StubArgA argA = new StubArgA() { FieldA = 0xCAD, FieldB = 0xDAB };
            StubArgB argB = new StubArgB() { FieldA = 0xFEED, FieldB = 0xCA55E7 };
            StubArgC argC = new StubArgC() { FieldA = 1.337f, FieldB = new ComplexA() { InnerFieldA = 0xC2C2, InnerFieldB = 0x8008 } };
            StubArgD argD = new StubArgD() { FieldA = 0x7EEF, FieldB = new ComplexB() { InnerFieldA = 0xACDC, InnerFieldB = 0x735D } };

            StubReplicatedA testA = new StubReplicatedA() { ArgA = argA, ArgB = argB };
            replicator.Allocate(testA);
            Assert.AreEqual(1, replicator.Objects.Count);
            Assert.AreEqual(0, testA.NetworkID);

            replicator.ProcessPacketBytes(CreatePacketBytes(testA.NetworkID, 0, argA));
            Assert.AreEqual(1, testA.MethodACalled);
            Assert.AreEqual(0, testA.MethodBCalled);
            replicator.ProcessPacketBytes(CreatePacketBytes(testA.NetworkID, 1, argB));
            Assert.AreEqual(1, testA.MethodACalled);
            Assert.AreEqual(1, testA.MethodBCalled);


            StubReplicatedB testB = new StubReplicatedB() { ArgA = argC, ArgB = argD };
            replicator.Allocate(testB);
            Assert.AreEqual(2, replicator.Objects.Count);
            Assert.AreEqual(1, testB.NetworkID);

            replicator.ProcessPacketBytes(CreatePacketBytes(testB.NetworkID, 0, argC));
            Assert.AreEqual(1, testB.MethodACalled);
            Assert.AreEqual(0, testB.MethodBCalled);
            replicator.ProcessPacketBytes(CreatePacketBytes(testB.NetworkID, 1, argD));
            Assert.AreEqual(1, testB.MethodACalled);
            Assert.AreEqual(1, testB.MethodBCalled);

            replicator.Free(testA);
            Assert.AreEqual(0, testA.NetworkID);
            replicator.Free(testB);
            Assert.AreEqual(0, testB.NetworkID);

            // MockReplicatedA testA = new MockReplicatedA() { }
        }
    }
}
