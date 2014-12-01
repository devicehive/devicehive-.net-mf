using System;
using System.Collections;
using System.Globalization;
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

                if (ft == typeof(string))
                {
                    string s = (string)val;
                    JsonString += "\"" + Escape(s) + "\"";
                }
                else
                {
                    if (ft == typeof(Guid))
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
                                DictionaryEntry de = (DictionaryEntry)ie.Current;
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
        /// Escapes the JSON string according to JSON spec
        /// </summary>
        /// <param name="s">String to escape</param>
        /// <returns>Escaped string</returns>
        private string Escape(string s)
        {
            string rv = s;
            
            for (int x = 0; x < _escapeSymbols.Length; x++)
            {
                rv = rv.Replace(_escapeSymbols[x], _escapeReplacement[x]);
            }

            return rv;
        }

        /// <summary>
        /// Unescapes JSON string according to JSON spec
        /// </summary>
        /// <param name="s">JSON string to unescape</param>
        /// <returns>Unescaped string</returns>
        private string Unescape(string s)
        {
            string rv = s;

            for (int x = 0; x < _escapeSymbols.Length; x++)
            {
                rv = rv.Replace(_escapeReplacement[x], _escapeSymbols[x]);
            }
            
            for (int nUni = rv.IndexOf("\\u"); nUni != -1; nUni = rv.IndexOf("\\u"))
            {
                if (nUni + 6 > rv.Length)
                {
                    throw new InvalidOperationException("Invalid unicode char in JSON string!");
                }
                string sCode = rv.Substring(nUni + 2, 4);
                char ch = UnicodeToUtf(sCode);
                rv = rv.Substring(0, nUni) + ch + rv.Substring(nUni + 6);
            }

            return rv;
        }

        /// <summary>
        /// Parse HEX string into integer.
        /// </summary>
        /// <param name="s">4-digit hexadecimal number to parse</param>
        /// <returns>Integer code</returns>
        /// <remarks>
        /// The input string is expected to carry numbers only, without "0x" prefix.
        /// Correct: 00A9
        /// Incorrect: 0x00A9
        /// Incorrect: 00A9h
        /// </remarks>
        private int ParseHex(string s)
        {
            int rv = 0;
            string ps = s.Trim().ToLower();
            if (ps.Length == 0 || ps.Length > 4)
            {
                throw new InvalidOperationException("Invalid code - expected 4-dight HEX number!");
            }
            int pos = 1;
            for (int x = ps.Length - 1; x >= 0; x--)
            {
                byte b = (byte)ps[x];
                if (b >= '0' && b <= '9')
                {
                    b -= (byte)'0';
                }
                else
                {
                    if (b >= 'a' && b <= 'f')
                    {
                        b = (byte)(b - (byte)'a' + (byte)0x0A);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected symbol in source string! Only hexadecimal numbers are expected!");
                    }

                }

                rv += pos * b;
                pos *= 0x10;
            }
            return rv;
        }

        /// <summary>
        /// Convert a Unicode character code to UTF-8 char
        /// </summary>
        /// <param name="code">4-dight hexadecimal code</param>
        /// <returns>UTF-8 character</returns>
        public char UnicodeToUtf(string code)
        {
            int nc = ParseHex(code);

            //byte b1 = (byte)(nc >> 8);
            //byte b2 = (byte)(nc & 0xFF);

            byte[] utf = new byte[] {};
            if (nc <= 0x7F)
            {
                utf = new byte[] { (byte)((byte)nc & 0x7F) };
            }
            else
            {   
                int nbytes = 0;
                int tmp = nc;
                while (tmp != 0)
                {
                    tmp = tmp >> 6;
                    nbytes++;
                }
                utf = new byte[nbytes];
                tmp = nc;

                byte header = 0;
                for (int x = nbytes - 1; x >= 0; x--)
                {
                    byte bt = (byte)(tmp & 0x3F);
                    tmp = tmp >> 6;
                    utf[x] = (byte)(bt | 0x80);
                    header = (byte)((header >> 1) | 0x80);
                }
                utf[0] |= header;
            }

            char [] rvs = Encoding.UTF8.GetChars(utf);
            if (rvs.Length == 1)
            {
                return rvs[0];
            }
            else
            {
                throw new InvalidOperationException("Invalid character code!");
            }
        }
        
        /// <summary>
        /// Returns next parameter string
        /// </summary>
        /// <param name="s">String to be searched</param>
        /// <param name="pos">Start position</param>
        /// <returns>next paramerter string</returns>
        private string GetNextQuotedString(string s, ref int pos)
        {
            string rv = string.Empty;
            int ns = GetEscapedIndex(s, '\"', pos); //s.IndexOf('\"', pos);
            if (ns != -1)
            {
                //int nb = GetEscapedIndex(s, '}', pos); //s.IndexOf('}', pos);
                //if (nb == -1 || nb > ns)
                //{
                int ne = GetEscapedIndex(s, '\"', ns + 1); //s.IndexOf('\"', ns + 1);
                if (ne != -1)
                {
                    rv = s.Substring(ns + 1, ne - ns - 1);
                    pos = ne + 1; // after ":
                }
                //}
            }
            return Unescape(rv);
        }

        /// <summary>
        /// Returns the index of the next character with regards of the escaping
        /// </summary>
        /// <param name="s">Source string</param>
        /// <param name="ch">Char to find</param>
        /// <param name="pos">Start position for the search</param>
        /// <returns>Zero-based index of the symbol or -1 if not found</returns>
        private int GetEscapedIndex(string s, char ch, int pos)
        {
            for (int x = pos; x < s.Length; ++x)
            {
                if (s[x] == ch)
                {
                    if (x > 0 && s[x - 1] != '\\') return x; 
                }
            }
            return -1;
        }

        /// <summary>
        /// JSON null string
        /// </summary>
        private const string NullString = "null";

        /// <summary>
        /// Symbols to be escaped in JSON
        /// </summary>
        private string[] _escapeSymbols = { "\\", "\"", "/", "\b", "\f", "\n", "\r", "\t" };

        /// <summary>
        /// Escape strings for forbidden symbols
        /// </summary>
        private string[] _escapeReplacement = { "\\\\", "\\\"", "\\/", "\\b", "\\f", "\\n", "\\r", "\\t" };

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
