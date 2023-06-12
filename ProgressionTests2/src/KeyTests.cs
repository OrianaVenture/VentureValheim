using Moq;
using Xunit;
using VentureValheim.Progression;

namespace VentureValheim.ProgressionTests
{
    public class KeyTests
    {
        public class TestKeyManager : KeyManager, IKeyManager
        {
            public TestKeyManager(IKeyManager manager) : base()
            {
                BlockedGlobalKeys = manager.BlockedGlobalKeys;
                AllowedGlobalKeys = manager.AllowedGlobalKeys;
                BlockedGlobalKeysList = manager.BlockedGlobalKeysList;
                AllowedGlobalKeysList = manager.AllowedGlobalKeysList;

                BlockedPrivateKeys = manager.BlockedPrivateKeys;
                AllowedPrivateKeys = manager.AllowedPrivateKeys;
                BlockedPrivateKeysList = manager.BlockedPrivateKeysList;
                AllowedPrivateKeysList = manager.AllowedPrivateKeysList;
                PrivateKeysList = manager.PrivateKeysList;
            }

            public void UpdateGlobalKeyConfigurationTest(string a, string b) => UpdateGlobalKeyConfiguration(a, b);
            public void UpdatePrivateKeyConfigurationTest(string a, string b) => UpdatePrivateKeyConfiguration(a, b);
            public int CountPrivateBossKeysTest() => CountPrivateBossKeys();
            public bool SetFilePathsTest(string path) => SetFilePaths(path);
            public bool PrivateKeyIsBlockedTest(string key) => PrivateKeyIsBlocked(key);
            protected override bool HasGlobalKey(string key)
            {
                return true;
            }
        }

        private const string string1 = "killedTroll";
        private const string string2 = "killedTroll,killedBear,killed_Jesus";
        private const string string3 = " killedTroll , killedBear   , killed_Jesus ";

        private TestKeyManager Setup()
        {
            var mockManager = new Mock<IKeyManager>();

            return new TestKeyManager(mockManager.Object);
        }

        private TestKeyManager Setup(string a, string b, string c, string d)
        {
            var mockManager = new Mock<IKeyManager>();
            mockManager.Setup(x => x.BlockedGlobalKeys).Returns(a);
            mockManager.Setup(x => x.AllowedGlobalKeys).Returns(b);
            var set1 = ProgressionAPI.StringToSet(a);
            var set2 = ProgressionAPI.StringToSet(b);
            mockManager.Setup(x => x.BlockedGlobalKeysList).Returns(set1);
            mockManager.Setup(x => x.AllowedGlobalKeysList).Returns(set2);

            mockManager.Setup(x => x.BlockedPrivateKeys).Returns(c);
            mockManager.Setup(x => x.AllowedPrivateKeys).Returns(d);
            var set3 = ProgressionAPI.StringToSet(c);
            var set4 = ProgressionAPI.StringToSet(d);
            mockManager.Setup(x => x.BlockedPrivateKeysList).Returns(set3);
            mockManager.Setup(x => x.AllowedPrivateKeysList).Returns(set4);

            return new TestKeyManager(mockManager.Object);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(string1, "")]
        [InlineData(string2, "")]
        [InlineData(string3, "")]
        public void BlockGlobalKey_BlockAll(string a, string b)
        {
            var keyManager = Setup(a, b, "", "");

            Assert.True(keyManager.BlockGlobalKey(true, "random_string"));
            Assert.True(keyManager.BlockGlobalKey(true, "killedTroll"));
        }

        [Theory]
        [InlineData("", string1)]
        [InlineData("", string2)]
        [InlineData("", string3)]
        public void BlockGlobalKey_BlockAllAllowedList(string a, string b)
        {
            var keyManager = Setup(a, b, "", "");

            Assert.True(keyManager.BlockGlobalKey(true, "random_string"));
            Assert.False(keyManager.BlockGlobalKey(true, "killedTroll"));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", string1)]
        [InlineData("", string2)]
        [InlineData("", string3)]
        public void BlockGlobalKey_BlockNone(string a, string b)
        {
            var keyManager = Setup(a, b, "", "");

            Assert.False(keyManager.BlockGlobalKey(false, "random_string"));
            Assert.False(keyManager.BlockGlobalKey(false, "killedTroll"));
        }

        [Theory]
        [InlineData(string1, "")]
        [InlineData(string2, "")]
        [InlineData(string3, "")]
        public void BlockGlobalKey_BlockNoneBlockedList(string a, string b)
        {
            var keyManager = Setup(a, b, "", "");

            Assert.False(keyManager.BlockGlobalKey(false, "random_string"));
            Assert.True(keyManager.BlockGlobalKey(false, "killedTroll"));
        }

        [Fact]
        public void BlockGlobalKey_BlockNullOrWhitespace()
        {
            var keyManager = Setup(string3, string3, "", "");

            Assert.True(keyManager.BlockGlobalKey(true, ""));
            Assert.True(keyManager.BlockGlobalKey(true, null));
            Assert.True(keyManager.BlockGlobalKey(false, ""));
            Assert.True(keyManager.BlockGlobalKey(false, null));
        }

        [Theory]
        [InlineData(string1, string2)]
        [InlineData(string2, string1)]
        public void UpdateGlobalKeyConfiguration_Update(string a, string b)
        {
            var keyManager = Setup("", "", "", "");
            keyManager.UpdateGlobalKeyConfigurationTest(a, b);

            var set1 = ProgressionAPI.StringToSet(a);
            var set2 = ProgressionAPI.StringToSet(b);

            Assert.Equal(a, keyManager.BlockedGlobalKeys);
            Assert.Equal(b, keyManager.AllowedGlobalKeys);
            Assert.Equal(set1, keyManager.BlockedGlobalKeysList);
            Assert.Equal(set2, keyManager.AllowedGlobalKeysList);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(string1, string1)]
        [InlineData(string2, string2)]
        public void UpdateGlobalKeyConfiguration_NoUpdate(string a, string b)
        {
            var mockManager = new Mock<IKeyManager>();
            mockManager.SetupGet(x => x.BlockedGlobalKeys).Returns(a);
            mockManager.SetupGet(x => x.AllowedGlobalKeys).Returns(b);
            var set3 = ProgressionAPI.StringToSet("Test1,Test2,Test3");
            mockManager.SetupGet(x => x.BlockedGlobalKeysList).Returns(set3);
            mockManager.SetupGet(x => x.AllowedGlobalKeysList).Returns(set3);

            var keyManager = new TestKeyManager(mockManager.Object);

            keyManager.UpdateGlobalKeyConfigurationTest(a, b);

            Assert.Equal(a, keyManager.BlockedGlobalKeys);
            Assert.Equal(b, keyManager.AllowedGlobalKeys);
            Assert.Equal(set3, keyManager.BlockedGlobalKeysList);
            Assert.Equal(set3, keyManager.AllowedGlobalKeysList);
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData("defeated_eikthyr", 1)]
        [InlineData("defeated_eikthyr,defeated_gdking,defeated_bonemass,defeated_dragon,defeated_goblinking,defeated_queen", 6)]
        [InlineData("defeated_eikthyr,defeated_gdking,defeated_bonemass,defeated_dragon,defeated_goblinking,test1,test2", 5)]
        public void CountPrivateBossKeys_All(string keys, int expected)
        {
            var mockManager = new Mock<IKeyManager>();
            var set = ProgressionAPI.StringToSet(keys);
            mockManager.SetupGet(x => x.PrivateKeysList).Returns(set);

            var keyManager = new TestKeyManager(mockManager.Object);

            Assert.Equal(expected, keyManager.CountPrivateBossKeysTest());
        }

        [Fact]
        public void SetFilePaths_EnsurePathUpdatesOnce()
        {
            var keyManager = Setup();

            Assert.False(keyManager.SetFilePathsTest(null));
            Assert.False(keyManager.SetFilePathsTest(""));
            Assert.False(keyManager.SetFilePathsTest(""));
            Assert.True(keyManager.SetFilePathsTest("APathThatExists"));
            Assert.True(keyManager.SetFilePathsTest(""));
        }

        [Theory]
        [InlineData("", string1)]
        [InlineData("", string2)]
        [InlineData("", string3)]
        public void BlockPrivateKey_AllowedList(string a, string b)
        {
            var keyManager = Setup("", "", a, b);

            Assert.True(keyManager.PrivateKeyIsBlockedTest("random_string"));
            Assert.False(keyManager.PrivateKeyIsBlockedTest("killedTroll"));
        }

        [Theory]
        [InlineData(string1, "")]
        [InlineData(string2, "")]
        [InlineData(string3, "")]
        [InlineData(string3, string1)]
        [InlineData(string3, string2)]
        [InlineData(string3, string3)]
        public void BlockPrivateKey_BlockedList(string a, string b)
        {
            var keyManager = Setup("", "", a, b);

            Assert.False(keyManager.PrivateKeyIsBlockedTest("random_string"));
            Assert.True(keyManager.PrivateKeyIsBlockedTest("killedTroll"));
        }

        [Fact]
        public void BlockPrivateKey_BlockNullOrWhitespace()
        {
            var keyManager = Setup("", "", "", "");

            Assert.True(keyManager.PrivateKeyIsBlockedTest(""));
            Assert.True(keyManager.PrivateKeyIsBlockedTest(null));
        }

        [Theory]
        [InlineData(string1, string2)]
        [InlineData(string2, string1)]
        public void UpdatePrivateKeyConfiguration_Update(string a, string b)
        {
            var keyManager = Setup("", "", "", "");
            keyManager.UpdatePrivateKeyConfigurationTest(a, b);

            var set1 = ProgressionAPI.StringToSet(a);
            var set2 = ProgressionAPI.StringToSet(b);

            Assert.Equal(a, keyManager.BlockedPrivateKeys);
            Assert.Equal(b, keyManager.AllowedPrivateKeys);
            Assert.Equal(set1, keyManager.BlockedPrivateKeysList);
            Assert.Equal(set2, keyManager.AllowedPrivateKeysList);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(string1, string1)]
        [InlineData(string2, string2)]
        public void UpdatePrivateKeyConfiguration_NoUpdate(string a, string b)
        {
            var mockManager = new Mock<IKeyManager>();
            mockManager.SetupGet(x => x.BlockedPrivateKeys).Returns(a);
            mockManager.SetupGet(x => x.AllowedPrivateKeys).Returns(b);
            var set3 = ProgressionAPI.StringToSet("Test1,Test2,Test3");
            mockManager.SetupGet(x => x.BlockedPrivateKeysList).Returns(set3);
            mockManager.SetupGet(x => x.AllowedPrivateKeysList).Returns(set3);

            var keyManager = new TestKeyManager(mockManager.Object);

            keyManager.UpdatePrivateKeyConfigurationTest(a, b);

            Assert.Equal(a, keyManager.BlockedPrivateKeys);
            Assert.Equal(b, keyManager.AllowedPrivateKeys);
            Assert.Equal(set3, keyManager.BlockedPrivateKeysList);
            Assert.Equal(set3, keyManager.AllowedPrivateKeysList);
        }
    }
}