//
// HttpFileTrasfer.cs
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

using System.Net;

namespace Stardust.Particles.FileTransfer
{
    public class HttpFileTrasfer : DynamicConfigurableObjectBase, IFileTransfer
    {
        private string Password;
        private string UserName;

        public HttpFileTrasfer()
            : base(null)
        {
        }

        public IFileTransfer SetServerRootUrl(string url)
        {
            Url = url;
            return this;
        }

        public IFileTransfer SetPort(string port)
        {
            return this;
        }

        public IFileTransfer SetTimeout(decimal ttl)
        {
            return this;
        }

        public bool TryTransfer(byte[] data)
        {
            try
            {
                Transfer(data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Transfer(byte[] data)
        {
            using (var client = CreateClient())
            {
                client.UploadData(Url, data);
            }
        }

        private WebClient CreateClient()
        {
            var client = new WebClient();
            if (UserName.ContainsCharacters() && Password.ContainsCharacters())
                GetPassword(client);
            return client;
        }

        private void GetPassword(WebClient client)
        {
            if (UserName.Contains("\\"))
            {
                var domain = UserName.Split('\\')[0];
                var user = UserName.Split('\\')[1];
                client.Credentials = new NetworkCredential(user, Password, domain);
            }
            else
                client.Credentials = new NetworkCredential(UserName, Password);
        }

        private string Url1;
        public string Url
        {
            get { return Url1 + FileName; }
            private set { Url1 = value; }
        }


        public byte[] Read()
        {
            using (var client = CreateClient())
            {
                return client.DownloadData(Url);
            }
        }

        public IFileTransfer SetUserNameAndPassword(string userName, string password)
        {
            UserName = userName;
            Password = password;
            return this;
        }

        public IFileTransfer SetFileName(string fileName)
        {
            FileName = fileName;
            return this;
        }

        public string FileName { get; private set; }
    }
}