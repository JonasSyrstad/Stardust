//
// ftpclient.cs
// This file is part of Stardust
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Stardust.Particles.FileTransfer
{
    public sealed class FtpClient : IDisposable
    {
        private const string DtFormat = "yyyyMMddHHmmss";
        private const string BinaryChar = "I";
        private const string AsciiChar = "A";
        public const string Rnfr = "RNFR";
        private const string Rnto = "RNTO";
        public const string Rmd = "RMD";
        public const string Mkd = "MKD";
        public const string Cdw = "CWD";
        public const string Mdtm = "MDTM";
        public const string Syst = "SYST";
        public const string HelpCommand = "HELP ";
        public const string QuitCommand = "QUIT";
        public const string PwdCommand = "PWD";
        public const string Dele = "DELE";
        private FtpControlSocket Control;
        private Socket Data;
        private FtpTransferType TransferTypePrivate = FtpTransferType.Ascii;
        private bool Disposing;

        public FtpClient(string remoteHost)
        {
            ConnectMode = FtpConnectMode.Pasive;
            Control = new FtpControlSocket(remoteHost, FtpControlSocket.ControlPort, null, 0);
        }

        [ExcludeFromCodeCoverage]
        public FtpClient(string remoteHost, int controlPort)
        {
            ConnectMode = FtpConnectMode.Pasive;
            Control = new FtpControlSocket(remoteHost, controlPort, null, 0);
        }

        [ExcludeFromCodeCoverage]
        public FtpClient(IPAddress remoteAddr)
        {
            ConnectMode = FtpConnectMode.Pasive;
            Control = new FtpControlSocket(remoteAddr, FtpControlSocket.ControlPort, null, 0);
        }

        [ExcludeFromCodeCoverage]
        public FtpClient(IPAddress remoteAddr, int controlPort)
        {
            ConnectMode = FtpConnectMode.Pasive;
            Control = new FtpControlSocket(remoteAddr, controlPort, null, 0);
        }

        [ExcludeFromCodeCoverage]
        public FtpClient(string remoteHost, StreamWriter log, int timeout)
        {
            ConnectMode = FtpConnectMode.Pasive;
            Control = new FtpControlSocket(remoteHost, FtpControlSocket.ControlPort, log, timeout);
        }

        public FtpClient(string remoteHost, int controlPort, StreamWriter log, int timeout)
        {
            ConnectMode = FtpConnectMode.Pasive;
            Control = new FtpControlSocket(remoteHost, controlPort, log, timeout);
        }

        [ExcludeFromCodeCoverage]
        public FtpClient(IPAddress remoteAddr, StreamWriter log, int timeout)
        {
            ConnectMode = FtpConnectMode.Pasive;
            Control = new FtpControlSocket(remoteAddr, FtpControlSocket.ControlPort, log, timeout);
        }

        [ExcludeFromCodeCoverage]
        public FtpClient(IPAddress remoteAddr, int controlPort, StreamWriter log, int timeout)
        {
            ConnectMode = FtpConnectMode.Pasive;
            Control = new FtpControlSocket(remoteAddr, controlPort, log, timeout);
        }

        [ExcludeFromCodeCoverage]
        public int Timeout
        {
            set { Control.Timeout = value; }
            get { return Control.Timeout; }
        }

        public FtpConnectMode ConnectMode { get; set; }

        public FtpReply LastValidReply { get; private set; }

        [ExcludeFromCodeCoverage]
        public StreamWriter LogStream
        {
            set { Control.LogStream = value; }
        }

        [ExcludeFromCodeCoverage]
        public FtpTransferType TransferType
        {
            get { return TransferTypePrivate; }
            set
            {
                var typeStr = AsciiChar;
                if (value.Equals(FtpTransferType.Binary))
                    typeStr = BinaryChar;
                var reply = Control.SendCommand("TYPE " + typeStr);
                LastValidReply = Control.ValidateReply(reply, "200");
                TransferTypePrivate = value;
            }
        }

        public void Login(string user, string password)
        {
            SendCommandAndValidateCode(user, "USER", "331");
            SendCommandAndValidateCode(password, "PASS", "230");
        }

        public void User(string user)
        {
            SendCommandAndValidateCode(user, "USER", "230", "331");
        }

        public void Password(string password)
        {
            SendCommandAndValidateCode(password, "PASS", "230", "202");
        }

        [ExcludeFromCodeCoverage]
        public void Quote(string command, string[] validCodes)
        {
            var reply = Control.SendCommand(command);
            if (validCodes != null && validCodes.Length > 0)
                LastValidReply = Control.ValidateReply(reply, validCodes);
        }

        [ExcludeFromCodeCoverage]
        public void Put(string localPath, string remoteFile)
        {
            Put(localPath, remoteFile, false);
        }

        [ExcludeFromCodeCoverage]
        public void Put(Stream srcStream, string remoteFile)
        {
            Put(srcStream, remoteFile, false);
        }

        [ExcludeFromCodeCoverage]
        public void Put(string localPath, string remoteFile, bool append)
        {
            if (TransferType == FtpTransferType.Ascii)
            {
                PutAscii(localPath, remoteFile, append);
            }
            else
            {
                PutBinary(localPath, remoteFile, append);
            }
            ValidateTransfer();
        }

        [ExcludeFromCodeCoverage]
        public void Put(Stream srcStream, string remoteFile, bool append)
        {
            if (TransferType == FtpTransferType.Ascii)
            {
                PutAscii(srcStream, remoteFile, append);
            }
            else
            {
                PutBinary(srcStream, remoteFile, append);
            }
            ValidateTransfer();
        }

        private void ValidateTransfer()
        {
            var validCodes = new[] { "226", "250" };
            var reply = Control.ReadReply();
            LastValidReply = Control.ValidateReply(reply, validCodes);
        }

        private NetworkStream GetDataStream()
        {
            var sock = Data;
            if (ConnectMode == FtpConnectMode.Active)
                sock = Data.Accept();
            return new NetworkStream(sock, true);
        }

        private void InitPut(string remoteFile, bool append)
        {
            Data = Control.CreateDataSocket(ConnectMode);
            var cmd = append ? "APPE" : "STOR";
            SendCommandAndValidateCode(remoteFile, cmd, "125", "150");
        }

        [ExcludeFromCodeCoverage]
        private void PutAscii(string localPath, string remoteFile, bool append)
        {
            Stream srcStream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
            PutAscii(srcStream, remoteFile, append);
        }

        [ExcludeFromCodeCoverage]
        private void PutAscii(Stream srcStream, string remoteFile, bool append)
        {
            InitPut(remoteFile, append);
            var reader = new StreamReader(srcStream);
            var writer = new StreamWriter(GetDataStream());
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                writer.Write(line, 0, line.Length);
                writer.Write(FtpControlSocket.EOL);
            }
            reader.Close();
            writer.Flush();
            writer.Close();
        }

        [ExcludeFromCodeCoverage]
        private void PutBinary(string localPath, string remoteFile, bool append)
        {
            Stream srcStream =
                new FileStream(localPath, FileMode.Open, FileAccess.Read);
            PutBinary(srcStream, remoteFile, append);
        }

        [ExcludeFromCodeCoverage]
        private void PutBinary(Stream srcStream, string remoteFile, bool append)
        {
            var reader = new BufferedStream(srcStream);
            InitPut(remoteFile, append);
            var writer = new BinaryWriter(GetDataStream());
            var buf = new byte[512];
            int count;
            while ((count = reader.Read(buf, 0, buf.Length)) > 0)
                writer.Write(buf, 0, count);
            reader.Close();
            writer.Flush();
            writer.Close();
        }

        public void Put(byte[] bytes, string remoteFile)
        {
            Put(bytes, remoteFile, false);
        }

        public void Put(byte[] bytes, string remoteFile, bool append)
        {
            InitPut(remoteFile, append);
            var writer = new BinaryWriter(GetDataStream());
            writer.Write(bytes, 0, bytes.Length);
            writer.Flush();
            writer.Close();
            ValidateTransfer();
        }

        [ExcludeFromCodeCoverage]
        public void Get(string localPath, string remoteFile)
        {
            if (TransferType == FtpTransferType.Ascii)
                GetAscii(localPath, remoteFile);
            else
                GetBinary(localPath, remoteFile);
            ValidateTransfer();
        }

        [ExcludeFromCodeCoverage]
        public void Get(Stream destStream, string remoteFile)
        {
            if (TransferType == FtpTransferType.Ascii)
                GetAscii(destStream, remoteFile);
            else
                GetBinary(destStream, remoteFile);
            ValidateTransfer();
        }

        private void InitGet(string remoteFile)
        {
            Data = Control.CreateDataSocket(ConnectMode);
            SendCommandAndValidateCode(remoteFile, "RETR", "125", "150");
        }

        [ExcludeFromCodeCoverage]
        private void GetAscii(string localPath, string remoteFile)
        {
            InitGet(remoteFile);
            var localFile = new FileInfo(localPath);
            var writer = new StreamWriter(localPath);
            var reader = new StreamReader(GetDataStream());
            IOException storedEx = null;
            try
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    writer.Write(line, 0, line.Length);
                    writer.WriteLine();
                }
            }
            catch (IOException ex)
            {
                storedEx = ex;
            }
            finally
            {
                writer.Close();
                if (storedEx != null && File.Exists(localFile.FullName))
                    File.Delete(localFile.FullName);
            }
            try
            {
                reader.Close();
            }
            catch (IOException) { }
            if (storedEx != null)
                throw storedEx;
        }

        [ExcludeFromCodeCoverage]
        private void GetAscii(Stream destStream, string remoteFile)
        {
            InitGet(remoteFile);
            var writer = new StreamWriter(destStream);
            var reader = new StreamReader(GetDataStream());
            IOException storedEx = null;
            try
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    writer.Write(line, 0, line.Length);
                    writer.WriteLine();
                }
            }
            catch (IOException ex)
            {
                storedEx = ex;
            }
            finally
            {
                writer.Close();
            }
            try
            {
                reader.Close();
            }
            catch (IOException)
            {
            }
            if (storedEx != null)
                throw storedEx;
        }

        [ExcludeFromCodeCoverage]
        private void GetBinary(string localPath, string remoteFile)
        {
            InitGet(remoteFile);
            var localFile = new FileInfo(localPath);
            var writer = new BinaryWriter(new FileStream(localPath, FileMode.OpenOrCreate));
            var reader = new BinaryReader(GetDataStream());
            const int chunksize = 4096;
            var chunk = new byte[chunksize];
            IOException storedEx = null;
            try
            {
                int count;
                while ((count = reader.Read(chunk, 0, chunk.Length)) > 0)
                    writer.Write(chunk, 0, count);
            }
            catch (IOException ex)
            {
                storedEx = ex;
            }
            finally
            {
                writer.Close();
                if (storedEx != null && File.Exists(localFile.FullName))
                    File.Delete(localFile.FullName);
            }
            try
            {
                reader.Close();
            }
            catch (IOException)
            {
            }
            if (storedEx != null)
                throw storedEx;
        }

        [ExcludeFromCodeCoverage]
        private void GetBinary(Stream destStream, string remoteFile)
        {
            InitGet(remoteFile);
            var writer = new BinaryWriter(destStream);
            var reader = new BinaryReader(GetDataStream());
            const int chunksize = 4096;
            var chunk = new byte[chunksize];
            IOException storedEx = null;
            try
            {
                int count;
                while ((count = reader.Read(chunk, 0, chunk.Length)) > 0)
                    writer.Write(chunk, 0, count);
            }
            catch (IOException ex)
            {
                storedEx = ex;
            }
            finally
            {
                writer.Close();
            }
            try
            {
                reader.Close();
            }
            catch (IOException)
            {
            }
            if (storedEx != null)
                throw storedEx;
        }

        public byte[] Get(string remoteFile)
        {
            InitGet(remoteFile);
            var reader = new BinaryReader(GetDataStream());
            const int chunksize = 4096;
            var chunk = new byte[chunksize];
            var temp = new MemoryStream(chunksize);
            int count;
            while ((count = reader.Read(chunk, 0, chunk.Length)) > 0)
                temp.Write(chunk, 0, count);
            temp.Close();
            try
            {
                reader.Close();
            }
            catch (IOException) { }
            ValidateTransfer();
            return temp.ToArray();
        }

        [ExcludeFromCodeCoverage]
        public bool Site(string command)
        {
            var reply = Control.SendCommand("SITE " + command);
            var validCodes = new[] { "200", "202", "502" };
            LastValidReply = Control.ValidateReply(reply, validCodes);
            if (reply.Substring(0, (3) - (0)).Equals("200"))
                return true;
            return false;
        }

        public string[] Dir()
        {
            return Dir(null, false);
        }

        public string[] Dir(string dirname)
        {
            return Dir(dirname, false);
        }

        public string[] Dir(string dirname, bool full)
        {
            Data = Control.CreateDataSocket(ConnectMode);
            var command = full ? "LIST" : "NLST";
            SendCommandAndValidateCode(dirname, command, "125", "150", "550");
            var result = new string[0];
            if (!LastValidReply.ReplyCode.Equals("550"))
            {
                var reader = new StreamReader(GetDataStream());
                var lines = new ArrayList();
                string line;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line);
                try
                {
                    reader.Close();
                }
                catch (IOException) { }
                Validate("226", "250");
                if (lines.Count > 0)
                    result = (string[])lines.ToArray(typeof(string));
            }
            return result;
        }

        private void Validate(params string[] validCodes2)
        {
            var reply = Control.ReadReply();
            LastValidReply = Control.ValidateReply(reply, validCodes2);
        }

        [ExcludeFromCodeCoverage]
        public void DebugResponses(bool on)
        {
            Control.DebugResponses(on);
        }

        public void Delete(string remoteFile)
        {
            SendCommandAndValidateCode(remoteFile, Dele, "250");
        }

        public void Rename(string from, string to)
        {
            SendCommandAndValidateCode(from, Rnfr, "350");
            SendCommandAndValidateCode(to, Rnto, "250");
        }

        private void SendCommandAndValidateCode(string from, string cmd, params string[] expectedReplyCode)
        {
            var reply = Control.SendCommand(cmd + " " + from);
            LastValidReply = Control.ValidateReply(reply, expectedReplyCode);
        }

        [ExcludeFromCodeCoverage]
        public void Rmdir(string dir)
        {
            SendCommandAndValidateCode(dir, Rmd, "250", "257");
        }

        public void Mkdir(string dir)
        {
            SendCommandAndValidateCode(dir, Mkd, "257");
        }

        [ExcludeFromCodeCoverage]
        public void Chdir(string dir)
        {
            SendCommandAndValidateCode(dir, Cdw, "250");
        }

        [ExcludeFromCodeCoverage]
        public DateTime ModTime(string remoteFile)
        {
            SendCommandAndValidateCode(remoteFile, Mdtm, "213");
            return DateTime.ParseExact(LastValidReply.ReplyText, DtFormat, null);
        }

        [ExcludeFromCodeCoverage]
        public string Pwd()
        {
            var reply = Control.SendCommand(PwdCommand);
            LastValidReply = Control.ValidateReply(reply, "257");
            var text = LastValidReply.ReplyText;
            var start = text.IndexOf('"');
            var end = text.LastIndexOf('"');
            if (start >= 0 && end > start)
                return text.Substring(start + 1, (end) - (start + 1));
            return text;
        }

        public string System()
        {
            var reply = Control.SendCommand(Syst);
            LastValidReply = Control.ValidateReply(reply, "215");
            return LastValidReply.ReplyText;
        }

        public string Help(string command)
        {
            var reply = Control.SendCommand(HelpCommand + command);
            var validCodes = new[] { "211", "214" };
            LastValidReply = Control.ValidateReply(reply, validCodes);
            return LastValidReply.ReplyText;
        }

        public void Quit()
        {
            try
            {
                var reply = Control.SendCommand(QuitCommand);
                var validCodes = new[] { "221", "226" };
                LastValidReply = Control.ValidateReply(reply, validCodes);
            }
            finally
            {
                Control.Logout();
                Control = null;
            }
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Disposing = true;
            Dispose(Disposing);

        }

        [ExcludeFromCodeCoverage]
        private void Dispose(bool disposing)
        {
            if (disposing)
                Quit();
        }
    }
}