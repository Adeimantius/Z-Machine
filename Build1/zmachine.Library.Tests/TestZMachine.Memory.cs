namespace zmachine.Library.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using zmachine.Library.Models;

    [TestClass]
    public class TestZMachineMemory
    {
        /// <summary>
        /// Memory test. TODO: move to memory
        /// </summary>
        [TestMethod]
        public void Test_setWord()
        {
            Memory testMemory = new Memory(Machine.MemorySizeByVersion[3], contents: null);
            uint address = 1234;
            ushort value = 2345;
            testMemory.setWord(address: address, value: value);

            ushort memoryCheck = testMemory.getWord(address: address);
            Assert.AreEqual(expected: value, actual: memoryCheck);
        }
    }
}
