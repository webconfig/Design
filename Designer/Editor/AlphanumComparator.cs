using SkillEditor.Runtime.Tasks;
using System;
using System.Collections.Generic;


    /// <summary>
    /// 字母顺序排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AlphanumComparator<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            string text = "";
            if (x.GetType().IsSubclassOf(typeof(Type)))
            {
                Type type = x as Type;
                //text = this.typePrefix(type) + "/";
                TaskCategoryAttribute[] array;
                if ((array = (type.GetCustomAttributes(typeof(TaskCategoryAttribute), false) as TaskCategoryAttribute[])).Length > 0)
                {
                    text = text + array[0].Category + "/";
                }
                text += type.Name.ToString();
            }
            else
            {
                text = x.ToString();
            }
            if (text == null)
            {
                return 0;
            }
            string text2 = "";
            if (y.GetType().IsSubclassOf(typeof(Type)))
            {
                Type type2 = y as Type;
                //text2 = this.typePrefix(type2) + "/";
                TaskCategoryAttribute[] array2;
                if ((array2 = (type2.GetCustomAttributes(typeof(TaskCategoryAttribute), false) as TaskCategoryAttribute[])).Length > 0)
                {
                    text2 = text2 + array2[0].Category + "/";
                }
                text2 += type2.Name.ToString();
            }
            else
            {
                text2 = y.ToString();
            }
            if (text2 == null)
            {
                return 0;
            }
            int length = text.Length;
            int length2 = text2.Length;
            int num = 0;
            int num2 = 0;
            while (num < length && num2 < length2)
            {
                char c = text[num];
                char c2 = text2[num2];
                char[] array3 = new char[length];
                int num3 = 0;
                char[] array4 = new char[length2];
                int num4 = 0;
                do
                {
                    array3[num3++] = c;
                    num++;
                    if (num >= length)
                    {
                        break;
                    }
                    c = text[num];
                }
                while (char.IsDigit(c) == char.IsDigit(array3[0]));
                do
                {
                    array4[num4++] = c2;
                    num2++;
                    if (num2 >= length2)
                    {
                        break;
                    }
                    c2 = text2[num2];
                }
                while (char.IsDigit(c2) == char.IsDigit(array4[0]));
                string text3 = new string(array3);
                string text4 = new string(array4);
                int num6;
                if (char.IsDigit(array3[0]) && char.IsDigit(array4[0]))
                {
                    int num5 = int.Parse(text3);
                    int value = int.Parse(text4);
                    num6 = num5.CompareTo(value);
                }
                else
                {
                    num6 = text3.CompareTo(text4);
                }
                if (num6 != 0)
                {
                    return num6;
                }
            }
            return length - length2;
        }
    }

