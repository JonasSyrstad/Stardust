//
// FTPControlSocket.cs
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Stardust.Particles.FileTransfer
{
    public sealed class FtpControlSocket
    {
        private int _timeout = -1;
        internal int Timeout
        {
            set
            {
                _timeout = value;
                if (ControlSock == null)
                    throw new SystemException("Failed to set timeout - no control socket");
                SetSocketTimeout(ControlSock, _timeout);
            }
            get { return _timeout; }
        }

        internal StreamWriter LogStream
        {
            set
            {
                if (value != null)
                    Log = value;
            }
        }

        internal const string EOL = "\r\n";
        internal const int ControlPort = 21;
        private bool debugResponses;
        private TextWriter Log = Console.Out;
        private Socket ControlSock;
        private StreamWriter Writer;
        private StreamReader Reader;

        public FtpControlSocket(string remoteHost, int controlPort, StreamWriter log, int timeout)
        {
            var remoteHostEntry = Dns.Resolve(remoteHost);
            var ipAddresses = remoteHostEntry.AddressList;
            Initialize(ipAddresses[0], controlPort, log, timeout);
        }

        public FtpControlSocket(IPAddress remoteAddr, int controlPort, StreamWriter log, int timeout)
        {
            Initialize(remoteAddr, controlPort, log, timeout);
        }

        internal void Initialize(IPAddress remoteAddr, int controlPort, StreamWriter log, int timeout)
        {
            LogStream = log;
            DebugResponses(true);
            var ipe = new IPEndPoint(remoteAddr, controlPort);
            ControlSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Timeout = timeout;
            ControlSock.Connect(ipe);
            InitStreams();
            ValidateConnection();
            DebugResponses(false);
        }

        private void ValidateConnection()
        {
            var reply = ReadReply();
            ValidateReply(reply, "220");
        }

        private void InitStreams()
        {
            var stream = new NetworkStream(ControlSock, true);
            Writer = new StreamWriter(stream);
            Reader = new StreamReader(stream);
        }

        public void Logout()
        {
            Log.Flush();
            Log = null;
            Writer.Close();
            Reader.Close();
        }
        internal Socket CreateDataSocket(FtpConnectMode connectMode)
        {
            if (connectMode == FtpConnectMode.Active)
                return CreateDataSocketActive();
            return CreateDataSocketPasv();
        }

        private Socket CreateDataSocketActive()
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var localHostEntry = Dns.Resolve(Dns.GetHostName());
            var localEndPoint = new IPEndPoint(localHostEntry.AddressList[0], 0);
            sock.Bind(localEndPoint);
            sock.Listen(5);
            SetDataPort((IPEndPoint)sock.LocalEndPoint);
            return sock;
        }

        private void SetDataPort(IPEndPoint ep)
        {
            var hostBytes = BitConverter.GetBytes(ep.Address.Address);
            var portBytes = ToByteArray((ushort)ep.Port);
            var cmd = new StringBuilder("PORT ").
                Append((short)hostBytes[0]).Append(",").
                Append((short)hostBytes[1]).Append(",").
                Append((short)hostBytes[2]).Append(",").
                Append((short)hostBytes[3]).Append(",").
                Append((short)portBytes[0]).Append(",").
                Append((short)portBytes[1]).ToString();
            var reply = SendCommand(cmd);
            ValidateReply(reply, "200");
        }

        private static byte[] ToByteArray(ushort val)
        {
            var bytes = new byte[2];
            bytes[0] = (byte)(val >> 8); // bits 1- 8
            bytes[1] = (byte)(val & 0x00FF); // bits 9-16
            return bytes;
        }

        private Socket CreateDataSocketPasv()
        {
            var reply = SendCommand("PASV");
            ValidateReply(reply, "227");
            var startIp = reply.IndexOf('(');
            var endIp = reply.IndexOf(')');
            if (startIp < 0 && endIp < 0)
            {
                startIp = reply.ToUpper().LastIndexOf("MODE", StringComparison.Ordinal) + 4;
                endIp = reply.Length;
            }
            var ipData = reply.Substring(startIp + 1, (endIp) - (startIp + 1));
            var parts = new int[6];
            var len = ipData.Length;
            var partCount = 0;
            var buf = new StringBuilder();
            for (var i = 0; i < len && partCount <= 6; i++)
            {
                var ch = ipData[i];
                if (Char.IsDigit(ch))
                    buf.Append(ch);
                else if (ch != ',')
                    throw new FtpException("Malformed PASV reply: " + reply);
                if (ch == ',' || i + 1 == len)
                {
                    try
                    {
                        parts[partCount++] = Int32.Parse(buf.ToString());
                        buf.Length = 0;
                    }
                    catch (FormatException)
                    {
                        throw new FtpException("Malformed PASV reply: " + reply);
                    }
                }
            }
            var ipAddress = parts[0] + "." + parts[1] + "." + parts[2] + "." + parts[3];
            var port = (parts[4] << 8) + parts[5];
            return CreateSocket(ipAddress, port);
        }

        private Socket CreateSocket(string ipAddress, int port)
        {
            var remoteHostEntry = Dns.Resolve(ipAddress);
            var ipe = new IPEndPoint(remoteHostEntry.AddressList[0], port);
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetSocketTimeout(sock, _timeout);
            sock.Connect(ipe);
            return sock;
        }

        internal string SendCommand(string command)
        {
            if (debugResponses)
                Log.WriteLine("---> " + command);
            Writer.Write(command + EOL);
            Writer.Flush();
            return ReadReply();
        }

        internal string ReadReply()
        {
            var firstLine = Reader.ReadLine();
            if (firstLine.IsNullOrEmpty())
                throw new IOException("Unexpected null reply received");
            var reply = new StringBuilder(firstLine);
            if (debugResponses)
                Log.WriteLine(reply.ToString());
            var replyCode = reply.ToString().Substring(0, 3);
            if (reply[3] == '-')
            {
                var complete = false;
                while (!complete)
                {
                    var line = Reader.ReadLine();
                    if (line == null)
                        throw new IOException("Unexpected null reply received");
                    if (debugResponses)
                        Log.WriteLine(line);
                    if (line.Length > 3 && line.Substring(0, 3).Equals(replyCode) && line[3] == ' ')
                    {
                        reply.Append(line.Substring(3));
                        complete = true;
                    }
                    else
                    {
                        reply.Append(" ");
                        reply.Append(line);
                    }
                }
            }
            return reply.ToString();
        }

        internal FtpReply ValidateReply(string reply, string expectedReplyCode)
        {
            var replyCode = reply.Substring(0, 3);
            var replyText = reply.Substring(4);
            var replyObj = new FtpReply(replyCode, replyText);
            if (replyCode.Equals(expectedReplyCode))
                return replyObj;
            throw new FtpException(replyText, replyCode);
        }

        internal FtpReply ValidateReply(string reply, string[] expectedReplyCodes)
        {
            var replyCode = reply.Substring(0, 3);
            var replyText = reply.Substring(4);
            var replyObj = new FtpReply(replyCode, replyText);
            if (expectedReplyCodes.Any(replyCode.Equals))
                return replyObj;
            throw new FtpException(replyText, replyCode);
        }

        internal void DebugResponses(bool on)
        {
            debugResponses = on;
        }

        private void SetSocketTimeout(Socket sock, int timeout)
        {
            if (timeout <= 0) return;
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
        }
    }
}