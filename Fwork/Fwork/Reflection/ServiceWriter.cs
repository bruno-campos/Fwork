using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Configuration;

namespace Fwork.Reflection {
	
	public struct ServiceWriterVars {
		public static string ValidateRequest = "false";
		public static string XmlResponseEncoding = "iso-8859-1";
		public static string HttpNamespace = "Fwork.Http";
	}
	
	public class ServiceWriter {
		
		private string[] namespaces, assemblies;
		
		public ServiceWriter() {
		}
		public ServiceWriter(string[] namespaces, string[] assemblies) {
			this.namespaces = namespaces;
			this.assemblies = assemblies;
		}
		
		public void CreateFiles() {
			foreach(string ass_name in this.assemblies) {
				Assembly asm = Assembly.Load(new AssemblyName(ass_name));
				try {
					foreach(Type type in asm.GetTypes()) {
						if(this.namespaces.Contains(type.Namespace)) {
							this.WriteService(type);
						}
					}
				} finally {
					asm = null;
				}
			}
		}
		
		public void WriteService(Type type) {
			string service_filename = type.FullName.Replace('.', '_').ToLower() + ".aspx";
			string service_path = Folders.Check(ConfigurationSettings.AppSettings["Fwork:folder__api_services"].ToString());
			string filepath = service_path + service_filename;
			StreamWriter sw = new StreamWriter(filepath, false);
			try {
				string newfilestr = this.serviceCode(type);
				sw.Write(newfilestr);
				newfilestr = null;
			} finally {
				sw.Close();
				sw.Dispose();
				sw = null;
			}
		}
		
		private string serviceCode(Type type) {
			string codestr = "";
			//---------- SERVICE HEADER
			codestr += "<%@ Page Language=\"C#\" ContentType=\"text/html\" ValidateRequest=\"" + ServiceWriterVars.ValidateRequest + "\" %>\r\n";
			codestr += "<%@ import Namespace=\"" + ServiceWriterVars.HttpNamespace + "\" %>\r\n";
			codestr += "<%@ import Namespace=\"" + type.FullName.Substring(0, type.FullName.LastIndexOf(".")) + "\" %>\r\n";
			codestr += "<script runat=\"server\">\r\n\r\n";
			codestr += "\t//Generated code by Fwork.Reflection\r\n\r\n";
			codestr += "\tvoid Page_Load() {\r\n";
			codestr += "\t\tResponse.Expires = 0;\r\n\r\n";
			//---------- RESPONSE TYPE
			codestr += "\t\tstring responseType = (Request.Params[\"responseType\"]!=null) ? Request.Params[\"responseType\"].ToString() : \"xml\";\r\n";
			codestr += "\t\tif(responseType == \"xml\") {\r\n";
			codestr += "\t\t	Response.Write(\"<?xml version=\\\"1.0\\\" encoding=\\\"" + ServiceWriterVars.XmlResponseEncoding + "\\\"?>\");\r\n";
			codestr += "\t\t}\r\n\r\n";
			//---------- OBJECT INSTANCE
			codestr += "\t\ttry {\r\n";
			codestr += "\t\t\t" + type.FullName + " obj = new " + type.FullName + "();\r\n";
			codestr += "\t\t\tobject classResponse = null;\r\n";
			codestr += "\t\t\ttry{\r\n";
			codestr += "\t\t\t\tswitch(Request.Params[\"api_method\"].ToLower()) {\r\n";
			MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			foreach(MethodInfo methodInfo in methods) {
				if(!(methodInfo.Name == "GetHashCode" || methodInfo.Name == "Equals" || methodInfo.Name == "ToString" || methodInfo.Name == "GetType" || (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_")) )) {
					codestr += "\t\t\t\t\tcase \"" + methodInfo.Name.ToLower() + "\":\r\n";
					string args = "";
					int paramcount = 1;
					foreach(ParameterInfo parameterInfo in methodInfo.GetParameters()) {
						args += this.paramText(paramcount, parameterInfo.ParameterType);
						paramcount++;
					}
					//strip last ','
					if(args.IndexOf(",") > 0) {
						args = args.Substring(0, args.LastIndexOf(","));
					}
					//write method
					codestr += "\t\t\t\t\t\tclassResponse = obj." + methodInfo.Name + "(" + args + ");\r\n";
					codestr += "\t\t\t\t\tbreak;\r\n";
				}
			}	//end foreach(MethodInfo methodInfo in methods) {
			codestr += "\t\t\t\t}\r\n";
			codestr += "\t\t\t} finally {\r\n";
			codestr += "\t\t\t\tobj = null;\r\n";
			codestr += "\t\t\t}\r\n";
			//---------- SERVICE RESPONSE
			codestr += "\t\t\tif(classResponse == null) {\r\n";
			codestr += "\t\t\t	Response.Write(GetResponseDone(responseType));\r\n";
			codestr += "\t\t\t} else if(classResponse.GetType().ToString() == \"System.String\" ||\r\n";
			codestr += "\t\t\t		  classResponse.GetType().ToString() == \"System.Int32\" ||\r\n";
			codestr += "\t\t\t		  classResponse.GetType().ToString() == \"System.Boolean\"\r\n";
			codestr += "\t\t	) {\r\n";
			codestr += "\t\t\t	Response.Write(GetResponse(classResponse.ToString(), responseType));\r\n";
			codestr += "\t\t\t}\r\n";
			codestr += "\t\t}  catch(Exception e) {\r\n";
			codestr += "\t\t	Response.Write(GetError(e, responseType));\r\n";
			codestr += "\t\t}\r\n";
			//---------- FOOTER
			codestr += "\t}\r\n\r\n";
			codestr += this.defaultFunctions() + "\r\n";
			codestr += "</script>";
			return codestr;
		}
		
		private string paramText(int count, Type type) {
			string typestr = type.ToString().Substring("System.".Length);
			return "Convert.To" + typestr + "(Request.Params[\"api_params" + count + "\"]), ";
		}
		
		private string defaultFunctions() {
			string codestr = "";
			codestr += "\tpublic string GetResponse(string response, string type) {\r\n";
			codestr += "\t	switch(type) {\r\n";
			codestr += "\t		case \"text\":\r\n";
			codestr += "\t			Response.ContentType=\"text/html\";\r\n";
			codestr += "\t			return response;\r\n";
			codestr += "\t		break;\r\n";
			codestr += "\t		case \"xml\":\r\n";
			codestr += "\t		default:\r\n";
			codestr += "\t			Response.ContentType=\"text/xml\";\r\n";
			codestr += "\t			return ServiceXml.Response(response);\r\n";
			codestr += "\t		break;\r\n";
			codestr += "\t	}\r\n";
			codestr += "\t}\r\n";
			codestr += "\t\r\n";
			codestr += "\tpublic string GetResponseDone(string type) {\r\n";
			codestr += "\t	switch(type) {\r\n";
			codestr += "\t		case \"text\":\r\n";
			codestr += "\t			Response.ContentType=\"text/html\";\r\n";
			codestr += "\t			return \"\";\r\n";
			codestr += "\t		break;\r\n";
			codestr += "\t		case \"xml\":\r\n";
			codestr += "\t		default:\r\n";
			codestr += "\t			Response.ContentType=\"text/xml\";\r\n";
			codestr += "\t			return ServiceXml.Done();\r\n";
			codestr += "\t		break;\r\n";
			codestr += "\t	}\r\n";
			codestr += "\t}\r\n";
			codestr += "\t\r\n";
			codestr += "\tpublic string GetError(Exception e, string type) {\r\n";
			codestr += "\t	switch(type) {\r\n";
			codestr += "\t		case \"text\":\r\n";
			codestr += "\t			Response.ContentType=\"text/html\";\r\n";
			codestr += "\t			return e.Message;\r\n";
			codestr += "\t		break;\r\n";
			codestr += "\t		case \"xml\":\r\n";
			codestr += "\t		default:\r\n";
			codestr += "\t			Response.ContentType=\"text/xml\";\r\n";
			codestr += "\t			return ServiceXml.Error(e);\r\n";
			codestr += "\t		break;\r\n";
			codestr += "\t	}\r\n";
			codestr += "\t}\r\n";
			return codestr;
		}
		
	}
	
}