using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Json.Serialization
{
    /// <summary>
    /// JSON formatter class for .NET Micro Framework
    /// </summary>
    /// <remarks>
    /// Can be used to serialize/deserialize with JSON.
    /// </remarks>
    public class JsonFormatter
    {
        //private Hashtable types;
        ArrayList _objectHash;

        /// <summary>
        /// Constructs a JSON formatter
        /// </summary>
        public JsonFormatter()
        {
            _objectHash = new ArrayList();
        }

        /// <summary>
        /// Serializes an object to a string
        /// </summary>
        /// <param name="o">Object to be serialized</param>
        /// <returns>JSON string</returns>
        /// <remarks>
        /// Object definition must have the [Serializable] attribute.
        /// Serializes only members which are not marked with [NonSerialized] attribute.
        /// </remarks>
        public string ToJson(object o)
        {
            string JsonString = "{";
            //WriteString(stm, "{");

            Type ot = o.GetType();
            FieldInfo[] fis = ot.GetFields();
            

            //int n = fis.Length;
            bool hasStarted = false;
            foreach (FieldInfo fi in fis)
            {
                object val = fi.GetValue(o);

                if (val == null) continue;
                Type ft = fi.FieldType;
                //if (!ft.IsSerializable) continue;

                if (hasStarted) JsonString += ", ";
                hasStarted = true;
                JsonString += "\"" + fi.Name + "\": ";
                
                if (ft == typeof(Guid) || ft == typeof(string))
                {
                    JsonString += "\"" + val.ToString() + "\"";
                }
                else 
                {
                    if (ft == typeof(Hashtable))
                    {
                        JsonString += "{ ";
                        Hashtable ht = (Hashtable)val;
                        IEnumerator ie = ht.GetEnumerator();
                        bool isNext = false;
                        while (ie.MoveNext())
                        {
                            DictionaryEntry de = (DictionaryEntry) ie.Current;
                            if (isNext)
                            {
                                JsonString += ", ";
                            }
                            else
                            {
                                isNext = true;
                            }
                            JsonString += "\"" + de.Key + "\": \"" + de.Value + "\"";
                        }

                        JsonString += " }";
                    }
                    else
                    {
                        if (ft.IsEnum)
                        {
                            string ss = val.ToString();
                            
                            JsonString += "\"" + ss + "\"";
                        }
                        else
                        {
                            if (ft.IsArray)
                            {
                                JsonString += "[";
                                bool IsNext = false;

                                Array arr = (Array)val;
                                foreach (object ov in arr)
                                {
                                    if (IsNext)
                                    {
                                        JsonString += ", ";
                                    }
                                    else
                                    {
                                        IsNext = true;
                                    }
                                    JsonString += ToJson(ov);
                                }
                                JsonString += "]";
                            }
                            else
                            {
                                if (ft == typeof(int) || ft == typeof(float) || ft == typeof(double))
                                {
                                    JsonString += val.ToString();
                                }
                                else
                                {
                                    if (_objectHash.Contains(val.GetHashCode()))
                                    {
                                        throw new InvalidOperationException("Cyclic loop in object structure!");
                                    }
                                    _objectHash.Add(val.GetHashCode());
                                    JsonString += ToJson(val);
                                }
                            }
                        }
                    }
                }
            }
            JsonString += "}";
            return JsonString;
        }

        /// <summary>
        /// Splits a sprecified string in parts according to JSON array definition
        /// </summary>
        /// <param name="s">string to be parsed</param>
        /// <returns>Array of item strings</returns>
        private ArrayList SplitArrayStrings(string s)
        {
            ArrayList sl = new ArrayList();
            int x = s.IndexOf('{');
            int nb = 1;
            int sp = x;
            while (sp >= 0 && x >=0)
            {
                //nb++;
                int nc = s.IndexOf('}', x + 1);
                if (nc < 0) break;
                int no = s.IndexOf('{', x + 1);
                if (nc < no || no < 0)
                {
                    nb--;
                    x = nc;
                }
                else
                {
                    nb++;
                    x = no;
                }
                string s1 = no < 0 ? string.Empty : s.Substring(no);
                string s2 = nc < 0 ? string.Empty : s.Substring(nc);
                string sx = x < 0 ? string.Empty : s.Substring(x);

                if (nb == 0)
                {
                    sl.Add(s.Substring(sp, nc - sp + 1));
                    sp = no;
                }
            }

            return sl;
        }

        /// <summary>
        /// Returns nets parameter string
        /// </summary>
        /// <param name="s">String to be searched</param>
        /// <param name="pos">Start position</param>
        /// <returns>next paramerter string</returns>
        private string GetNextQuotedString(string s, ref int pos)
        {
            string rv = string.Empty;
            int ns = s.IndexOf('\"', pos);
            if (ns != -1)
            {
                int nb = s.IndexOf('}', pos);
                if (nb == -1 || nb > ns)
                {
                    int ne = s.IndexOf('\"', ns + 1);
                    if (ne != -1)
                    {
                        rv = s.Substring(ns + 1, ne - ns - 1);
                        pos = ne + 1; // after ":
                    }
                }
            }
            return rv;
        }

        /// <summary>
        /// JSON null string
        /// </summary>
        private const string NullString = "null";

        /// <summary>
        /// Extracts a value from a given string
        /// </summary>
        /// <param name="s">string to be parsed</param>
        /// <param name="pos">start position</param>
        /// <returns>string value</returns>
        private string GetValueString(string s, ref int pos)
        {
            int nq = s.IndexOf('\"', pos);
            int nn = s.IndexOf(NullString, pos);
            if (nn > 0 && nn < nq) return null;
            int nsp = s.IndexOf(':', pos);
            if (nsp != -1)
            {
                //pos = nsp + 2;
                pos = nsp + 1;
            }
            
            int nt = s.IndexOfAny(new char[] { '}', ',' }, pos);
            if (nt < nq || (nq == -1 && nt != -1))
            {
                string rv = s.Substring(pos, nt - pos);
                pos = nt + 1;
                return rv;
            }
            
            return GetNextQuotedString(s, ref pos);
        }

        /// <summary>
        /// Returns next sub-object string
        /// </summary>
        /// <param name="s">string to be searched</param>
        /// <param name="pos">start position</param>
        /// <returns>next object string</returns>
        private string GetSubObject(string s, ref int pos)
        {
            int nb = 0;
            int ns = s.IndexOf('{', pos);
            int ne = ns;
            do
            {
                ne = s.IndexOfAny(new char[] { '{', '}' }, ne);
                if (ne == -1) return string.Empty;
                if (s[ne] == '{') nb++;
                if (s[ne] == '}') nb--;
            } while (nb > 0);

            pos = ne;
            return s.Substring(ns, ne - ns);
        }

        /// <summary>
        /// Deserializes an object from JSON
        /// </summary>
        /// <param name="bts">JSON input bytes</param>
        /// <param name="objType">type of an object to deserialize</param>
        /// <returns>Desetrialized object; can be null in case if the string does not contain any objects</returns>
        /// <remarks>
        /// This function can be used to parse JSON objecta and arrays.
        /// </remarks>
        public object FromJson(byte[] bts, Type objType)
        {
            string JsonString = new string(Encoding.UTF8.GetChars(bts));
            if (JsonString[0] == '[')
            {
                ArrayList lst = new ArrayList();
                ArrayList parts = SplitArrayStrings(JsonString); //JsonString.Split('[', ']');
                foreach (object o in parts)
                {
                    string s = (string)o;
                    if (s != string.Empty) lst.Add(ParseObject(s, objType));
                }
                return lst;
            }
            else
                return ParseObject(JsonString, objType);
        }

        /// <summary>
        /// Parses object from JSON
        /// </summary>
        /// <param name="JsonString">JSon string</param>
        /// <param name="objType">object type</param>
        /// <returns>deserialized object</returns>
        /// <remarks>
        /// Works recursively.
        /// </remarks>
        private object ParseObject(string JsonString, Type objType)
        {
            object rv = objType.GetConstructor(new Type[] {}).Invoke(new object[] {});

            int pos = 0;
            string fn = GetNextQuotedString(JsonString, ref pos);
            while (fn != string.Empty)
            {
                FieldInfo fi = objType.GetField(fn);
                if (fi.FieldType == typeof(string))
                {
                    string val = GetValueString(JsonString, ref pos); //GetNextQuotedString(JsonString, ref pos);
                    fi.SetValue(rv, val);
                }
                else
                {
                    if (fi.FieldType == typeof(int))
                    {
                        string val = GetValueString(JsonString, ref pos);
                        int i = (val == null) ? 0 : int.Parse(val);
                        fi.SetValue(rv, i);
                    }
                    else
                    {
                        if (fi.FieldType == typeof(float))
                        {
                            string val = GetValueString(JsonString, ref pos);
                            float f = (val == null) ? 0f : val.ToFloat();
                            fi.SetValue(rv, f);
                        }
                        else
                        {
                            if (fi.FieldType == typeof(double))
                            {
                                string val = GetNextQuotedString(JsonString, ref pos);
                                double d = (val == null) ? 0.0 : double.Parse(val);
                                fi.SetValue(rv, d);
                            }
                            else
                            {
                                if (fi.FieldType == typeof(Guid))
                                {
                                    string val = GetNextQuotedString(JsonString, ref pos);
                                    Guid uid = val.ToGuid();
                                    
                                    fi.SetValue(rv, uid);
                                }
                                else
                                {
                                    if (fi.FieldType == typeof(DateTime))
                                    {
                                        string val = GetValueString(JsonString, ref pos);
                                        if (val != null)
                                        {
                                            DateTime dt = val.ToDateTime();
                                            fi.SetValue(rv, dt);
                                        }
                                    }
                                    else
                                    {
                                        if (fi.FieldType == typeof(Hashtable))
                                        {
                                            Hashtable ht = new Hashtable();
                                            string key = GetNextQuotedString(JsonString, ref pos);
                                            while (key != string.Empty)
                                            {
                                                string val = GetNextQuotedString(JsonString, ref pos);
                                                ht.Add(key, val);
                                                key = GetNextQuotedString(JsonString, ref pos);
                                            }
                                            fi.SetValue(rv, ht);
                                            pos++;
                                        }
                                        else
                                        {
                                            if (fi.FieldType.IsArray)
                                            {
                                                throw new NotImplementedException("Array deserialization is not implemented yet.");
                                                // TODO: deserialize array
                                            }
                                            else // structures
                                            {
                                                string val = GetSubObject(JsonString, ref pos);
                                                object ov = ParseObject(val, fi.FieldType);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                fn = GetNextQuotedString(JsonString, ref pos);
            }

            return rv;
        }
    }
}
