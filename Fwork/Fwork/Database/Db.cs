using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fwork.Database {
	
	public struct DbConnVariables {
		public static string ConnString = "";
	}
	
	class Db {
		
		private static readonly Db instance = new Db();
		
		public static Db Instance {
			get{ return instance; }
		}
		public static SqlConnection Conn {
			get{return new SqlConnection(DbConnVariables.ConnString);}
		}
		
		private Db() {
		}
		
		public static void Open(SqlConnection conn) {
			conn.Open();
		}
		
		public static void Close(SqlConnection conn) {
			conn.Close();
			conn.Dispose();
			conn = null;
		}

	}
	
}