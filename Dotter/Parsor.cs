using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dotter
{
    class Parsor
    {
        public string ParsePlayerCount(byte[] target)
        {
            string temp = Encoding.UTF8.GetString(target);
            int from, to;

            from = temp.IndexOf("\u0001mp") + 3;
            to = temp.IndexOf(",cp");
            string maxPlayer = temp.Substring(from, to - from);

            from = temp.IndexOf(",cp") + 3;
            to = temp.IndexOf(",qp");
            string minPlayer = temp.Substring(from, to - from);

            return minPlayer + "/" + maxPlayer;
        }

        public string ParsePlayerList(byte[] target, string targetText)
        {
            string temp = BitConverter.ToString(target);
            temp = Regex.Replace(temp, "FF-FF-FF-FF-44-..-00-", string.Empty);
            temp = Regex.Replace(temp, "00-00-00-00-00-..-..-..-..-00", "0A");
            temp = Regex.Replace(temp, "-00-00-00-00-00-..-..-..-..", string.Empty);
            temp = Regex.Replace(temp, "FF-FF-FF-FF-44-", string.Empty);

            string[] tempSplit = temp.Split('-');
            byte[] tempByte = new byte[tempSplit.Length];

            for(int i = 0; i < tempSplit.Length; i++)
            {
                tempByte[i] = Convert.ToByte(tempSplit[i], 16);
            }

            tempSplit = Encoding.UTF8.GetString(tempByte).Split('\n');

            string result = string.Empty;
            foreach (string data in tempSplit)
            {
                if (data.Contains(targetText))
                {
                    result += data + '\n';
                }
            }

            return result;
        }
    }
}
