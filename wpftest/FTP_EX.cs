using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Media.Imaging;

//*******************************************************************************
//프로그램명    FTP_EX.cs
//메뉴ID        
//설명          공통 메서드
//작성일        2016.12.06
//개발자        김은미
//*******************************************************************************
// 변경일자     변경자      요청자      요구사항ID          요청 및 작업내용
//*******************************************************************************
//
//
//*******************************************************************************

namespace WizMes_EVC
{
    public class FTP_EX
    {
        private string host = null;
        private string user = null;
        private string pass = null;

        private FtpWebRequest ftpRequest = null;
        private FtpWebResponse ftpResponse = null;
        private Stream ftpStream = null;
        private int bufferSize = 2048;

        /* Construct Object */
        public FTP_EX(string hostIP, string userName, string password) { host = hostIP; user = userName; pass = password; }

        // 20191210 둘리 - 테스트
        public byte[] getFileToByte(string remoteFile)
        {
            byte[] file = null;

            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
                ftpRequest.Credentials = new NetworkCredential(user, pass);

                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                Stream responseStream = ftpResponse.GetResponseStream();

                file = ToByteArray(responseStream);

                responseStream.Close();
            }
            catch (Exception ex9)
            {
                System.Windows.MessageBox.Show("바이트 변환 실패임여");
            }

            return file;
        }
        public Byte[] ToByteArray(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            byte[] chunk = new byte[4096];
            int bytesRead;
            while ((bytesRead = stream.Read(chunk, 0, chunk.Length)) > 0)
            {
                ms.Write(chunk, 0, bytesRead);
            }

            return ms.ToArray();
        }
        // 20191210 둘리 - 파일 업로드 테스트
        public bool UploadUsingByte(byte[] buffer, string FileName)
        {
            bool flag = false;

            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + FileName);
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                Stream reqStream = ftpRequest.GetRequestStream();
                reqStream.Write(buffer, 0, buffer.Length);
                reqStream.Close();
            }
            catch (Exception ex9)
            {
                System.Windows.MessageBox.Show("저장 실패임여");
            }

            return flag;
        }
        // 20191211 둘리 - 파일 업로드 - 리스트로 넣어봅세
        public bool UploadUsingByteList(Dictionary<string, byte[]> lstFileByte, string FolderName)
        {
            bool flag = false;

            try
            {
                foreach (string FileName in lstFileByte.Keys)
                {
                    FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + FolderName + "/" + FileName);
                    ftpRequest.Credentials = new NetworkCredential(user, pass);
                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                    Stream reqStream = ftpRequest.GetRequestStream();
                    reqStream.Write(lstFileByte[FileName], 0, lstFileByte[FileName].Length);
                    reqStream.Close();
                }
            }
            catch (Exception ex9)
            {
                System.Windows.MessageBox.Show("저장 실패임여");
            }

            return flag;
        }

        // 20191211 둘리 → 요거는 FTP Server File Path 를 받아서, 그걸 다운로드 한 다음에 그걸로다가 바로 업로드 시켜버리는 것이여
        public bool UploadUsingFtpServerFilePath(Dictionary<string, string> lstFtpFilePath, string FolderName)
        {
            bool flag = false;

            try
            {
                foreach (string FileName in lstFtpFilePath.Keys)
                {
                    // 일단 다운로드 먼저 받아야 쓰것어
                    WebClient ftpClient = new WebClient();
                    ftpClient.Credentials = new NetworkCredential(user, pass);
                    byte[] imageByte = ftpClient.DownloadData(host + "/" + lstFtpFilePath[FileName] + "/" + FileName);

                    // 이것은 업로드여
                    FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + FolderName + "/" + FileName);
                    ftpRequest.Credentials = new NetworkCredential(user, pass);
                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                    Stream reqStream = ftpRequest.GetRequestStream();
                    reqStream.Write(imageByte, 0, imageByte.Length);
                    ftpRequest = null;
                    reqStream.Close();
                }
            }
            catch (Exception ex9)
            {
                System.Windows.MessageBox.Show("저장 실패임여");
            }

            return flag;
        }



        /* Download File */
        public bool download(string remoteFile, string localFile, bool showMsg = true)
        {
            try
            {
                /* Create an FTP Request */
                //string url = HttpUtility.UrlEncode("#");
                //url = remoteFile.Replace("#", url);

                //ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + url);
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Get the FTP Server's Response Stream */
                ftpStream = ftpResponse.GetResponseStream();
                /* Open a File Stream to Write the Downloaded File */
                FileStream localFileStream = new FileStream(localFile, FileMode.Create);
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[bufferSize];
                int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                    }
                }
                catch (Exception ex)
                {
                    if(showMsg == true) System.Windows.MessageBox.Show("1" + ex.Message + " / " + ex.Source);
                    throw;
                }
                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                return true;
            }
            catch (Exception ex)
            {
                if (showMsg == true)  System.Windows.MessageBox.Show("2" + ex.Message + " / " + ex.Source);
                return false;
                //throw ex;
            }
        }

        /* Upload File */
        public bool upload(string remoteFile, string localFile)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpRequest.GetRequestStream();
                /* Open a File Stream to Read the File for Upload */
                FileStream localFileStream = new FileStream(localFile, FileMode.Open);
                /* Buffer for the Upload Data */
                byte[] byteBuffer = new byte[bufferSize];
                int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesSent != 0)
                    {
                        ftpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpRequest = null;
                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return false;
        }


        /* Upload Files */
        public bool uploads(string remoteFile, List<string> strName, List<string> localFile)
        {
            try
            {
                for (int i = 0; i < strName.Count; i++)
                {
                    /* Create an FTP Request */
                    ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile + strName[i]);
                    /* When in doubt, use these options */
                    ftpRequest.UseBinary = true;
                    ftpRequest.UsePassive = true;
                    ftpRequest.Timeout = -1;
                    ftpRequest.KeepAlive = false;
                    /* Log in to the FTP Server with the User Name and Password Provided */
                    ftpRequest.Credentials = new NetworkCredential(user, pass);
                    /* Specify the Type of FTP Request */
                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    /* Open a File Stream to Read the File for Upload */
                    FileStream fs = File.OpenRead(localFile[i] + strName[i]);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                    Stream requestStr = ftpRequest.GetRequestStream();
                    requestStr.Write(buffer, 0, buffer.Length);
                    requestStr.Close();
                    requestStr.Flush();
                    FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                    response.Close();
                    //File.Delete(localFile[i] + strName[i]);
                }
                ftpRequest = null;
                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return false;
        }



        /// <summary>
        /// upload 할 file의 name and Full Path를 string 배열에 넣는 방식으로 
        /// upload할 모든 string 배열을 List에 담아주면 모두 업로드
        /// </summary>
        public bool UploadTempFilesToFTP(List<string[]> remoteFiles)
        {
            List<string[]> fileList;
            try
            {
                fileList = remoteFiles;
                for (int i = 0; i < fileList.Count; i++)
                {
                    string FileName = fileList[i][0];
                    string localFile = fileList[i][1];
                    FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + FileName.Replace("#", "%23"));
                    ftpRequest.Credentials = new NetworkCredential(user, pass);
                    ftpRequest.UseBinary = true;
                    ftpRequest.UsePassive = true;
                    if (i == fileList.Count - 1)
                    {
                        ftpRequest.KeepAlive = true;
                    }
                    else
                    {
                        ftpRequest.KeepAlive = false;
                    }

                    ftpRequest.Timeout = -1;

                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    string file = FileName.Substring(FileName.IndexOf('/') + 1);
                    FileStream fs = File.OpenRead(localFile + file);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                    Stream requestStr = ftpRequest.GetRequestStream();
                    requestStr.Write(buffer, 0, buffer.Length);
                    requestStr.Close();
                    requestStr.Flush();
                    FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                    response.Close();
                }
                Console.WriteLine("Uploaded Successfully to Temp folder");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Upload failed. {0}", ex.Message);
                return false;
            }

        }

        /* Delete File */
        public bool delete(string deleteFile)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + deleteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return false;
        }

        //2016.12.05.kem
        /* Remove Directory */
        public bool removeDir(string deleteDirectory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + deleteDirectory);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;

                List<string> lines = new List<string>();

                using (FtpWebResponse listResponse = (FtpWebResponse)ftpRequest.GetResponse())
                using (Stream listStream = listResponse.GetResponseStream())
                using (StreamReader listReader = new StreamReader(listStream))
                {
                    while (!listReader.EndOfStream)
                    {
                        lines.Add(listReader.ReadLine());
                    }
                }

                foreach (string line in lines)
                {
                    string[] tokens = line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                    string name = tokens[3].ToString(); //배열이 8이었다 3으로 바꿈
                    string permissions = tokens[0].ToString();

                    string fileUrl = host + "/" + deleteDirectory + "/" + name;

                    FtpWebRequest deleteRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
                    deleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                    deleteRequest.Credentials = new NetworkCredential(user, pass);

                    deleteRequest.GetResponse();
                }

                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + deleteDirectory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return false;
        }

        /* Rename File */
        public bool rename(string currentFileNameAndPath, string newFileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentFileNameAndPath);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.Rename;
                /* Rename the File */
                ftpRequest.RenameTo = newFileName;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return false;
        }

        /* Create a New Directory on the FTP Server */
        //public bool createDirectory(string newDirectory)
        //{
        //    try
        //    {
        //        /* Create an FTP Request */
        //        ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + newDirectory);
        //        /* Log in to the FTP Server with the User Name and Password Provided */
        //        ftpRequest.Credentials = new NetworkCredential(user, pass);
        //        /* When in doubt, use these options */
        //        ftpRequest.UseBinary = true;
        //        ftpRequest.UsePassive = true;
        //        ftpRequest.KeepAlive = true;
        //        /* Specify the Type of FTP Request */
        //        ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
        //        /* Establish Return Communication with the FTP Server */
        //        ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
        //        /* Resource Cleanup */
        //        ftpResponse.Close();
        //        ftpRequest = null;
        //        return true;
        //    }
        //    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        //    return false;
        //}

        public bool createDirectory(string newDirectory, bool createParent = true)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + newDirectory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */
                ftpResponse.Close();
                ftpRequest = null;
                return true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    // 상위 디렉토리 생성 시도
                    return createDirectoryWithParentDir(newDirectory);
                }
                Console.WriteLine($"FTP Error: {response.StatusCode} - {response.StatusDescription}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }


        //부모경로 생성
        public bool createDirectoryWithParentDir(string newDirectory)
        {
            try
            {
                // 프로토콜 이후의 시작 위치를 찾습니다
                int protocolIndex = host.IndexOf("://");
                // 포트 시작 위치를 찾습니다
                int colonIndex = host.IndexOf(":", protocolIndex + 3);
                // 포트 다음의 첫 슬래시 위치를 찾습니다
                int slashIndex = host.IndexOf("/", colonIndex);

                // host에서 기본 경로 추출 
                string basePath = host.Substring(slashIndex + 1);
                string[] pathParts = basePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                // 상위 경로들 순차적으로 생성
                string currentPath = host.Substring(0, slashIndex + 1);
                foreach (string part in pathParts)
                {
                    currentPath += part + "/";
                    try
                    {
                        ftpRequest = (FtpWebRequest)WebRequest.Create(currentPath);
                        ftpRequest.Credentials = new NetworkCredential(user, pass);
                        ftpRequest.UseBinary = true;
                        ftpRequest.UsePassive = true;
                        ftpRequest.KeepAlive = true;
                        ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

                        using (ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                        {
                            // 폴더가 성공적으로 생성되거나 이미 존재하는 경우
                        }
                    }
                    catch (WebException ex)
                    {
                        // 폴더가 이미 존재하는 경우는 무시하고 계속 진행
                        FtpWebResponse response = (FtpWebResponse)ex.Response;
                        if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            throw;
                        }
                    }
                }

                // 최종적으로 요청된 새 디렉토리 생성
                ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + newDirectory);
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

                using (ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }


        /* Get the Date/Time a File was Created */
        public string getFileCreatedDateTime(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string fileInfo = null;
                /* Read the Full Response Stream */
                try { fileInfo = ftpReader.ReadToEnd(); }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return File Created Date Time */
                return fileInfo;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return "";
        }

        /* Get the Size of a File */
        public string getFileSize(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string fileInfo = null;
                /* Read the Full Response Stream */
                try { while (ftpReader.Peek() != -1) { fileInfo = ftpReader.ReadToEnd(); } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return File Size */
                return fileInfo;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return "";
        }

        /* List Directory Contents File/Folder Name Only */
        public string[] directoryListSimple(string directory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { "" };
        }

        /* List Directory Contents in Detail (Name, Size, Created, etc.) */
        public string[] directoryListDetailed(string directory)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { "" };
        }

        //이거 폴더안에 비어있거나 미리 안 만들면 오류 무조건 남
        /* List Directory Contents File/Folder Name Only */
        //public string[] directoryListSimple(string directory, Encoding encoding)
        //{
        //    try
        //    {
        //        /* Create an FTP Request */
        //        ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
        //        /* Log in to the FTP Server with the User Name and Password Provided */
        //        ftpRequest.Credentials = new NetworkCredential(user, pass);
        //        /* When in doubt, use these options */
        //        ftpRequest.UseBinary = true;
        //        ftpRequest.UsePassive = true;
        //        ftpRequest.KeepAlive = true;
        //        /* Specify the Type of FTP Request */
        //        ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
        //        /* Establish Return Communication with the FTP Server */
        //        ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
        //        /* Establish Return Communication with the FTP Server */
        //        ftpStream = ftpResponse.GetResponseStream();
        //        /* Get the FTP Server's Response Stream */
        //        StreamReader ftpReader = new StreamReader(ftpStream, encoding);
        //        /* Store the Raw Response */
        //        string directoryRaw = null;
        //        /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
        //        try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
        //        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        //        /* Resource Cleanup */
        //        ftpReader.Close();
        //        ftpStream.Close();
        //        ftpResponse.Close();
        //        ftpRequest = null;
        //        /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
        //        try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
        //        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        //    }
        //    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        //    /* Return an Empty string Array if an Exception Occurs */
        //    return new string[] { "" };
        //}

        //host의 폴더를 보고 만약 비어있을때 빈 문자열 반환
        public string[] directoryListSimple(string directory, Encoding encoding)
        {
            try
            {
                string fullPath = host + "/" + directory;
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(fullPath);
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

                using (ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                using (ftpStream = ftpResponse.GetResponseStream())
                using (StreamReader ftpReader = new StreamReader(ftpStream, encoding))
                {
                    StringBuilder directoryRaw = new StringBuilder();
                    string line;

                    while ((line = ftpReader.ReadLine()) != null)
                    {
                        directoryRaw.Append(line + "|");
                    }

                    string result = directoryRaw.ToString();
                    if (string.IsNullOrEmpty(result))
                    {
                        return new string[0];
                    }

                    string[] splitResult = result.Split('|');
                    List<string> cleanResult = new List<string>();

                    for (int i = 0; i < splitResult.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(splitResult[i]))
                        {
                            cleanResult.Add(splitResult[i]);
                        }
                    }

                    return cleanResult.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FTP 저장 중 중 실패 Line in 796: {ex.Message}");
                return new string[0];
            }
            finally
            {                     
                ftpRequest = null;      //리소스 해제  IDisposable.Dispose()가 자동호출되어서 null로 충분하다 함..
            }
        }

        /* List Directory Contents in Detail (Name, Size, Created, etc.) */
        public string[] directoryListDetailed(string directory, Encoding encoding)
        {
            try
            {
                /* Create an FTP Request */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream, encoding);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { "" };
        }

        /// <summary>
        /// 그냥 삭제가능으로 가보자
        /// </summary>
        /// <param name="serverUri"></param>
        /// <param name="ftpUsername"></param>
        /// <param name="ftpPassword"></param>
        /// <returns></returns>
        public bool DeleteFileOnFtpServer(Uri serverUri)
        {
            try
            {
                // The serverUri parameter should use the ftp:// scheme.
                // It contains the name of the server file that is to be deleted.
                // Example: ftp://contoso.com/someFile.txt.
                // 

                if (serverUri.Scheme != Uri.UriSchemeFtp)
                {
                    return false;
                }
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
                request.Credentials = new NetworkCredential(user, pass);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                //Console.WriteLine("Delete status: {0}", response.StatusDescription);
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// ftp경로를 가지고 Bitmap 정보 리턴한다
        /// </summary>
        /// <param name="ftpFilePath"></param>
        /// <returns></returns>
        public BitmapImage DrawingImageByByte(string ftpFilePath)
        {
            BitmapImage image = new BitmapImage();

            try
            {
                WebClient ftpClient = new WebClient();
                ftpClient.Credentials = new NetworkCredential(user, pass);
                byte[] imageByte = ftpClient.DownloadData(ftpFilePath);

                //MemoryStream mStream = new MemoryStream();
                //mStream.Write(imageByte, 0, Convert.ToInt32(imageByte.Length));

                using (MemoryStream stream = new MemoryStream(imageByte))
                {
                    image.BeginInit();
                    image.StreamSource = stream;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("1" + ex.Message + " / " + ex.Source);
                //throw ex;
            }

            return image;
        }

        //sourcePath는 복사하고자 하는 소스가 위치한 경로 예 : /ImageData/Reserve
        //newForder는 호출한 곳 경로에 생성 본 문서 FTP클래스를 호출할때 전역변수인 host에 담겨있음
        //filesNames는 소스경로에서 이 파일명을 찾아서 임시 다운하고 새로 만든 폴더에 업로드 후 삭제함
        //파일을 찾지 못하면 넘어갑니다. 
        public bool FTP_copyFiles(string sourcePath, string newFolderName, List<string> fileNames)
        {
            try
            {
                // 새 폴더 생성
                // newFolderName은 보통 PK를 넘겨줍시다.
                createDirectory(newFolderName);

                string baseHost = host;
                // 프로토콜 이후의 시작 위치를 찾습니다
                int protocolIndex = baseHost.IndexOf("://");
                if (protocolIndex > 0)
                {
                    // 프로토콜 이후부터 포트를 찾습니다
                    int colonIndex = baseHost.IndexOf(":", protocolIndex + 3);
                    if (colonIndex > 0)
                    {
                        int slashIndex = baseHost.IndexOf("/", colonIndex);
                        if (slashIndex > 0)
                        {
                            baseHost = baseHost.Substring(0, slashIndex);
                        }
                    }
                }

                foreach (string fileName in fileNames)
                {
                    try
                    {                   
                        // 소스 파일 다운로드를 위한 request
                        FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create($"{baseHost}{sourcePath}/{fileName}");
                        downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                        downloadRequest.Credentials = new NetworkCredential(user, pass);
                        downloadRequest.UseBinary = true;
                        downloadRequest.UsePassive = true;

                        byte[] fileData;
                        using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
                        using (Stream responseStream = downloadResponse.GetResponseStream())
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            responseStream.CopyTo(memoryStream);
                            fileData = memoryStream.ToArray();
                        }

                        // 새 위치에 파일 업로드를 위한 request
                        FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create($"{host}/{newFolderName}/{fileName}");
                        uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
                        uploadRequest.Credentials = new NetworkCredential(user, pass);
                        uploadRequest.UseBinary = true;
                        uploadRequest.UsePassive = true;

                        using (Stream requestStream = uploadRequest.GetRequestStream())
                        {
                            requestStream.Write(fileData, 0, fileData.Length);
                        }

                        using (FtpWebResponse uploadResponse = (FtpWebResponse)uploadRequest.GetResponse())
                        {
                            Console.WriteLine($"업로드 상태: {uploadResponse.StatusDescription}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"다음 파일에 문제가 있습니다 ☞ {fileName}: {ex.Message}");
                        continue;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"에러: {ex.Message}");
                return false;
            }
        }
    }
}
