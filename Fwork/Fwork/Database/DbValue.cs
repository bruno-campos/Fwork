using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fwork.Database {
	
	public class DbValue {
	
		public static string GetValue(object value) {
			return (value == DBNull.Value) ? "" : value.ToString();
		}
		
		public static object Get(object value) {
			return (value == DBNull.Value) ? null : value;
		}
	
	}
	
}