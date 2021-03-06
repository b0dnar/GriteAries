﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GriteAries.Models;

namespace GriteAries.BK
{
    public abstract class Bukmeker
    {
        public const int MaxMinuteMatchFootball = 85;
        public string GetOppositeForaName(string name)
        {
            if (name.Contains("-"))
            {
                return name.Replace("-", "+");
            }
            else if (name.Contains("+"))
            {
                return name.Replace("+", "-");
            }
            else
            {
                return name;
            }
        }

        #region Methods Convert
        public ValueBK ConvertToValueBK(TypeBK bk, string value)
        {
            ValueBK valueBK = new ValueBK(bk);

            if (!value.Equals(""))
            {
                valueBK.Value = ConvertToFloat(value);
            }
            else
            {
                valueBK.Value = 0;
            } 

            return valueBK;
        }

        public float ConvertToFloat(string str)
        {
            if (str.Equals(""))
            {
                return 0;
            }

            return Convert.ToSingle(str);
        }
        #endregion
    }
}