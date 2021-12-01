using System.Net;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Summer_Pokemon_Game_C.Pkmn {
    public class EvolutionLine {

        public Dictionary<int, string> Stages { get; private set; } = new();
        //public Dictionary<string, Dictionary<string, string>> EvolutionRequirements = new();
        readonly string url;

        public EvolutionLine(int EvolutionLineID) {
            url = "https://pokeapi.co/api/v2/evolution-chain/" + EvolutionLineID + "/";
            using WebClient wc = new(); string json = wc.DownloadString(url);
            string[] species = json.Split("\"species\":{\"name\":")[1..];
            int currentStage = 1;
            foreach (string s in species.Reverse()) {
                int number = int.Parse(s.Split("https://pokeapi.co/api/v2/pokemon-species/")[1].Split("/")[0]);
                string name = s.Split("\"")[1];
                currentStage++;
                Stages.Add(number, name);
            }
        }
    }
}
