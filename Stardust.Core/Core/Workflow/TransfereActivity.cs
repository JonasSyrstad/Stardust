//
// TransfereActivity.cs
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

using System.Activities;
using Stardust.Clusters;
using Stardust.Particles;
using Stardust.Particles.FileTransfer;

namespace Stardust.Core.Workflow
{
    public class TransfereActivity : CodeActivity
    {
        #region Arguments
        public InArgument<TransferType> TransferType { get; set; }

        public InArgument<string> TransferMethod { get; set; }

        public InArgument<string> Location { get; set; }

        public InArgument<string> FileName { get; set; }

        public InArgument<string> UserName { get; set; }

        public InArgument<string> Password { get; set; }

        public InOutArgument<byte[]> File { get; set; }
        #endregion

        protected override void Execute(CodeActivityContext context)
        {
            var transporter = CreateTransporter(context);
            if (TransferType.Get(context) == Workflow.TransferType.Read)
                File.Set(context, transporter.Read());
            else
                transporter.Transfer(File.Get(context));
        }

        private IFileTransfer CreateTransporter(ActivityContext context)
        {
            var transporter = CreateFileTransferInstance(context);
            return SetCredentials(context, transporter)
                .SetServerRootUrl(Location.Get(context))
                .SetFileName(FileName.Get(context));
        }

        private IFileTransfer CreateFileTransferInstance(ActivityContext context)
        {
            if (TransferMethod.Get<string>(context).ContainsCharacters())
                return TransferFactory.Create(TransferMethod.Get<string>(context));
            return TransferFactory.Create(TransferMethods.Http);
        }

        private IFileTransfer SetCredentials(ActivityContext context, IFileTransfer transporter)
        {
            if (UserName.Get(context).ContainsCharacters() && Password.Get(context).ContainsCharacters())
                transporter.SetUserNameAndPassword(UserName.Get(context), Password.Get(context));
            return transporter;
        }
    }
}