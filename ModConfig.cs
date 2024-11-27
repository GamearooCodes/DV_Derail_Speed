using System;
using System.IO;
using Newtonsoft.Json;

namespace Derial_Speed
{
    // Token: 0x02000003 RID: 3
    public class ModConfig
    {
        
        public float Derial_Speed { get; set; } = 120f;

        

      

        public static ModConfig Load(string path)
        {
            bool flag = File.Exists(path);
            ModConfig result;
            if (flag)
            {
                string text = File.ReadAllText(path);
                result = (JsonConvert.DeserializeObject<ModConfig>(text) ?? new ModConfig());
            }
            else
            {
                ModConfig modConfig = new ModConfig();
                modConfig.Save(path);
                result = modConfig;
            }
            return result;
        }

      
        public void Save(string path)
        {
            string contents = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, contents);
        }
    }
}
