using Moq;
using NUnit.Framework;
using VentureValheim.Progression;

namespace VentureValheim.ProgressionTests
{
    [TestFixture]
    public class Tests
    {
        [TestCase(true, "", "")]
        [TestCase(true, "killedTroll", "")]
        [TestCase(true, "killedTroll,killedBear,killed_Jesus", "")]
        [TestCase(true, "killedTroll , killedBear   , killed_Jesus ", "")]
        public void BlockGlobalKey_BlockAll(bool a, string b, string c)
        {
            ProgressionManager.Instance.Initialize(a, b, c);
            Assert.True(ProgressionManager.Instance.BlockGlobalKey("killedTroll"));
            Assert.True(ProgressionManager.Instance.BlockGlobalKey("random_string"));
            Assert.True(ProgressionManager.Instance.BlockGlobalKey(""));
            Assert.True(ProgressionManager.Instance.BlockGlobalKey(null));
        }
        
        [TestCase(true, "", "killedTroll")]
        [TestCase(true, "", "killedTroll,killedBear,killed_Jesus")]
        [TestCase(true, "", "killedTroll , killedBear   , killed_Jesus ")]
        public void BlockGlobalKey_BlockAllAllowedList(bool a, string b, string c)
        {
            ProgressionManager.Instance.Initialize(a, b, c);
            Assert.False(ProgressionManager.Instance.BlockGlobalKey("killedTroll"));
            Assert.True(ProgressionManager.Instance.BlockGlobalKey("random_string"));
            Assert.True(ProgressionManager.Instance.BlockGlobalKey(""));
            Assert.True(ProgressionManager.Instance.BlockGlobalKey(null));
        }
        
        [TestCase(false, "", "")]
        [TestCase(false, "", "killedTroll")]
        [TestCase(false, "", "killedTroll,killedBear,killed_Jesus")]
        [TestCase(false, "", "killedTroll , killedBear   , killed_Jesus ")]
        public void BlockGlobalKey_BlockNone(bool a, string b, string c)
        {
            ProgressionManager.Instance.Initialize(a, b, c);
            Assert.False(ProgressionManager.Instance.BlockGlobalKey("killedTroll"));
            Assert.False(ProgressionManager.Instance.BlockGlobalKey("random_string"));
            Assert.False(ProgressionManager.Instance.BlockGlobalKey(""));
            Assert.False(ProgressionManager.Instance.BlockGlobalKey(null));
        }
        
        [TestCase(false, "killedTroll", "")]
        [TestCase(false, "killedTroll,killedBear,killed_Jesus", "")]
        [TestCase(false, "killedTroll , killedBear   , killed_Jesus ", "")]
        public void BlockGlobalKey_BlockNoneBlockedList(bool a, string b, string c)
        {
            ProgressionManager.Instance.Initialize(a, b, c);
            Assert.True(ProgressionManager.Instance.BlockGlobalKey("killedTroll"));
            Assert.False(ProgressionManager.Instance.BlockGlobalKey("random_string"));
            Assert.False(ProgressionManager.Instance.BlockGlobalKey(""));
            Assert.False(ProgressionManager.Instance.BlockGlobalKey(null));
        }
    }
}