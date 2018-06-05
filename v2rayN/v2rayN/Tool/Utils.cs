﻿using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace v2rayN
{
    class Utils
    {
        private static string autoRunName = "v2rayNAutoRun";
        private static string autoRunRegPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        #region 资源Json操作

        /// <summary>
        /// 获取嵌入文本资源
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static string GetEmbedText(string res)
        {
            string result = string.Empty;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(res))
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch
            {
            }
            return result;
        }


        /// <summary>
        /// 取得存储资源
        /// </summary>
        /// <returns></returns>
        public static string LoadResource(string res)
        {
            string result = string.Empty;

            try
            {
                using (StreamReader reader = new StreamReader(res))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch
            {
            }
            return result;
        }

        /// <summary>
        /// 反序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strJson"></param>
        /// <returns></returns>
        public static T FromJson<T>(string strJson)
        {
            try
            {
                T obj = JsonConvert.DeserializeObject<T>(strJson);
                return obj;
            }
            catch
            {
                return JsonConvert.DeserializeObject<T>("");
            }
        }

        /// <summary>
        /// 序列化成Json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(Object obj)
        {
            string result = string.Empty;
            try
            {
                result = JsonConvert.SerializeObject(obj,
                                           Formatting.Indented,
                                           new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch
            {
            }
            return result;
        }

        /// <summary>
        /// 保存成json文件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static int ToJsonFile(Object obj, string filePath)
        {
            int result = -1;
            try
            {
                using (StreamWriter file = System.IO.File.CreateText(filePath))
                {
                    //JsonSerializer serializer = new JsonSerializer();
                    JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
                    //JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };

                    serializer.Serialize(file, obj);
                }
                result = 0;
            }
            catch
            {
                result = -1;
            }
            return result;
        }
        #endregion

        #region 转换函数

        /// <summary>
        /// List<string>转逗号分隔的字符串
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static string List2String(List<string> lst)
        {
            try
            {
                return string.Join(",", lst.ToArray());
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 逗号分隔的字符串,转List<string>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<string> String2List(string str)
        {
            try
            {
                return new List<string>(str.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(string plainText)
        {
            try
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return Convert.ToBase64String(plainTextBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Decode(string plainText)
        {
            try
            {
                byte[] data = Convert.FromBase64String(plainText);
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 转Int
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int ToInt(object obj)
        {
            try
            {
                return Convert.ToInt32(obj);
            }
            catch
            {
                return 0;
            }
        }

        #endregion


        #region 数据检查

        /// <summary>
        /// 判断输入的是否是数字
        /// </summary>
        /// <param name="oText"></param>
        /// <returns></returns>
        public static bool IsNumberic(string oText)
        {
            try
            {
                int var1 = Utils.ToInt(oText);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 文本
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }
            if (text.Equals("null"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 验证IP地址是否合法
        /// </summary>
        /// <param name="ip"></param>        
        public static bool IsIP(string ip)
        {
            //如果为空
            if (IsNullOrEmpty(ip))
            {
                return false;
            }

            //清除要验证字符串中的空格
            //ip = ip.Trim();

            //模式字符串
            string pattern = @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$";

            //验证
            return IsMatch(ip, pattern);
        }

        /// <summary>
        /// 验证Domain地址是否合法
        /// </summary>
        /// <param name="domain"></param>        
        public static bool IsDomain(string domain)
        {
            //如果为空
            if (IsNullOrEmpty(domain))
            {
                return false;
            }

            //清除要验证字符串中的空格
            //domain = domain.Trim();

            //模式字符串
            string pattern = @"^(?=^.{3,255}$)[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+$";

            //验证
            return IsMatch(domain, pattern);
        }

        /// <summary>
        /// 验证输入字符串是否与模式字符串匹配，匹配返回true
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="pattern">模式字符串</param>        
        public static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }

        #endregion

        #region 开机自动启动

        /// <summary>
        /// 开机自动启动
        /// </summary>
        /// <param name="run"></param>
        /// <returns></returns>
        public static int SetAutoRun(bool run)
        {
            try
            {
                RegistryKey regKey = Registry.LocalMachine.CreateSubKey(autoRunRegPath);
                if (run)
                {
                    string exePath = Application.ExecutablePath;
                    regKey.SetValue(autoRunName, exePath);
                }
                else
                {
                    regKey.DeleteValue(autoRunName, false);
                }
                regKey.Close();
            }
            catch
            {
            }
            return 0;
        }

        /// <summary>
        /// 是否已经设置开机自动启动
        /// </summary>
        /// <returns></returns>
        public static bool IsAutoRun()
        {
            try
            {
                RegistryKey regKey = Registry.LocalMachine.CreateSubKey(autoRunRegPath);
                string value = regKey.GetValue(autoRunName).ToString();
                string exePath = GetExePath();
                if (value.Equals(exePath))
                {
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// 获取启动了应用程序的可执行文件的路径
        /// </summary>
        /// <returns></returns>
        public static string GetPath(string fileName)
        {
            string StartupPath = Application.StartupPath;
            if (Utils.IsNullOrEmpty(fileName))
            {
                return StartupPath;
            }
            return string.Format("{0}\\{1}", StartupPath, fileName);

        }

        /// <summary>
        /// 获取启动了应用程序的可执行文件的路径及文件名
        /// </summary>
        /// <returns></returns>
        public static string GetExePath()
        {
            return System.Windows.Forms.Application.ExecutablePath;
        }

        #endregion

        #region 测速

        /// <summary>
        /// Ping
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static long Ping(string host)
        {
            long roundtripTime = 0;
            try
            {
                long totalTime = 0;
                int timeout = 120;
                int echoNum = 3;
                Ping pingSender = new Ping();
                for (int i = 0; i < echoNum; i++)
                {
                    PingReply reply = pingSender.Send(host, timeout);
                    if (reply.Status == IPStatus.Success)
                    {
                        totalTime += reply.RoundtripTime;
                    }
                }
                roundtripTime = totalTime / echoNum;
            }
            catch
            {
                return -1;
            }
            return roundtripTime;
        }

        /// <summary>
        /// 取得本机 IP Address
        /// </summary>
        /// <returns></returns>
        public static List<string> GetHostIPAddress()
        {
            List<string> lstIPAddress = new List<string>();
            try
            {
                IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ipa in IpEntry.AddressList)
                {
                    if (ipa.AddressFamily == AddressFamily.InterNetwork)
                        lstIPAddress.Add(ipa.ToString());
                }
            }
            catch
            {
            }
            return lstIPAddress;
        }


        #endregion

        #region 杂项

        /// <summary>
        /// 取得版本
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            try
            {
                string location = GetExePath();
                return string.Format("v2rayN - V{0} - {1}",
                        FileVersionInfo.GetVersionInfo(location).FileVersion.ToString(),
                        File.GetLastWriteTime(location).ToString("yyyy/MM/dd"));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 深度拷贝
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                //序列化成流
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                //反序列化成对象
                retval = bf.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }

        /// <summary>
        /// 获取剪贴板数
        /// </summary>
        /// <returns></returns>
        public static string GetClipboardData()
        {
            string strData = string.Empty;
            try
            {
                IDataObject data = Clipboard.GetDataObject();
                if (data.GetDataPresent(DataFormats.Text))
                {
                    strData = data.GetData(DataFormats.Text).ToString();
                }
                return strData;
            }
            catch
            {
            }
            return strData;
        }

        /// <summary>
        /// 拷贝至剪贴板
        /// </summary>
        /// <returns></returns>
        public static void SetClipboardData(string strData)
        {
            try
            {
                Clipboard.SetText(strData);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 取得GUID
        /// </summary>
        /// <returns></returns>
        public static string GetGUID()
        {
            try
            {
                return Guid.NewGuid().ToString("D");
            }
            catch
            {
            }
            return string.Empty;
        }

        #endregion

        #region TempPath

        private static string _tempPath = null;

        // return path to store temporary files
        public static string GetTempPath()
        {
            if (_tempPath == null)
            {
                Directory.CreateDirectory(Path.Combine(Application.StartupPath, "v2ray_win_temp"));
                // don't use "/", it will fail when we call explorer /select xxx/ss_win_temp\xxx.log
                _tempPath = Path.Combine(Application.StartupPath, "v2ray_win_temp");
            }
            return _tempPath;
        }

        public static string GetTempPath(string filename)
        {
            return Path.Combine(GetTempPath(), filename);
        }

        public static void ClearTempPath()
        {
            //Directory.Delete(GetTempPath(), true);
            //_tempPath = null;
        }

        public static string UnGzip(byte[] buf)
        {
            byte[] buffer = new byte[1024];
            int n;
            using (MemoryStream sb = new MemoryStream())
            {
                using (GZipStream input = new GZipStream(new MemoryStream(buf),
                    CompressionMode.Decompress,
                    false))
                {
                    while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        sb.Write(buffer, 0, n);
                    }
                }
                return System.Text.Encoding.UTF8.GetString(sb.ToArray());
            }
        }

        #endregion

        #region Log

        public static void SaveLog(string strContent)
        {
            SaveLog("info", new Exception(strContent));
        }
        public static void SaveLog(string strTitle, Exception ex)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "guiLogs");
                string FilePath = Path.Combine(path, DateTime.Now.ToString("yyyyMMdd") + ".txt");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!File.Exists(FilePath))
                {
                    FileStream FsCreate = new FileStream(FilePath, FileMode.Create);
                    FsCreate.Close();
                    FsCreate.Dispose();
                }
                FileStream FsWrite = new FileStream(FilePath, FileMode.Append, FileAccess.Write);
                StreamWriter SwWrite = new StreamWriter(FsWrite);

                string strContent = ex.ToString();

                SwWrite.WriteLine(string.Format("{0}{1}[{2}]{3}", "--------------------------------", strTitle, DateTime.Now.ToString("HH:mm:ss"), "--------------------------------"));
                SwWrite.Write(strContent);
                SwWrite.WriteLine("\r\n");
                SwWrite.WriteLine(" ");
                SwWrite.Flush();
                SwWrite.Close();
            }
            catch { }
        }

        #endregion


        #region scan screen

        public static string ScanScreen()
        {
            string ret = string.Empty;
            try
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    using (Bitmap fullImage = new Bitmap(screen.Bounds.Width,
                                                    screen.Bounds.Height))
                    {
                        using (Graphics g = Graphics.FromImage(fullImage))
                        {
                            g.CopyFromScreen(screen.Bounds.X,
                                             screen.Bounds.Y,
                                             0, 0,
                                             fullImage.Size,
                                             CopyPixelOperation.SourceCopy);
                        }
                        int maxTry = 10;
                        for (int i = 0; i < maxTry; i++)
                        {
                            int marginLeft = (int)((double)fullImage.Width * i / 2.5 / maxTry);
                            int marginTop = (int)((double)fullImage.Height * i / 2.5 / maxTry);
                            Rectangle cropRect = new Rectangle(marginLeft, marginTop, fullImage.Width - marginLeft * 2, fullImage.Height - marginTop * 2);
                            Bitmap target = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);

                            double imageScale = (double)screen.Bounds.Width / (double)cropRect.Width;
                            using (Graphics g = Graphics.FromImage(target))
                            {
                                g.DrawImage(fullImage, new Rectangle(0, 0, target.Width, target.Height),
                                                cropRect,
                                                GraphicsUnit.Pixel);
                            }

                            var source = new BitmapLuminanceSource(target);
                            var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                            QRCodeReader reader = new QRCodeReader();
                            var result = reader.decode(bitmap);
                            if (result != null)
                            {
                                ret = result.Text;
                                return ret;
                            }
                        }
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        #endregion

    }
}
