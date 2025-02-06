using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SanctumMerchant.Helper
{
    public class JsonLoader
    {
        public List<SanctumPriority> SanctumEffects { get; private set; } = new();

        private readonly string _filePath;

        public JsonLoader(string filePath)
        {
            _filePath = filePath;
        }

        public void LoadJson()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string jsonContent = File.ReadAllText(_filePath);
                    var jsonData = JsonConvert.DeserializeObject<SanctumData>(jsonContent);

                    if (jsonData?.SanctumEffects != null)
                    {
                        SanctumEffects = jsonData.SanctumEffects;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SanctumMerchant] Error loading JSON: {ex.Message}");
            }
        }
    }

    public class SanctumData
    {
        public List<SanctumPriority> SanctumEffects { get; set; }
    }

    public class SanctumPriority
    {
        public string MenuName { get; set; }
        public List<SanctumEffect> Effects { get; set; }
    }

    public class SanctumEffect
    {
        public string EffectName { get; set; }
    }
}