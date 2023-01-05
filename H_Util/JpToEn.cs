using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//https://ameblo.jp/ponkotsuameba/entry-11599269577.html
namespace H_Util
{
    /// <summary>
    /// Created By Jickie阿文
    /// </summary>
    public class JpToEn
    {
        private bool FInitialized = false;
        private const int S_OK = 0;
        private const int CLSCTX_LOCAL_SERVER = 4;
        private const int CLSCTX_INPROC_SERVER = 1;
        private const int CLSCTX_INPROC_HANDLER = 2;
        private const int CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER;
        private const int FELANG_REQ_REV = 0x00030000;
        private const int FELANG_CMODE_PINYIN = 0x00002000;
        private const int FELANG_CMODE_NOINVISIBLECHAR = 0x40000000;

        [DllImport("ole32.dll")]
        private static extern int CLSIDFromString([MarshalAs(UnmanagedType.LPWStr)] string lpsz, out Guid pclsid);

        [DllImport("ole32.dll")]
        private static extern int CoCreateInstance([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
            IntPtr pUnkOuter, uint dwClsContext, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr rpv);

        [DllImport("ole32.dll")]
        private static extern int CoInitialize(IntPtr pvReserved);

        [DllImport("ole32.dll")]
        private static extern int CoUninitialize();

        //-------------------------------------------------------------------------------------
        // Constructor
        public JpToEn()
        {
            int res = CoInitialize(IntPtr.Zero);
            if (res == S_OK) FInitialized = true;
        }

        public void Dispose()
        {
            if (FInitialized) { CoUninitialize(); FInitialized = false; }
        }

        // Destructor
        ~JpToEn()
        {
            if (FInitialized)
                CoUninitialize();
        }

        public string GetReadString(string value, MSIME imetype = MSIME.Japan)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value)) { return value; }
            string readString = String.Empty;
            Guid pclsid;
            int res;
            //  Get CLSID's pointer from text's CLSID
            res = CLSIDFromString(imetype.GetName(), out pclsid);
            if (res != S_OK) { this.Dispose(); return readString; }
            Guid riid = new Guid("019F7152-E6DB-11D0-83C3-00C04FDDB82E ");
            IntPtr ppv;
            res = CoCreateInstance(pclsid, IntPtr.Zero, CLSCTX_SERVER, riid, out ppv);
            if (res != S_OK) { this.Dispose(); return readString; }
            IFELanguage language = Marshal.GetTypedObjectForIUnknown(ppv, typeof(IFELanguage)) as IFELanguage;
            res = language.Open();
            if (res != S_OK) { this.Dispose(); return readString; }
            IntPtr result;
            res = language.GetJMorphResult(FELANG_REQ_REV, FELANG_CMODE_PINYIN | FELANG_CMODE_NOINVISIBLECHAR, value.Length, value, IntPtr.Zero, out result);
            if (res != S_OK) { this.Dispose(); return readString; }
            readString = Marshal.PtrToStringUni(Marshal.ReadIntPtr(result, 4), Marshal.ReadInt16(result, 8));
            language.Close();
            return readString;
        }

        private string HiraganaToAlphabet(string s1)
        {
            string s2 = "";
            for (int i = 0; i < s1.Length; i++)
            {
                // 小さい文字が含まれる場合
                if (i + 1 < s1.Length)
                {
                    // 「っ」が含まれる場合
                    if (s1.Substring(i, 1).CompareTo("っ") == 0)
                    {
                        s2 += HiraganaToAlphabet1(s1.Substring(i + 1, 1)).Substring(0, 1);
                        continue;
                    }


                    // それ以外の小さい文字
                    string s3 = HiraganaToAlphabet1(s1.Substring(i, 2));
                    if (s3.CompareTo("*") != 0)
                    {
                        s2 += s3;
                        i++;
                        continue;
                    }
                }
                s2 += HiraganaToAlphabet1(s1.Substring(i, 1));
            }
            return s2;
        }
        private string HiraganaToAlphabet1(string s1)
        {
            switch (s1)
            {
                case "あ": return "a";
                case "い": return "i";
                case "う": return "u";
                case "え": return "e";
                case "お": return "o";
                case "か": return "ka";
                case "き": return "ki";
                case "く": return "ku";
                case "け": return "ke";
                case "こ": return "ko";
                case "さ": return "sa";
                case "し": return "shi";
                case "す": return "su";
                case "せ": return "se";
                case "そ": return "so";
                case "た": return "ta";
                case "ち": return "chi";
                case "つ": return "tsu";
                case "て": return "te";
                case "と": return "to";
                case "な": return "na";
                case "に": return "ni";
                case "ぬ": return "nu";
                case "ね": return "ne";
                case "の": return "no";
                case "は": return "ha";
                case "ひ": return "hi";
                case "ふ": return "hu";
                case "へ": return "he";
                case "ほ": return "ho";
                case "ま": return "ma";
                case "み": return "mi";
                case "む": return "mu";
                case "め": return "me";
                case "も": return "mo";
                case "や": return "ya";
                case "ゆ": return "yu";
                case "よ": return "yo";
                case "ら": return "ra";
                case "り": return "ri";
                case "る": return "ru";
                case "れ": return "re";
                case "ろ": return "ro";
                case "わ": return "wa";
                case "を": return "wo";
                case "ん": return "n";
                case "が": return "ga";
                case "ぎ": return "gi";
                case "ぐ": return "gu";
                case "げ": return "ge";
                case "ご": return "go";
                case "ざ": return "za";
                case "じ": return "ji";
                case "ず": return "zu";
                case "ぜ": return "ze";
                case "ぞ": return "zo";
                case "だ": return "da";
                case "ぢ": return "ji";
                case "づ": return "du";
                case "で": return "de";
                case "ど": return "do";
                case "ば": return "ba";
                case "び": return "bi";
                case "ぶ": return "bu";
                case "べ": return "be";
                case "ぼ": return "bo";
                case "ぱ": return "pa";
                case "ぴ": return "pi";
                case "ぷ": return "pu";
                case "ぺ": return "pe";
                case "ぽ": return "po";
                case "きゃ": return "kya";
                case "きぃ": return "kyi";
                case "きゅ": return "kyu";
                case "きぇ": return "kye";
                case "きょ": return "kyo";
                case "しゃ": return "sha";
                case "しぃ": return "syi";
                case "しゅ": return "shu";
                case "しぇ": return "she";
                case "しょ": return "sho";
                case "ちゃ": return "cha";
                case "ちぃ": return "cyi";
                case "ちゅ": return "chu";
                case "ちぇ": return "che";
                case "ちょ": return "cho";
                case "にゃ": return "nya";
                case "にぃ": return "nyi";
                case "にゅ": return "nyu";
                case "にぇ": return "nye";
                case "にょ": return "nyo";
                case "ひゃ": return "hya";
                case "ひぃ": return "hyi";
                case "ひゅ": return "hyu";
                case "ひぇ": return "hye";
                case "ひょ": return "hyo";
                case "みゃ": return "mya";
                case "みぃ": return "myi";
                case "みゅ": return "myu";
                case "みぇ": return "mye";
                case "みょ": return "myo";
                case "りゃ": return "rya";
                case "りぃ": return "ryi";
                case "りゅ": return "ryu";
                case "りぇ": return "rye";
                case "りょ": return "ryo";
                case "ぎゃ": return "gya";
                case "ぎぃ": return "gyi";
                case "ぎゅ": return "gyu";
                case "ぎぇ": return "gye";
                case "ぎょ": return "gyo";
                case "じゃ": return "ja";
                case "じぃ": return "ji";
                case "じゅ": return "ju";
                case "じぇ": return "je";
                case "じょ": return "jo";
                case "ぢゃ": return "dya";
                case "ぢぃ": return "dyi";
                case "ぢゅ": return "dyu";
                case "ぢぇ": return "dye";
                case "ぢょ": return "dyo";
                case "びゃ": return "bya";
                case "びぃ": return "byi";
                case "びゅ": return "byu";
                case "びぇ": return "bye";
                case "びょ": return "byo";
                case "ぴゃ": return "pya";
                case "ぴぃ": return "pyi";
                case "ぴゅ": return "pyu";
                case "ぴぇ": return "pye";
                case "ぴょ": return "pyo";
                case "ぐぁ": return "gwa";
                case "ぐぃ": return "gwi";
                case "ぐぅ": return "gwu";
                case "ぐぇ": return "gwe";
                case "ぐぉ": return "gwo";
                case "つぁ": return "tsa";
                case "つぃ": return "tsi";
                case "つぇ": return "tse";
                case "つぉ": return "tso";
                case "ふぁ": return "fa";
                case "ふぃ": return "fi";
                case "ふぇ": return "fe";
                case "ふぉ": return "fo";
                case "うぁ": return "wha";
                case "うぃ": return "whi";
                case "うぅ": return "whu";
                case "うぇ": return "whe";
                case "うぉ": return "who";
                case "ヴぁ": return "va";
                case "ヴぃ": return "vi";
                case "ヴ": return "vu";
                case "ヴぇ": return "ve";
                case "ヴぉ": return "vo";
                case "でゃ": return "dha";
                case "でぃ": return "dhi";
                case "でゅ": return "dhu";
                case "でぇ": return "dhe";
                case "でょ": return "dho";
                case "てゃ": return "tha";
                case "てぃ": return "thi";
                case "てゅ": return "thu";
                case "てぇ": return "the";
                case "てょ": return "tho";
                default: return "*";
            }
        }

        public string GetJpToEn(string str) {
            try
            {
                return HiraganaToAlphabet(GetReadString(str));
            }
            catch (Exception)
            {
                return "";
            }            
        }

    } // end of ImeLanguage class

    public enum MSIME { China, Taiwan, Japan, Korea }
    public static class Extensions
    {
        public static string GetName(this MSIME value)
        {
            return string.Format("{0}.{1}", typeof(MSIME).Name, Enum.GetName(typeof(MSIME), value));
        }
    }

    //**************************************************************************************
    // IFELanguage Interface
    //**************************************************************************************
    [ComImport]
    [Guid("019F7152-E6DB-11D0-83C3-00C04FDDB82E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFELanguage
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int Open();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int Close();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetJMorphResult(uint dwRequest, uint dwCMode, int cwchInput,
            [MarshalAs(UnmanagedType.LPWStr)] string pwchInput, IntPtr pfCInfo, out IntPtr ppResult);
    } // end of IFELanguage Interface

    
   
}
