using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public static string[] initialNames = new string[] { "Scrimblo Blimbo", };

    public static string[] names = new string[] { "Scrimblo", "Blimbo", "Scrimblo Blimbo", "Blimbo Scrimblo", "Blimboff Scriminof", "Scrimblo Blimblam", "Scrimblab Blibblab", "Srimbly Bimbly","Sclorby Dorby","Scrobus the Blorbus","Blorbus Butthead","Wormbo Dormbo","Scrumpus Dumpo","Jumpboots Jamstrang","Gwimbly","Jimbo Blimbo","Blurbee Durbee","Tweedle Beatle","John","Paul","George","Ringo","Jimblo","The Scrunkly","Scrimblim Blimble","Superb Mairo","Glup Shitto","Cash Bannoca","Glover"};
        public string[] quips;
    private readonly string[] dead = { "You died." };

    private void Awake()
    {
        quips = new string[]
        {
            "I'm a quip",
            $"I'm {GetRandomName()}"
        };
    }

    public static string GetRandomName()
    {
        return names[Random.Range(0, names.Length)];
    }

    public string GetRandomQuip()
    {
        return quips[Random.Range(0, quips.Length)];
    }

    public string GetRandomDeathMessage()
    {
        return dead[Random.Range(0, dead.Length)];
    }
}
