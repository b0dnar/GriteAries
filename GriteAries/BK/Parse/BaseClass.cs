using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GriteAries.Models;

namespace GriteAries.BK.Parse
{
    public abstract class BaseClass
    {
        public List<Data> dataFootballs { get; set; }
        public List<Data> dataHockeys { get; set; }

        public BaseClass()
        {
            dataFootballs = new List<Data>();
            dataHockeys = new List<Data>();
        }

        #region Methods from Fora
        public List<Fora> GetFora(string str, Regex reg1, Regex reg2)
        {
            List<Fora> list = new List<Fora>();

            var collFora1 = reg1.Matches(str);
            var collFora2 = reg2.Matches(str);

            for (int i = 0; i < collFora1.Count; i++)
            {
                Fora fora = new Fora();

                string nameFora = collFora1[i].Groups["val1"].Value;

                if (nameFora.Contains(","))
                {
                    string[] name = collFora1[i].Groups["val1"].Value.Split(',');

                    if (name[0].Contains("-") || name[1].Contains("-"))
                        fora.Name = Convert.ToString((ConvertToFloat(name[0]) + ConvertToFloat(name[1])) / 2);
                    else
                        fora.Name = "+" + Convert.ToString((ConvertToFloat(name[0]) + ConvertToFloat(name[1])) / 2);
                }
                else
                {
                    fora.Name = nameFora;
                }

                fora.Team1 = ConvertToValueBK(collFora1[i].Groups["val2"].Value);
                fora.Team2 = GetValueFora(collFora2, collFora1[i].Groups["val1"].Value);

                list.Add(fora);
            }

            return list;
        }

        public List<Handicap> GetHandicap(string str, Regex reg1, Regex reg2, Regex reg3)
        {
            List<Handicap> list = new List<Handicap>();

            var collHand1 = reg1.Matches(str);
            var collHand2 = reg2.Matches(str);
            var collHandD = reg3.Matches(str);

            for (int i = 0; i < collHand1.Count; i++)
            {
                Handicap handicap = new Handicap();
                handicap.Name = collHand1[i].Groups["val1"].Value;
                handicap.Team1 = ConvertToValueBK(collHand1[i].Groups["val2"].Value);
                handicap.Team2 = GetValueFora(collHand2, handicap.Name);
                handicap.Draw = ConvertToValueBK(collHandD.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(handicap.Name)).Groups["val2"].Value);

                list.Add(handicap);
            }

            return list;
        }

        public ValueBK GetValueFora(MatchCollection match, string name1)
        {
            ValueBK valueBK;

            valueBK.BK = TypeBK.Marathone;

            if (match.Count == 0)
                valueBK.Value = 0;
            else
            {
                string temp;
                if (name1.Contains("-"))
                    temp = name1.Replace("-", "+");
                else
                    temp = name1.Replace("+", "-");

                valueBK.Value = ConvertToFloat(match.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(temp)).Groups["val2"].Value);
            }

            return valueBK;
        }

        #endregion

        #region Methods from Total
        public List<Total> GetTotal(string str, Regex reg1, Regex reg2)
        {
            List<Total> list = new List<Total>();
            var collTotB = reg1.Matches(str);
            var collTotM = reg2.Matches(str);

            for (int i = 0; i < collTotB.Count; i++)
            {
                Total total = new Total();
                string temp = collTotB[i].Groups["val1"].Value;

                if (temp.Contains(","))
                {
                    string[] name = temp.Split(',');
                    total.Name = Convert.ToString((ConvertToFloat(name[0]) + ConvertToFloat(name[1])) / 2);
                }
                else
                    total.Name = temp;

                total.Over = ConvertToValueBK(collTotB[i].Groups["val2"].Value);
                total.Under = ConvertToValueBK(collTotM.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(temp)).Groups["val2"].Value);

                list.Add(total);
            }

            return list;
        }

        public List<Total3Event> GetTot3Event(string str, Regex reg1, Regex reg2, Regex reg3)
        {
            List<Total3Event> list = new List<Total3Event>();
            var collTotB = reg1.Matches(str);
            var collTotM = reg2.Matches(str);
            var collTotE = reg3.Matches(str);

            for (int i = 0; i < collTotB.Count; i++)
            {
                Total3Event total = new Total3Event();

                total.Name = collTotB[i].Groups["val1"].Value;

                total.Over = ConvertToValueBK(collTotB[i].Groups["val2"].Value);
                total.Under = ConvertToValueBK(collTotM.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(total.Name)).Groups["val2"].Value);
                total.Exactly = ConvertToValueBK(collTotE.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(total.Name)).Groups["val2"].Value);

                list.Add(total);
            }
            return list;
        }
        #endregion

        public ValueBK ConvertToValueBK(string str)
        {
            ValueBK valueBK;

            valueBK.BK = TypeBK.Marathone;

            if (!str.Equals(""))
                valueBK.Value = ConvertToFloat(str);
            else
                valueBK.Value = 0;

            return valueBK;
        }

        public float ConvertToFloat(string str)
        {
            if (str.Equals(""))
                return 0;

            return ConvertToFloat(str);
        }

    }
}