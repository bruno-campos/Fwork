using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Fwork.Http {
	
	public class ServiceXml {
	
		public static string RootNode = "fwork";
	
		private ServiceXml() {
		}
		
		public static string Response(string xml) {
			return "<" + RootNode + "><response>" + xml + "</response>" + infoNode() + "<error></error></" + RootNode + ">";
		}
		
		public static string Response(Exception e) {
			return "<" + RootNode + "><response></response>" + infoNode() + "<error><message>" + e.Message + "</message><stacktrace>" + e.StackTrace + "</stacktrace></error></" + RootNode + ">";
		}
		
		public static string Response(string xml, Exception e) {
			return "<" + RootNode + "><response>" + xml + "</response>" + infoNode() + "<error><message>" + e.Message + "</message><stacktrace>" + e.StackTrace + "</stacktrace></error></" + RootNode + ">";
		}
		
		public static string Done() {
			return Response("<done>1</done>");
		}
		
		public static string Done(string xml) {
			return Response("<done>1</done>" + xml);
		}

		public static string Error(Exception e) {
			return "<" + RootNode + "><response></response>" + getErrorMessage(e) + infoNode() + "</" + RootNode + ">";
		}
		
		private static string getErrorMessage(Exception e) {
			string xml = "";
			xml += "<error>";
				xml += "<type>" + e.GetType().ToString() + "</type>";
				xml += "<message>"+e.Message+"</message>";
				xml += "<stacktrace><![CDATA[" + e.StackTrace + "]]></stacktrace>";
				if(e.InnerException!=null) {
					xml += "<innerEx>";
						xml += "<type>" + e.InnerException.GetType().ToString() + "</type>";
						xml += "<message>" + e.InnerException.Message + "</message>";
						xml += "<stacktrace><![CDATA[" + e.InnerException.StackTrace + "]]></stacktrace>";
					xml += "</innerEx>";
				}
			xml += "</error>";
			return xml;
		}

		public static string GetInnerXml(string xml) {
			return xml.Substring(("<" + RootNode + "><response>").ToString().Length, xml.IndexOf("</response>") - ("<" + RootNode + "><response>").ToString().Length);
		}
		
		private static string infoNode() {
			string xml = "";
			 xml = "<info></info>";
			return xml;
		}
		
	}
	
}
