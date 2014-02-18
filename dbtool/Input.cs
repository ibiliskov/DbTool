using System.Collections.Generic;
using System.Linq;

namespace dbtool
{
    public class Input
    {
        public string Verb { get; set; }
        public List<string> Params { get; set; } 

        public Input(string[] args)
        {
            Verb = "help";
            Params = new List<string>();

            if (args.Length > 0)
            {
                Verb = args[0];

                for (int i = 1; i < args.Length; i++)
                {
                    Params.Add(args[i]);
                }
            }
        }

        public int ParamCount
        {
            get
            {
                return Params.Count(param => !string.IsNullOrEmpty(param));
            }
        }

        public string P1
        {
            get
            {
                return Params.Count > 0 ? Params[0] : "";
            }
        }

        public string P2
        {
            get
            {
                return Params.Count > 1 ? Params[1] : "";
            }
        }
    }
}
