using System.Collections.Generic;

namespace WindowsServiceWorker
{
    public class ServiceContext
    {
        private static ServiceContext _context = new ServiceContext();

        public static ServiceContext Instance()
        {
            return _context;
        }
        private ServiceContext()
        {
            Values = new Dictionary<string, string>();
            Services = new Dictionary<string, Dictionary<string, string>>();
        }

        private Dictionary<string, string> Values;

        public string Get(string name)
        {
            if (!Values.ContainsKey(name))
                return string.Empty;

            return Values[name];
        }

        public void Set(string name, string value)
        {
            Values[name] = value;
        }

        public Dictionary<string, Dictionary<string, string>> Services;

        public string GetService(string name, string key)
        {
            if (!Services.ContainsKey(name))
                return string.Empty;
            if (!Services[name].ContainsKey(key))
                return string.Empty;

            return Services[name][key];
        }
        public void SetService(string name, string key, string value)
        {
            if (!Services.ContainsKey(name))
                Services[name] = new Dictionary<string, string>();
            Services[name][key] = value;
        }
    }
}