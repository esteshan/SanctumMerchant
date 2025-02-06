using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ExileCore2.Shared.Nodes;

namespace SanctumRewards
{
    public class SanctumEffectsLoader
    {
        private string _loadError;
        public Dictionary<string, List<string>> Priorities { get; private set; } = new();

        public void LoadSanctumEffects(ListNode effectsFile)
        {
            try
            {
                string filePath = effectsFile.Value;
                if (!File.Exists(filePath))
                {
                    _loadError = "File not found";
                    Priorities.Clear();
                    return;
                }

                string json = File.ReadAllText(filePath);
                Priorities = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json) ?? new();
                _loadError = null;
            }
            catch (Exception ex)
            {
                _loadError = ex.Message;
                Priorities.Clear();
            }
        }

        public string GetLoadStatus()
        {
            return Priorities.Count > 0 ? "Loaded successfully" : $"Failed to load: {_loadError}";
        }

        public int GetTotalEntries()
        {
            return Priorities.Count > 0 ? Priorities.Values.Sum(list => list.Count) : 0;
        }
    }
}