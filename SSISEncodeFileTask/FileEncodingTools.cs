using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.SqlServer.Dts.Runtime;

namespace SSISEncodeFileTask100
{
    class FileEncodingTools
    {
        #region Private fields
        private string _filePath;
        private int _destinationEncoding;
        private static object lockObject = new object();
        #endregion

        #region Encoding List
        public static List<string[]> EncodingList = new List<string[]>
                                                        {
                                                            new [] { "37","IBM037","IBM EBCDIC (US-Canada)"},
                                                            new [] { "437","IBM437","OEM United States"},
                                                            new [] { "500","IBM500","IBM EBCDIC (International)"},
                                                            new [] { "708","ASMO-708","Arabic (ASMO 708)"},
                                                            new [] { "720","DOS-720","Arabic (DOS)"},
                                                            new [] { "737","ibm737","Greek (DOS)"},
                                                            new [] { "775","ibm775","Baltic (DOS)"},
                                                            new [] { "850","ibm850","Western European (DOS)"},
                                                            new [] { "852","ibm852","Central European (DOS)"},
                                                            new [] { "855","IBM855","OEM Cyrillic"},
                                                            new [] { "857","ibm857","Turkish (DOS)"},
                                                            new [] { "858","IBM00858","OEM Multilingual Latin I"},
                                                            new [] { "860","IBM860","Portuguese (DOS)"},
                                                            new [] { "861","ibm861","Icelandic (DOS)"},
                                                            new [] { "862","DOS-862","Hebrew (DOS)"},
                                                            new [] { "863","IBM863","French Canadian (DOS)"},
                                                            new [] { "864","IBM864","Arabic (864)"},
                                                            new [] { "865","IBM865","Nordic (DOS)"},
                                                            new [] { "866","cp866","Cyrillic (DOS)"},
                                                            new [] { "869","ibm869","Greek, Modern (DOS)"},
                                                            new [] { "870","IBM870","IBM EBCDIC (Multilingual Latin-2)"},
                                                            new [] { "874","windows-874","Thai (Windows)"},
                                                            new [] { "875","cp875","IBM EBCDIC (Greek Modern)"},
                                                            new [] { "932","shift_jis","Japanese (Shift-JIS)"},
                                                            new [] { "936","gb2312","Chinese Simplified (GB2312)"},
                                                            new [] { "949","ks_c_5601-1987","Korean"},
                                                            new [] { "950","big5","Chinese Traditional (Big5)"},
                                                            new [] { "1026","IBM1026","IBM EBCDIC (Turkish Latin-5)"},
                                                            new [] { "1047","IBM01047","IBM Latin-1"},
                                                            new [] { "1140","IBM01140","IBM EBCDIC (US-Canada-Euro)"},
                                                            new [] { "1141","IBM01141","IBM EBCDIC (Germany-Euro)"},
                                                            new [] { "1142","IBM01142","IBM EBCDIC (Denmark-Norway-Euro)"},
                                                            new [] { "1143","IBM01143","IBM EBCDIC (Finland-Sweden-Euro)"},
                                                            new [] { "1144","IBM01144","IBM EBCDIC (Italy-Euro)"},
                                                            new [] { "1145","IBM01145","IBM EBCDIC (Spain-Euro)"},
                                                            new [] { "1146","IBM01146","IBM EBCDIC (UK-Euro)"},
                                                            new [] { "1147","IBM01147","IBM EBCDIC (France-Euro)"},
                                                            new [] { "1148","IBM01148","IBM EBCDIC (International-Euro)"},
                                                            new [] { "1149","IBM01149","IBM EBCDIC (Icelandic-Euro)"},
                                                            new [] { "1200","utf-16","Unicode"},
                                                            new [] { "1201","unicodeFFFE","Unicode (Big-Endian)"},
                                                            new [] { "1250","windows-1250","Central European (Windows)"},
                                                            new [] { "1251","windows-1251","Cyrillic (Windows)"},
                                                            new [] { "1252","Windows-1252","Western European (Windows)"},
                                                            new [] { "1253","windows-1253","Greek (Windows)"},
                                                            new [] { "1254","windows-1254","Turkish (Windows)"},
                                                            new [] { "1255","windows-1255","Hebrew (Windows)"},
                                                            new [] { "1256","windows-1256","Arabic (Windows)"},
                                                            new [] { "1257","windows-1257","Baltic (Windows)"},
                                                            new [] { "1258","windows-1258","Vietnamese (Windows)"},
                                                            new [] { "1361","Johab","Korean (Johab)"},
                                                            new [] { "10000","macintosh","Western European (Mac)"},
                                                            new [] { "10001","x-mac-japanese","Japanese (Mac)"},
                                                            new [] { "10002","x-mac-chinesetrad","Chinese Traditional (Mac)"},
                                                            new [] { "10003","x-mac-korean","Korean (Mac)"},
                                                            new [] { "10004","x-mac-arabic","Arabic (Mac)"},
                                                            new [] { "10005","x-mac-hebrew","Hebrew (Mac)"},
                                                            new [] { "10006","x-mac-greek","Greek (Mac)"},
                                                            new [] { "10007","x-mac-cyrillic","Cyrillic (Mac)"},
                                                            new [] { "10008","x-mac-chinesesimp","Chinese Simplified (Mac)"},
                                                            new [] { "10010","x-mac-romanian","Romanian (Mac)"},
                                                            new [] { "10017","x-mac-ukrainian","Ukrainian (Mac)"},
                                                            new [] { "10021","x-mac-thai","Thai (Mac)"},
                                                            new [] { "10029","x-mac-ce","Central European (Mac)"},
                                                            new [] { "10079","x-mac-icelandic","Icelandic (Mac)"},
                                                            new [] { "10081","x-mac-turkish","Turkish (Mac)"},
                                                            new [] { "10082","x-mac-croatian","Croatian (Mac)"},
                                                            new [] { "12000","utf-32","Unicode (UTF-32)"},
                                                            new [] { "12001","utf-32BE","Unicode (UTF-32 Big-Endian)"},
                                                            new [] { "20000","x-Chinese-CNS","Chinese Traditional (CNS)"},
                                                            new [] { "20001","x-cp20001","TCA Taiwan"},
                                                            new [] { "20002","x-Chinese-Eten","Chinese Traditional (Eten)"},
                                                            new [] { "20003","x-cp20003","IBM5550 Taiwan"},
                                                            new [] { "20004","x-cp20004","TeleText Taiwan"},
                                                            new [] { "20005","x-cp20005","Wang Taiwan"},
                                                            new [] { "20105","x-IA5","Western European (IA5)"},
                                                            new [] { "20106","x-IA5-German","German (IA5)"},
                                                            new [] { "20107","x-IA5-Swedish","Swedish (IA5)"},
                                                            new [] { "20108","x-IA5-Norwegian","Norwegian (IA5)"},
                                                            new [] { "20127","us-ascii","US-ASCII"},
                                                            new [] { "20261","x-cp20261","T.61"},
                                                            new [] { "20269","x-cp20269","ISO-6937"},
                                                            new [] { "20273","IBM273","IBM EBCDIC (Germany)"},
                                                            new [] { "20277","IBM277","IBM EBCDIC (Denmark-Norway)"},
                                                            new [] { "20278","IBM278","IBM EBCDIC (Finland-Sweden)"},
                                                            new [] { "20280","IBM280","IBM EBCDIC (Italy)"},
                                                            new [] { "20284","IBM284","IBM EBCDIC (Spain)"},
                                                            new [] { "20285","IBM285","IBM EBCDIC (UK)"},
                                                            new [] { "20290","IBM290","IBM EBCDIC (Japanese katakana)"},
                                                            new [] { "20297","IBM297","IBM EBCDIC (France)"},
                                                            new [] { "20420","IBM420","IBM EBCDIC (Arabic)"},
                                                            new [] { "20423","IBM423","IBM EBCDIC (Greek)"},
                                                            new [] { "20424","IBM424","IBM EBCDIC (Hebrew)"},
                                                            new [] { "20833","x-EBCDIC-KoreanExtended","IBM EBCDIC (Korean Extended)"},
                                                            new [] { "20838","IBM-Thai","IBM EBCDIC (Thai)"},
                                                            new [] { "20866","koi8-r","Cyrillic (KOI8-R)"},
                                                            new [] { "20871","IBM871","IBM EBCDIC (Icelandic)"},
                                                            new [] { "20880","IBM880","IBM EBCDIC (Cyrillic Russian)"},
                                                            new [] { "20905","IBM905","IBM EBCDIC (Turkish)"},
                                                            new [] { "20924","IBM00924","IBM Latin-1"},
                                                            new [] { "20932","EUC-JP","Japanese (JIS 0208-1990 and 0212-1990)"},
                                                            new [] { "20936","x-cp20936","Chinese Simplified (GB2312-80)"},
                                                            new [] { "20949","x-cp20949","Korean Wansung"},
                                                            new [] { "21025","cp1025","IBM EBCDIC (Cyrillic Serbian-Bulgarian)"},
                                                            new [] { "21866","koi8-u","Cyrillic (KOI8-U)"},
                                                            new [] { "28591","iso-8859-1","Western European (ISO)"},
                                                            new [] { "28592","iso-8859-2","Central European (ISO)"},
                                                            new [] { "28593","iso-8859-3","Latin 3 (ISO)"},
                                                            new [] { "28594","iso-8859-4","Baltic (ISO)"},
                                                            new [] { "28595","iso-8859-5","Cyrillic (ISO)"},
                                                            new [] { "28596","iso-8859-6","Arabic (ISO)"},
                                                            new [] { "28597","iso-8859-7","Greek (ISO)"},
                                                            new [] { "28598","iso-8859-8","Hebrew (ISO-Visual)"},
                                                            new [] { "28599","iso-8859-9","Turkish (ISO)"},
                                                            new [] { "28603","iso-8859-13","Estonian (ISO)"},
                                                            new [] { "28605","iso-8859-15","Latin 9 (ISO)"},
                                                            new [] { "29001","x-Europa","Europa"},
                                                            new [] { "38598","iso-8859-8-i","Hebrew (ISO-Logical)"},
                                                            new [] { "50220","iso-2022-jp","Japanese (JIS)"},
                                                            new [] { "50221","csISO2022JP","Japanese (JIS-Allow 1 byte Kana)"},
                                                            new [] { "50222","iso-2022-jp","Japanese (JIS-Allow 1 byte Kana - SO/SI)"},
                                                            new [] { "50225","iso-2022-kr","Korean (ISO)"},
                                                            new [] { "50227","x-cp50227","Chinese Simplified (ISO-2022)"},
                                                            new [] { "51932","euc-jp","Japanese (EUC)"},
                                                            new [] { "51936","EUC-CN","Chinese Simplified (EUC)"},
                                                            new [] { "51949","euc-kr","Korean (EUC)"},
                                                            new [] { "52936","hz-gb-2312","Chinese Simplified (HZ)"},
                                                            new [] { "54936","GB18030","Chinese Simplified (GB18030)"},
                                                            new [] { "57002","x-iscii-de","ISCII Devanagari"},
                                                            new [] { "57003","x-iscii-be","ISCII Bengali"},
                                                            new [] { "57004","x-iscii-ta","ISCII Tamil"},
                                                            new [] { "57005","x-iscii-te","ISCII Telugu"},
                                                            new [] { "57006","x-iscii-as","ISCII Assamese"},
                                                            new [] { "57007","x-iscii-or","ISCII Oriya"},
                                                            new [] { "57008","x-iscii-ka","ISCII Kannada"},
                                                            new [] { "57009","x-iscii-ma","ISCII Malayalam"},
                                                            new [] { "57010","x-iscii-gu","ISCII Gujarati"},
                                                            new [] { "57011","x-iscii-pa","ISCII Punjabi"},
                                                            new [] { "65000","utf-7","Unicode (UTF-7)"},
                                                            new [] { "65001","utf-8","Unicode (UTF-8)"}
    };
        #endregion

        #region .ctor
        public FileEncodingTools(string filePath, int destinationEncoding)
        {
            _filePath = filePath;
            _destinationEncoding = destinationEncoding;
        }
        #endregion

        #region Methods
        public bool Encode(IDTSComponentEvents componentEvents)
        {
            bool retVal = false;
            bool refire = false;
            try
            {
                lock (lockObject)
                {
                    Encoding sourceEncoding;
                    string sourceText;
                    using (StreamReader reader = new StreamReader(_filePath, true))
                    {
                        sourceEncoding = reader.CurrentEncoding;
                        sourceText = reader.ReadToEnd();
                        reader.Close();
                    }

                    componentEvents.FireInformation(0,
                                "SSISEncodeFileTask",
                                string.Format("File to encode {0} from current encoding {1} - {2}", _filePath, sourceEncoding.CodePage, sourceEncoding.EncodingName),
                                string.Empty,
                                0,
                                ref refire);

                    //byte[] bytes;
                    //using (FileStream openRead = File.OpenRead(_filePath))
                    //{
                    //    bytes = new byte[openRead.Length];
                    //    openRead.Read(bytes, 0, (int)openRead.Length);
                    //    openRead.Close();
                    //}

                    Encoding destinationEncoding = Encoding.GetEncoding(_destinationEncoding);

                    componentEvents.FireInformation(0,
                                    "SSISEncodeFileTask",
                                    string.Format("Destination encoding {0} - {1}", destinationEncoding.CodePage, destinationEncoding.EncodingName),
                                    string.Empty,
                                    0,
                                    ref refire);

                    File.Delete(_filePath);

                    using (StreamWriter streamWriter = new StreamWriter(_filePath, true, destinationEncoding))
                    {
                        streamWriter.Write(sourceText);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }


                    //byte[] decoded = Encoding.Convert(sourceEncoding,
                    //                                  destinationEncoding,
                    //                                  bytes,
                    //                                  0,
                    //                                  bytes.Length);

                    //using (FileStream fileStream = new FileStream(_filePath, FileMode.OpenOrCreate))
                    //{
                    //    fileStream.Write(decoded, 0, decoded.Length);
                    //    fileStream.Close();
                    //}
                }

                retVal = true;
            }
            catch (Exception exception)
            {
                componentEvents.FireError(0,
                                       "SSISEncodeFileTask",
                                       string.Format("Error in Encode function:{0} {1} {2}", exception.Message, exception.Source, exception.StackTrace),
                                       string.Empty,
                                       0);
            }
            return retVal;
        }

        public bool Encode()
        {
            bool retVal = false;
            bool refFire = false;
            try
            {
                lock (lockObject)
                {
                    Encoding sourceEncoding;
                    using (StreamReader sourceEncodingRead = new StreamReader(_filePath, true))
                    {
                        sourceEncoding = sourceEncodingRead.CurrentEncoding;
                        sourceEncodingRead.Close();
                    }

                    byte[] bytes;
                    using (FileStream openRead = File.OpenRead(_filePath))
                    {
                        bytes = new byte[openRead.Length];
                        openRead.Read(bytes, 0, (int)openRead.Length);
                        openRead.Close();
                    }

                    Encoding destinationEncoding = Encoding.GetEncoding(_destinationEncoding);

                    byte[] decoded = Encoding.Convert(sourceEncoding, destinationEncoding, bytes, 0, bytes.Length);

                    using (FileStream openWrite = File.OpenWrite(_filePath))
                    {
                        openWrite.Write(decoded, 0, decoded.Length);
                        openWrite.Close();
                    }
                }

                retVal = true;
            }
            catch
            {
                retVal = false;
            }
            return retVal;
        }

        #endregion
    }
}