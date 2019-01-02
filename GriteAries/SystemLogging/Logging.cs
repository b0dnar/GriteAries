using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace GriteAries.SystemLogging
{
    public class Logging
    {
        public async Task WriteLog(string log)
        {
            string path = @"D:\LogSys";

            using (var wr = new StreamWriter(path + @"\log.txt", true))
            {
                string data = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + " -  ";
                await wr.WriteLineAsync(data + log);
            }
        }
    }
}