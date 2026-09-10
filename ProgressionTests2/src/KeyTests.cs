using Moq;
using Xunit;
using VentureValheim.Progression;

namespace VentureValheim.ProgressionTests;

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
        public bool HasGuardianKeyTest(string guardianPower) => HasGuardianKey(guardianPower);
        public bool PrivateKeyIsBlockedTest(string key) => PrivateKeyIsBlocked(key);
        public bool SummoningTimeReachedTest(string key, int gameDay) => SummoningTimeReached(key, gameDay);
    }

    private const string string1 = "killedTroll";
    private const string string2 = "killedTroll,killedBear,killed_Jesus";
    private const string string3 = " killedTroll , killedBear   , killed_Jesus ";

    private static void SetupConfiguration(int bossSummonsTime)
    {
        Mock<IProgressionConfiguration> mockManager = new Mock<IProgressionConfiguration>();
        mockManager.Setup(x => x.GetUnlockBossSummonsTime()).Returns(bossSummonsTime);

        new ProgressionConfiguration(mockManager.Object);
    }

    private static TestKeyManager Setup(string a, string b, string c, string d)
    {
        Mock<IKeyManager> mockManager = new Mock<IKeyManager>();
        mockManager.Setup(x => x.BlockedGlobalKeys).Returns(a);
        mockManager.Setup(x => x.AllowedGlobalKeys).Returns(b);
        HashSet<string> set1 = ProgressionAPI.StringToSet(a);
        HashSet<string> set2 = ProgressionAPI.StringToSet(b);
        mockManager.Setup(x => x.BlockedGlobalKeysList).Returns(set1);
        mockManager.Setup(x => x.AllowedGlobalKeysList).Returns(set2);

        mockManager.Setup(x => x.BlockedPrivateKeys).Returns(c);
        mockManager.Setup(x => x.AllowedPrivateKeys).Returns(d);
        HashSet<string> set3 = ProgressionAPI.StringToSet(c);
        HashSet<string> set4 = ProgressionAPI.StringToSet(d);
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
        TestKeyManager keyManager = Setup(a, b, "", "");

        Assert.True(keyManager.BlockGlobalKey(true, "random_string"));
        Assert.True(keyManager.BlockGlobalKey(true, "killedtroll"));
        //Assert.False(keyManager.BlockGlobalKey(true, "season_winter")); // Seasonality
    }

    [Theory]
    [InlineData("", string1)]
    [InlineData("", string2)]
    [InlineData("", string3)]
    public void BlockGlobalKey_BlockAllAllowedList(string a, string b)
    {
        TestKeyManager keyManager = Setup(a, b, "", "");

        Assert.True(keyManager.BlockGlobalKey(true, "random_string"));
        Assert.False(keyManager.BlockGlobalKey(true, "killedtroll"));
        //Assert.False(keyManager.BlockGlobalKey(true, "season_winter")); // Seasonality
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("", string1)]
    [InlineData("", string2)]
    [InlineData("", string3)]
    public void BlockGlobalKey_BlockNone(string a, string b)
    {
        TestKeyManager keyManager = Setup(a, b, "", "");

        Assert.False(keyManager.BlockGlobalKey(false, "random_string"));
        Assert.False(keyManager.BlockGlobalKey(false, "killedtroll"));
        //Assert.False(keyManager.BlockGlobalKey(false, "season_winter")); // Seasonality
    }

    [Theory]
    [InlineData(string1, "")]
    [InlineData(string2, "")]
    [InlineData(string3, "")]
    public void BlockGlobalKey_BlockNoneBlockedList(string a, string b)
    {
        TestKeyManager keyManager = Setup(a, b, "", "");

        Assert.False(keyManager.BlockGlobalKey(false, "random_string"));
        Assert.True(keyManager.BlockGlobalKey(false, "killedtroll"));
        //Assert.False(keyManager.BlockGlobalKey(false, "season_winter")); // Seasonality
    }

    [Fact]
    public void BlockGlobalKey_BlockNullOrWhitespace()
    {
        TestKeyManager keyManager = Setup(string3, string3, "", "");

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
        TestKeyManager keyManager = Setup("", "", "", "");
        keyManager.UpdateGlobalKeyConfigurationTest(a, b);

        HashSet<string> set1 = ProgressionAPI.StringToSet(a);
        HashSet<string> set2 = ProgressionAPI.StringToSet(b);

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
        Mock<IKeyManager> mockManager = new Mock<IKeyManager>();
        mockManager.SetupGet(x => x.BlockedGlobalKeys).Returns(a);
        mockManager.SetupGet(x => x.AllowedGlobalKeys).Returns(b);
        HashSet<string> set3 = ProgressionAPI.StringToSet("Test1,Test2,Test3");
        mockManager.SetupGet(x => x.BlockedGlobalKeysList).Returns(set3);
        mockManager.SetupGet(x => x.AllowedGlobalKeysList).Returns(set3);

        TestKeyManager keyManager = new TestKeyManager(mockManager.Object);

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
        Mock<IKeyManager> mockManager = new Mock<IKeyManager>();
        HashSet<string> set = ProgressionAPI.StringToSet(keys);
        mockManager.SetupGet(x => x.PrivateKeysList).Returns(set);

        TestKeyManager keyManager = new TestKeyManager(mockManager.Object);

        Assert.Equal(expected, keyManager.CountPrivateBossKeysTest());
    }

    [Theory]
    [InlineData("GP_TheElder", "", false)]
    [InlineData("GP_TheElder", "defeated_eikthyr", false)]
    [InlineData("GP_TheElder", "defeated_gdking", true)]
    [InlineData("GP_TheElder", "defeated_eikthyr,defeated_gdking", true)]
    [InlineData("GP_Eikthyr,GP_Bonemass", "", false)]
    [InlineData("GP_Eikthyr,GP_Bonemass", "defeated_eikthyr,defeated_gdking", false)]
    [InlineData("GP_Eikthyr,GP_Bonemass", "defeated_eikthyr,defeated_gdking,defeated_bonemass", true)]
    [InlineData("GP_Eikthyr,GP_TheElder,GP_Bonemass", "defeated_eikthyr,defeated_gdking", false)]
    [InlineData("GP_Eikthyr,GP_TheElder,GP_Bonemass", "defeated_eikthyr,defeated_gdking,defeated_bonemass", true)]
    public void HasGuardianKey_All(string guardianPower, string keys, bool expected)
    {
        Mock<IKeyManager> mockManager = new Mock<IKeyManager>();
        HashSet<string> set = ProgressionAPI.StringToSet(keys);
        mockManager.SetupGet(x => x.PrivateKeysList).Returns(set);
        TestKeyManager keyManager = new TestKeyManager(mockManager.Object);

        Mock<IProgressionConfiguration> mockProgressionConfiguration = new Mock<IProgressionConfiguration>();
        mockProgressionConfiguration.Setup(x => x.GetUsePrivateKeys()).Returns(true);
        new ProgressionConfiguration(mockProgressionConfiguration.Object);

        Assert.Equal(expected, keyManager.HasGuardianKeyTest(guardianPower));
    }

    [Theory]
    [InlineData("", string1)]
    [InlineData("", string2)]
    [InlineData("", string3)]
    public void BlockPrivateKey_AllowedList(string a, string b)
    {
        TestKeyManager keyManager = Setup("", "", a, b);

        Assert.True(keyManager.PrivateKeyIsBlockedTest("random_string"));
        Assert.False(keyManager.PrivateKeyIsBlockedTest("killedtroll"));
        Assert.True(keyManager.PrivateKeyIsBlockedTest("season_winter")); // Seasonality
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
        TestKeyManager keyManager = Setup("", "", a, b);

        Assert.False(keyManager.PrivateKeyIsBlockedTest("random_string"));
        Assert.True(keyManager.PrivateKeyIsBlockedTest("killedtroll"));
        Assert.True(keyManager.PrivateKeyIsBlockedTest("season_winter")); // Seasonality
    }

    [Fact]
    public void BlockPrivateKey_BlockNullOrWhitespace()
    {
        TestKeyManager keyManager = Setup("", "", "", "");

        Assert.True(keyManager.PrivateKeyIsBlockedTest(""));
        Assert.True(keyManager.PrivateKeyIsBlockedTest(null));
    }

    [Theory]
    [InlineData(string1, string2)]
    [InlineData(string2, string1)]
    public void UpdatePrivateKeyConfiguration_Update(string a, string b)
    {
        TestKeyManager keyManager = Setup("", "", "", "");
        keyManager.UpdatePrivateKeyConfigurationTest(a, b);

        HashSet<string> set1 = ProgressionAPI.StringToSet(a);
        HashSet<string> set2 = ProgressionAPI.StringToSet(b);

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
        Mock<IKeyManager> mockManager = new Mock<IKeyManager>();
        mockManager.SetupGet(x => x.BlockedPrivateKeys).Returns(a);
        mockManager.SetupGet(x => x.AllowedPrivateKeys).Returns(b);
        HashSet<string> set3 = ProgressionAPI.StringToSet("Test1,Test2,Test3");
        mockManager.SetupGet(x => x.BlockedPrivateKeysList).Returns(set3);
        mockManager.SetupGet(x => x.AllowedPrivateKeysList).Returns(set3);

        TestKeyManager keyManager = new TestKeyManager(mockManager.Object);

        keyManager.UpdatePrivateKeyConfigurationTest(a, b);

        Assert.Equal(a, keyManager.BlockedPrivateKeys);
        Assert.Equal(b, keyManager.AllowedPrivateKeys);
        Assert.Equal(set3, keyManager.BlockedPrivateKeysList);
        Assert.Equal(set3, keyManager.AllowedPrivateKeysList);
    }

    [Theory]
    [InlineData("", 10, true)]
    [InlineData("", 99, true)]
    [InlineData("", 100, true)]
    [InlineData("", 101, true)]
    [InlineData("", 200, true)]
    [InlineData("defeated_eikthyr", 10, false)]
    [InlineData("defeated_eikthyr", 99, false)]
    [InlineData("defeated_eikthyr", 100, true)]
    [InlineData("defeated_eikthyr", 101, true)]
    [InlineData("defeated_eikthyr", 200, true)]
    [InlineData("defeated_dragon", 100, false)]
    [InlineData("defeated_dragon", 399, false)]
    [InlineData("defeated_dragon", 400, true)]
    [InlineData("defeated_dragon", 401, true)]
    [InlineData("defeated_dragon", 500, true)]
    public void SummoningTimeReachedTest(string key, int day, bool expected)
    {
        TestKeyManager keyManager = Setup("", "", "", "");
        SetupConfiguration(100);
        Assert.Equal(expected, keyManager.SummoningTimeReachedTest(key, day));
    }

    [Theory]
    [InlineData("PlayerDamage", true)]
    [InlineData("PlayerEvents", true)]
    [InlineData("Fire", true)]
    [InlineData("DeathDeleteItems", true)]
    [InlineData("AllPiecesUnlocked", true)]
    [InlineData("NoMap", true)]
    [InlineData("NoPortals", true)]
    [InlineData("Preset", true)]
    [InlineData("NonServerOption", false)]
    [InlineData("defeated_eikthyr", false)]
    [InlineData("defeated_gdking", false)]
    [InlineData("KilledTroll", false)]
    [InlineData("Count", false)]
    [InlineData("CustomKey1", false)]
    [InlineData("CustomKey2", false)]
    [InlineData("CustomKey3", false)]
    public void GlobalKeyServerOptionTest(string key, bool expected)
    {
        ZoneSystem.GetKeyValue(key.ToLower(), out _, out GlobalKeys gk);
        Assert.Equal(expected, gk < GlobalKeys.NonServerOption);
    }
}
