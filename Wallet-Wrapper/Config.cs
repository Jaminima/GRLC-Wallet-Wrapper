﻿using Newtonsoft.Json;
using System.IO;

namespace Wallet_Wrapper
{
    public class Conf
    {
        public string rpcAddress = "http://127.0.0.1:42068/", username = "test", password = "test";
    }

    public static class Config
    {
        public static Conf conf = Load();

        public static void Save(Conf conf)
        {
            StreamWriter writer = new StreamWriter(new FileStream("./config.json", FileMode.CreateNew));
            writer.Write(JsonConvert.SerializeObject(conf));
            writer.Flush();
            writer.Close();
        }

        public static Conf Load()
        {
            Conf c = new Conf();

            if (File.Exists("./config.json"))
            {
                StreamReader reader = new StreamReader(new FileStream("./config.json", FileMode.Open));
                string s = reader.ReadToEnd();

                c = JsonConvert.DeserializeObject<Conf>(s);
                reader.Close();
            }
            else
            {
                Save(c);
            }
            return c;
        }
    }
}