using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Configuration;

namespace Fwork.Reflection {
	
	public struct JsWriterVars {
		public static bool Minimalist = true;
	}
	
	public class JsWriter {
		
		private string[] namespaces, assemblies;
		
		public JsWriter() {
		}
		public JsWriter(string[] namespaces, string[] assemblies) {
			this.namespaces = namespaces;
			this.assemblies = assemblies;
		}
		
		public void CreateFile() {
			string services_http_path = ConfigurationSettings.AppSettings["Fwork:httppath__api_services"].ToString();
			string js_filename = ConfigurationSettings.AppSettings["Fwork:filename__api_js"].ToString();
			string js_path = Folders.Check(ConfigurationSettings.AppSettings["Fwork:folder__api_js"].ToString());
			string filepath = js_path + js_filename;
			StreamWriter sw = new StreamWriter(filepath, false);
			try {
				string newfilestr = this.WriteJs();
				newfilestr += "\r\n__rreq = function() {";
				newfilestr += "	var req = new $d.ajaxRequest(\"" + services_http_path + "\" + arguments[0].replace(/\\./g, \"_\").toLowerCase() + \".aspx\");";
				newfilestr += "	req._apifn = arguments[0] + \".\" + arguments[1];";
				newfilestr += "	req.addParam(\"api_classname\", arguments[0]);";
				newfilestr += "	req.addParam(\"api_method\", arguments[1]);";
				newfilestr += "	var count = 1;";
				newfilestr += "	for (var i = 2; i < arguments.length; i++) {";
				newfilestr += "	req.addParam(\"api_params\" + count, arguments[i]);";
				newfilestr += "	count ++;";
				newfilestr += "	}";
				newfilestr += "	return req;";
				newfilestr += "}";
				sw.Write(newfilestr);
				newfilestr = null;
			} finally {
				sw.Close();
				sw.Dispose();
				sw = null;
			}
		}
		
		public string WriteJs() {
			string js = "";
			foreach(string ass_name in this.assemblies) {
				Assembly asm = Assembly.Load(new AssemblyName(ass_name));
				try {
					//js += this.writeAssemblyJs(ass_name);
					foreach(Type type in asm.GetTypes()) {
						if(this.namespaces.Contains(type.Namespace)) {
							js += this.writeNamespaceJs(type);
						}
					}
				} finally {
					asm = null;
				}
			}
			return js;
		}
		
		private string writeHeaderJs(string ass_name) {
			//Assembly header script
			string js = "";
			string[] names = ass_name.Split('.');
			string name = "";
			for(int i = 0; i < names.Length-1; i++) {
				name += names[i];
				js += "if(!" + name + "){";
				if (i == 0){
					js += "var " + name + "={};";
				} else {
					js += name + "={};";
				}
				js += "}";
				if (!JsWriterVars.Minimalist) js += "\r\n";
				name += ".";
			}
			js += "if(!" + ass_name + "){" + ass_name + "={};}";
			if (!JsWriterVars.Minimalist) js += "\r\n";
			return js;
		}
		
		private string writeNamespaceJs(Type type) {
			string js = "";
			//Namespace names
			string[] names = type.FullName.Split('.');
			//Class header script
			js += this.writeHeaderJs(type.Namespace);
			js += type.FullName + " = {";		//Nome da classe
			if (!JsWriterVars.Minimalist) js += "\r\n";
			//Class methods
			MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			foreach(MethodInfo methodInfo in methods) {
				if(!(methodInfo.Name == "GetHashCode" || methodInfo.Name == "Equals" || methodInfo.Name == "ToString" || methodInfo.Name == "GetType" || (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_")) )) {
					string args = "";
					int pcount = 1;
					foreach(ParameterInfo parameterInfo in methodInfo.GetParameters()) {
						if (!JsWriterVars.Minimalist) {
							args += parameterInfo.Name + ",";
						} else {
							args += "p" + pcount++.ToString() + ",";
						}
					}
					//strip last ,
					if(args.IndexOf(",") > 0) {
						args = args.Substring(0, args.LastIndexOf(","));
					}
					// Print the Method Signature
					if (!JsWriterVars.Minimalist) js += "\t";
					js += methodInfo.Name + ":function(" + args + ") {";
					if (!JsWriterVars.Minimalist) js += "\r\n";
					if (!JsWriterVars.Minimalist) js += "\t\t";
					if (args == "") {
						js += "return __rreq('" + methodInfo.DeclaringType.FullName + "','" + methodInfo.Name + "');";
					} else {
						js += "return __rreq('" + methodInfo.DeclaringType.FullName + "','" + methodInfo.Name + "'," + args + ");";
					}
					if (!JsWriterVars.Minimalist) js += "\r\n";
					if (!JsWriterVars.Minimalist) js += "\t";
					js += "},";
					if (!JsWriterVars.Minimalist) js += "\r\n";
				}
			}	//end foreach(MethodInfo methodInfo in methods) {
			//strip last ','
			if (js.IndexOf(",") > 0) {
				js = js.Substring(0, js.LastIndexOf(","));
				if (!JsWriterVars.Minimalist) js += "\r\n";
			}
			js += "};";
			if (!JsWriterVars.Minimalist) js += "\r\n";
			return js;
		}
		
	}
	
}