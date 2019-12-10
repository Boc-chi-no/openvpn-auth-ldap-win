using System;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;

namespace WindowsADLogin
{
    public class Logs
    {
        private static string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LoginLog");
        private static object _lockerForLog = new object();

        /// <summary>
        /// Log
        /// </summary>
        /// <param name="content"></param>
        /// 
        public static void SaveLog(string content)
        {
            bool enableLog = true;
            string enableLogConfig = ConfigurationManager.AppSettings["Log"];
           
            if (!string.IsNullOrEmpty(enableLogConfig))
            {
                if (bool.TryParse(enableLogConfig, out enableLog))
                {
                    if (!enableLog) return;
                }
            }
            
            try
            {
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }

                lock (_lockerForLog)
                {
                    FileStream fs;
                    fs = new FileStream(Path.Combine(_logPath, DateTime.Now.ToString("yyyyMMdd") + ".log"), FileMode.OpenOrCreate);
                    StreamWriter streamWriter = new StreamWriter(fs);
                    streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    streamWriter.WriteLine("[" + DateTime.Now.ToString() + "]：" + content);
                    streamWriter.Flush();
                    fs.Close();
                }
            }
            catch
            {
            }
        }
    }
    class Program
    {
        private static string domain = "contoso.com";
        private static bool enableInheritanceDetection = false;
        private static string AccessGroup = "VPN";
        private static string GroupReadAdmin = "ReadGroup";
        private static string Password = "1234abcd!@#$";

        static bool Auth(string userAccount, string userPassword)
        {

            using (DirectoryEntry deUser = new DirectoryEntry(@"LDAP://" + domain, userAccount, userPassword))
            {
                DirectorySearcher directorySearcher = new DirectorySearcher(deUser);
                directorySearcher.Filter = "(&(&(objectCategory=person)(objectClass=user))(sAMAccountName=" + userAccount + "))";
                directorySearcher.PropertiesToLoad.Add("cn");
                directorySearcher.PropertiesToLoad.Add("memberof");
                directorySearcher.SearchRoot = deUser;
                directorySearcher.SearchScope = SearchScope.Subtree;
                try
                {
                    SearchResult result = directorySearcher.FindOne();
                    if (result != null)
                    {
                        string[] memberof = new string[result.Properties["memberof"].Count];
                        int i = 0;
                        foreach (Object myColl in result.Properties["memberof"])
                        {
                            memberof[i] = myColl.ToString().Substring(3, myColl.ToString().IndexOf(",") - 3);
                            if (memberof[i] == AccessGroup)
                            {
                                return true;
                            }
                            i++;
                        }
                        if (enableInheritanceDetection)
                        {
                            foreach (string GroupName in memberof)
                            {
                                if (InheritanceDetection(GroupName, AccessGroup))
                                    return true;
                            }
                        }
                        Logs.SaveLog(String.Format("User logged in successfully, but is not a member of the access group {0}", AccessGroup));
                    }
                    else
                    {
                        Logs.SaveLog("User exception, DC did not return data");
                    }
                }
                catch (DirectoryServicesCOMException ex) when (ex.ErrorCode == -2147023570)
                {
                    Logs.SaveLog(String.Format("username or password are invalid {0}:{1}", userAccount, userPassword));
                }
                catch (System.Runtime.InteropServices.COMException ex) when (ex.ErrorCode == -2147016646)
                {
                    Logs.SaveLog("Domain connection timeout");
                }
                catch (Exception ex)
                {
                    Logs.SaveLog(String.Format("Error Authenticating User. {0}", ex.Message));
                }
                return false;
            }
        }

        private static bool InheritanceDetection(string GroupName, string RoleName)
        {
            bool isFind = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry(@"LDAP://" + domain, GroupReadAdmin, Password);
                DirectorySearcher mySearcher = new DirectorySearcher(entry);

                mySearcher.PropertiesToLoad.Add("memberof");
                SearchResult mysr = mySearcher.FindOne();
                string memberof;
                if (mysr.Properties.Count > 1)
                {
                    foreach (Object myColl in mysr.Properties["memberof"])
                    {
                        memberof = myColl.ToString().Substring(3, myColl.ToString().IndexOf(",") - 3);
                        if (memberof == RoleName)
                        {
                            isFind = true;
                            break;
                        }
                        else if (InheritanceDetection(memberof, RoleName))
                        {
                            isFind = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.SaveLog(String.Format("InheritanceDetection Exception {0}", ex.Message));
                return false;
            }
            return isFind;
        }

        static int Main(string[] args)
        {
            try
            {
                string enableInheritanceDetectionConfig = ConfigurationManager.AppSettings["InheritanceDetection"];
                if (!string.IsNullOrEmpty(enableInheritanceDetectionConfig))
                {
                    if (!bool.TryParse(enableInheritanceDetectionConfig, out enableInheritanceDetection))
                    {
                        throw new Exception();
                    }
                }
            }
            catch
            {
                enableInheritanceDetection = false;
                Logs.SaveLog("InheritanceDetection is Wrong value Default is false");
            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["Domain"]))
            {
                domain = ConfigurationManager.AppSettings["Domain"];
            }
            else
            {
                Logs.SaveLog("Domain not is Empty");
                return 1;
            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["AccessGroup"]))
            {
                AccessGroup = ConfigurationManager.AppSettings["AccessGroup"];
            }
            else
            {
                Logs.SaveLog("AccessGroup not is Empty");
                return 1;
            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ReadGroupUser"]))
            {
                GroupReadAdmin = ConfigurationManager.AppSettings["ReadGroupUser"];
            }
            else
            {
                Logs.SaveLog("ReadGroupUser not is Empty");
                return 1;
            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["Password"]))
            {
                Password = ConfigurationManager.AppSettings["Password"];
            }
            else
            {
                Logs.SaveLog("Password not is Empty");
                return 1;

            }
            string userAccount = Environment.GetEnvironmentVariable("USERNAME");
            string userPassword = Environment.GetEnvironmentVariable("PASSWORD");

            if (string.IsNullOrEmpty(userAccount) || string.IsNullOrEmpty(userPassword))
            {
                Logs.SaveLog("Authentication transfer error");
                return 1;
            }
            if (Auth(userAccount, userPassword)) {
                Logs.SaveLog(String.Format("{0} Login successfully", userAccount));
                return 0;
            } 
            else return 1;
        }
    }
}
