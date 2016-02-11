//
// EmailHelper.cs
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
using System.Net;
using System.Net.Mail;

namespace Stardust.Particles
{
    public static class EmailHelper
    {
        public static MailMessage CreateMessage(string subject, string body, string from, string to = null)
        {
            var message = new MailMessage { Subject = subject, From = new MailAddress(@from), Body = body };
            if (to.ContainsCharacters())
                message.To.Add(to);
            return message;
        }

        public static bool TrySendMessage(this MailMessage message, string smtpAddress)
        {
            try
            {
                message.SendMessageManaged(smtpAddress);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This method disposes the message after it has been called, sucess or not.
        /// </summary>
        public static void SendMessageManaged(this MailMessage message, string smtpAddress, string username = null, string password = null)
        {
            using (message)
            {
                message.SendMessage(smtpAddress,username,password);
            }
        }

        /// <summary>
        /// Sends the message. Remember to dispose the message after sending.
        /// If you want to use a fluent syntax use SendMessageManaged.
        /// </summary>
        public static void SendMessage(this MailMessage message, string smtpAddress, string username = null, string password = null)
        {
            try
            {
                message.ValidateSendAttributes(smtpAddress);
                using (var smtp = new SmtpClient(smtpAddress))
                {
                    if (username.ContainsCharacters())
                    {
                        smtp.Credentials = new NetworkCredential(username, password);
                        smtp.EnableSsl = true;
                        smtp.Port = 587;
                    }

                    smtp.Send(message);
                }
            }
            catch (StardustCoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void ValidateSendAttributes(this MailMessage message, string smtpAddress)
        {
            if (smtpAddress.IsNullOrWhiteSpace()) throw new StardustCoreException("Smtp address is not set");
            if (message.IsNull()) throw new StardustCoreException("Mail message is null");
            if (message.To.Count == 0) throw new StardustCoreException("Mail message has no recipients");
        }

        private static void HandleException(Exception ex)
        {
            Logging.Exception(ex, "Stardust.Particles.EmailHelper");
            throw new StardustCoreException("Sending message failed, see inner exception for details", ex);
        }
    }
}
