using VentureValheim.Progression;
using Xunit;

namespace VentureValheim.ProgressionTests;

public class APITests
{
    [Fact]
    public void StringToSet_All()
    {
        string string1 = "killedTroll";
        string string2 = "killedTroll,killedBear,killed_Jesus";
        string string3 = " killedTroll , killedBear   , killed_Jesus ";

        HashSet<string> set1 = new HashSet<string>
        {
            "killedtroll"
        };
        HashSet<string> set2 = new HashSet<string>
        {
            "killedtroll",
            "killedbear",
            "killed_jesus"
        };

        Assert.Single(set1);
        Assert.Equal(set1, ProgressionAPI.StringToSet(string1));
        Assert.Equal(set2, ProgressionAPI.StringToSet(string2));
        Assert.Equal(set2, ProgressionAPI.StringToSet(string3));
    }

    [Fact]
    public void StringToDictionary_All()
    {
        string string1 = "Boar";
        string string2 = "Boar,defeated_eikthyr,Wolf,defeated_dragon,Lox,defeated_goblinking";
        string string3 = " Boar, defeated_eikthyr  ,  Wolf, defeated_dragon,   Lox, defeated_goblinking ";

        Dictionary<string, string> dict1 = new Dictionary<string, string>();
        Dictionary<string, string> dict2 = new Dictionary<string, string>
        {
            { "Boar", "defeated_eikthyr" },
            { "Wolf", "defeated_dragon" },
            { "Lox", "defeated_goblinking" }
        };

        Assert.Equal(dict1, ProgressionAPI.StringToDictionary(string1));
        Assert.Equal(dict2, ProgressionAPI.StringToDictionary(string2));
        Assert.Equal(dict2, ProgressionAPI.StringToDictionary(string3));
    }

    [Fact]
    public void MergeLists_All()
    {
        List<string> list1 = new List<string>
        {
            "key1",
            "key1",
            "key1",
            "key2",
            "key3",
            "key4",
            "key5",
            "key6"
        };
        List<string> list2 = new List<string>
        {
            "key1",
            "key2",
            "key7",
            "key8",
            "key9",
            "key10"
        };
        List<string> merged = new List<string>
        {
            "key1",
            "key2",
            "key3",
            "key4",
            "key5",
            "key6",
            "key7",
            "key8",
            "key9",
            "key10"
        };

        List<string> result = ProgressionAPI.MergeLists(list1, list2);
        Assert.Equal(merged, result);
    }
}