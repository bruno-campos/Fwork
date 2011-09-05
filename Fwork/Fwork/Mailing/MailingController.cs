using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace Fwork.Mailing {

	public class MailingController {

		private static MailingController instance = new MailingController();
		
		public static MailingController Instance {
			get { return instance; }
		}
		
		private MailingController() { }
		
		public void A() {
			
		}
		
	}
	
}
