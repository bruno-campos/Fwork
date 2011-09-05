using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net.Mail;
using System.Data.SqlClient;
using Fwork.Database;
using System.Web;

namespace Smart.Mailing {
	
	public class MailingController_bkp {
		/*
		public string a = "", b = "", c = "";
		public int d = 0, e = 0, f = 0;

		public Dictionary<string, Config> Configs = new Dictionary<string, Config>();
		
		private static readonly MailingController instance = new MailingController();
		
		private List<string> campanhas = new List<string>();
		private List<MailAlerta> alertas = new List<MailAlerta>();
		private List<string> sending = new List<string>();
		private bool running = false;
		
		public static MailingController Instance {
			get{ return instance; }
		}
		
		private MailingController() {
		}
		
		public int OptOut(string grp_root, string opt_mail, string cam_codigo) {
			DbCmd cmd = new DbCmd("_mailing_optout_save");
			try {
				cmd.AddParameter("@grp_root", grp_root);
				cmd.AddParameter("@opt_mail", opt_mail);
				cmd.AddParameter("@cam_codigo", cam_codigo);
				return cmd.ExecuteNonQuery();
			} finally {
				cmd.Dispose();
				cmd = null;
			}
		}
		public int Read(string cam_codigo, string usu_email) {
			DbCmd cmd = new DbCmd("_mailing_campanha_read");
			try {
				cmd.AddParameter("@cam_codigo", cam_codigo);
				cmd.AddParameter("@rea_email", usu_email);
				return cmd.ExecuteNonQuery();
			} finally {
				cmd.Dispose();
				cmd = null;
			}
		}
		
		public void AddAlerta(MailAlerta M) {
			this.alertas.Add(M);
			this.checkQueue();
		}
		
		public void AddCampanha(string cam_codigo) {
			this.campanhas.Add(cam_codigo);
			this.checkQueue();
		}
		public void PauseCampanha(string cam_codigo) {
			Campanha C = new Campanha(cam_codigo);
			C.SetStatus(CampanhaStatus.Pausado);
			this.sending.Remove(cam_codigo);
		}
		
		public void Start() {
			this.running = true;
			this.checkQueue();
		}
		public void Pause() {
			this.running = false;
		}
		
		private void checkMissing() {
		
		}
		
		private void checkQueue() {
			if(this.campanhas.Count > 0 || this.alertas.Count > 0) {
				if(this.alertas.Count > 0) {
					//Envio de alertas
					MailAlerta MA = this.alertas[0];
					this.alertas.RemoveAt(0);
					try {
						Thread T = new Thread(delegate() { this.sendAlerta(MA); });
						T.Start();
					} finally {
						//MA = null;
					}
				} else {
					//Envio de campanhas
					string cam_codigo = this.campanhas[0];
					this.campanhas.RemoveAt(0);
					this.sending.Add(cam_codigo);
					Thread T = new Thread(delegate() { this.sendCampanha(cam_codigo); });
					T.Start();
				}
				this.checkQueue();
			}
		}
		
		private void sendAlerta(MailAlerta MA) {
			this.a += "MA " + MA.Mail + "<br />";
			Mail M = new Mail(MA.Mail);
			try {
                M.Load();
                MA.FillToSend();
                //Carregando config de smtp
                ArrayList smtp_cred = this.getSMTPCred(M.Root);
                int si = 0;
                //From
                MailAddress from = new MailAddress(MailingController.Instance.Configs[M.Root]["mailing_from_alert_mail"], MailingController.Instance.Configs[M.Root]["mailing_from_alert_name"]);
                foreach(MailAlertaToSend MTS in MA.ToSend) {
                    this.a += MTS.usu_email + " <br />";
                    this.sendMailAlerta(M, from, MTS, (string[])smtp_cred[si]);
                    si = (si >= smtp_cred.Count - 1) ? 0 : si + 1;
                }
			} finally {
				M = null;
                //MA = null;
			}
		}
		
		private void endCampanha(string cam_codigo) {
			new LogFile(LogFiles.Mailing).Save(new LogFileEntry("[ENVIADO][" + cam_codigo + "]: Campanha finalizada as " + DateTime.Now.ToString("dd/MM/yyyy HH:mm")));
			this.sending.Remove(cam_codigo);
		}
		
		private void sendCampanha(string cam_codigo) {
			Campanha C = new Campanha(cam_codigo);
			try {
				C.Load();
				string file_sent = C.FileToSend;
				XmlDocument doc = new XmlDocument();
				Mail M = new Mail(C.Mail);
				try {
					//Carregando email
					M.Load();
					//Carregando optout
					List<string> optout = this.getOptOut(M.Root);
					//Carregando config de smtp
					ArrayList smtp_cred = this.getSMTPCred(M.Root);
					//Carregando arquivo XML de destinatarios
					doc.Load(file_sent);
					XmlElement root = doc.DocumentElement;
					XmlNodeList nodeList;
					nodeList = root.SelectSingleNode("usuarios").ChildNodes;
					C.SetStatus(CampanhaStatus.Enviando);
					int i = 0, si = 0;
					while ((nodeList.Count > 0) && (this.sending.Contains(cam_codigo))) {
						//Enviando email
						string to_mail = nodeList[0].SelectSingleNode("email").InnerText;
						string to_nome = nodeList[0].SelectSingleNode("nome").InnerText;
						//Verificando optout antes do envio
						if(!optout.Contains(to_mail)) {
							this.sendMail(M, C.Codigo, C.From, C.ReplyTo, to_mail, to_nome, (string[])smtp_cred[si]);
						}
						//Removendo nodo e salvando XML a cada 10 enviados
						root.SelectSingleNode("usuarios").RemoveChild(nodeList[0]);
						i++;
						//if(i == 10) {
							doc.Save(file_sent);
						//	i = 0;
						//}
						C.IncrementSent();
						//Alterando credenciais de SMTP
						si = (si>=smtp_cred.Count-1) ? 0 : si + 1;
						Thread.Sleep(100);
					}
					if(this.sending.Contains(cam_codigo)) {
						C.SetStatus(CampanhaStatus.Finalizado);
						this.endCampanha(cam_codigo);
					}
				} finally {
					doc = null;
					M = null;
				}
			} catch(Exception ex) {
				new LogFile(LogFiles.Mailing).Save(new LogFileEntry("[ERROR][Controller][sendCampanha]: " + ex.Message));
			} finally {
				C = null;
			}
		}
		
		private void sendMail(Mail M, string cam_codigo, string cam_from, string cam_replyto, string to_mail, string to_nome, string[] smtp_cred) {
			try {
				string html = this.getHtml(M.Root, cam_codigo, to_mail, M.Html, M.Codigo);
				MailMessage MM = new MailMessage(new MailAddress(cam_from), new MailAddress(to_mail, to_nome), cam_replyto, M.Subject, html);
				try {
					MM.Send();
					MM.SetCredentials(smtp_cred[0], smtp_cred[1], smtp_cred[2]);
				} finally {
					MM.Dispose();
					MM = null;
				}
			} catch(Exception ex) {
				new LogFile(LogFiles.Mailing).Save(new LogFileEntry("[ERROR][Controller][sendMail]: " + ex.Message));
			}
		}
		
		private void sendMailAlerta(Mail M, MailAddress from, MailAlertaToSend MTS, string[] smtp_cred) {
			try {
				string html = "<html><head></head><body>%body</body></html>".Replace("%body", M.Html);
				html = html.Replace("%http", this.Configs[M.Root]["http"]);
                string subject = MailAlerta.GetHtml(M.Subject, MTS);
                html = MailAlerta.GetHtml(html, MTS);
				MailMessage MM = new MailMessage(from, new MailAddress(MTS.usu_email, MTS.usu_nome), subject, html);
				try {
					MM.Send();
					MM.SetCredentials(smtp_cred[0], smtp_cred[1], smtp_cred[2]);
				} finally {
					MM.Dispose();
					MM = null;
				}
			} catch(Exception ex) {
				new LogFile(LogFiles.Mailing).Save(new LogFileEntry("[ERROR][Controller][sendMailAlerta]: " + ex.Message));
			}
		}
		
		private string getHtml(string grp_root, string cam_codigo, string mail_to, string mai_html, string mai_codigo) {
			string html = "<html><head></head><body>%netvmsg%body%optmsg%sig</body></html>";
			html = html.Replace("%body", mai_html);
			html = html.Replace("%sig", "<img src=\"http://www.plugminas.mg.gov.br/services/mailing/image.aspx?root=%root&cam=%cam&mail=%email\" />");
			html = html.Replace("%root", grp_root);
			html = html.Replace("%cam", cam_codigo);
			html = html.Replace("%email", mail_to);
			html = html.Replace("src=\"/custom/", "src=\"" + this.Configs[grp_root]["http"] + "/custom/");
			html = html.Replace("src=\"custom/", "src=\"" + this.Configs[grp_root]["http"] + "custom/");
			string link_netvmsg = this.Configs[grp_root]["http"] + "/services/mailing/mail.aspx?mail=" + mai_codigo + "&to=" + mail_to;
			string link_optmsg = this.Configs[grp_root]["http"] + "/services/mailing/optout.aspx?email=" + mail_to + "&cam=" + cam_codigo + "&root=" + grp_root;
			html = html.Replace("%netvmsg", "<a href=\"" + link_netvmsg + "\">Caso não esteja visualizando este e-mail, acesse aqui a versão online.</a>");
			html = html.Replace("%optmsg", "<a href=\"" + link_optmsg + "\">Caso não queira mais receber nossos emails, clique aqui.</a>");
			this.a = html;
			return html;
		}
		
		private List<string> getOptOut(string grp_root) {
			List<string> ret = new List<string>();
			DbCmd cmd = new DbCmd("_mailing_optout_getallby_root");
			SqlDataReader dr;
			try {
				cmd.AddParameter("@grp_root", grp_root);
				dr = cmd.ExecuteReader();
				if(dr.HasRows) {
					while(dr.Read()) {
						ret.Add(DbValue.GetValue(dr["opt_mail"]));
					}
				}
				dr.Dispose();
				return ret;
			} finally {
				cmd.Dispose();
				cmd = null;
			}
		}
		
		public ArrayList getSMTPCred(string grp_root) {
			ArrayList ret = new ArrayList();
			DbCmd cmd = new DbCmd("_mailing_smtp_getallby_root");
			SqlDataReader dr;
			try {
				cmd.AddParameter("@grp_root", grp_root);
				dr = cmd.ExecuteReader();
				if(dr.HasRows) {
					while(dr.Read()) {
						string smt_address = DbValue.GetValue(dr["smt_address"]);
						string smt_username = DbValue.GetValue(dr["smt_username"]);
						string smt_password = DbValue.GetValue(dr["smt_password"]);
						ret.Add(new string[] {smt_address, smt_username, smt_password});
					}
				}
				dr.Dispose();
				return ret;
			} finally {
				cmd.Dispose();
				cmd = null;
			}
		}
		*/
	}
	
}
