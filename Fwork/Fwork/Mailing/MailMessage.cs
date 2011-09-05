using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;

namespace Fwork.Mailing {
	
	public struct MailVariables {
		public static string SMTPHost = "";
		public static string SMTPUser = "";
		public static string SMTPPass = "";
	}
	
	public class MailMessage: System.Net.Mail.MailMessage {
		
		private string smtp_host, smtp_user, smtp_pass;
		private bool smtp_usedefault = true;
		
		public MailMessage(): base() { }

		public MailMessage(MailAddress from, MailAddress to) : base(from, to) { }

		public MailMessage(string from, string to) : base(from, to) { }

		public MailMessage(string from, string to, string subject, string body) : base(from, to, subject, body) { }
		
		public void SetCredentials(string smtp_host, string smtp_user, string smtp_pass) {
			this.smtp_host = smtp_host;
			this.smtp_user = smtp_user;
			this.smtp_pass = smtp_pass;
			this.smtp_usedefault = false;
		}
		
		private void setCredentials() {
			if(this.smtp_usedefault) {
				this.smtp_host = MailVariables.SMTPHost;
				this.smtp_user = MailVariables.SMTPUser;
				this.smtp_pass = MailVariables.SMTPPass;
				if(this.smtp_host.Trim()=="" || this.smtp_user.Trim()=="" || this.smtp_pass.Trim()=="") {
					throw new Exception("SMTP Credentials not set.");
				}
			}
		}
		
		public void Send() {
			this.setCredentials();
			SmtpClient SC;
			try {
				SC = new SmtpClient(this.smtp_host);
				NetworkCredential SMTPUserInfo = new NetworkCredential(this.smtp_user, this.smtp_pass);
				SC.UseDefaultCredentials = false;
				SC.Credentials = SMTPUserInfo;
				this.IsBodyHtml = true;
				SC.Send(this);
			} finally {
				SC = null;
			}
		}
		
	}
	
}
