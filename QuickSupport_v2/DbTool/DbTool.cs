using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using DevExpress.XtraPrinting.Native;
using QuickSupport_v2.Model;

namespace QuickSupport_v2.DbTool
{
    public class DbTool
    {
        public static DataTable Query(SqlConnection connection, string strCommand, FPT.Framework.Data.DataObject parameters)
        {
            DataTable dataTable = new DataTable();
            if (ExIsOpen(connection))
            {
                using (SqlCommand command = new SqlCommand(strCommand, connection))
                {
                    try
                    {
                        using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command))
                        {
                            command.CommandText = strCommand;
                            command.CommandType = CommandType.Text;
                            if (parameters != null)
                            {
                                foreach (KeyValuePair<string, object> parameter in (IEnumerable<KeyValuePair<string, object>>)parameters)
                                {
                                    if (!(parameter.Key == string.Empty))
                                        command.Parameters.AddWithValue("@" + parameter.Key, parameter.Value ?? (object)DBNull.Value);
                                }
                            }
                            sqlDataAdapter.Fill(dataTable);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        return null;
                    }
                }
            }
            return dataTable;
        }

        public static bool ExecuteNonQuery(SqlConnection connection, string strCommand, FPT.Framework.Data.DataObject parameters = null)
        {
            try
            {
                using (var cmd = connection.CreateCommand())
                {
                    if (ExIsOpen(connection))
                    {
                        cmd.CommandText = strCommand;
                        if (parameters != null)
                        {
                            foreach (KeyValuePair<string, object> parameter in (IEnumerable<KeyValuePair<string, object>>)parameters)
                            {
                                if (!(parameter.Key == string.Empty))
                                    cmd.Parameters.AddWithValue("@" + parameter.Key, parameter.Value ?? (object)DBNull.Value);
                            }
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;  
            }
        }

        public static DataSet QueryStored(SqlConnection connection, string strCommand, FPT.Framework.Data.DataObject parameters)
        {
            DataSet ds = new DataSet();
            if (ExIsOpen(connection))
            {
                using (SqlCommand command = new SqlCommand(strCommand, connection))
                {
                    try
                    {
                        using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command))
                        {

                            command.CommandText = strCommand;
                            command.CommandType = CommandType.StoredProcedure;
                            if (parameters != null)
                            {
                                foreach (KeyValuePair<string, object> parameter in (IEnumerable<KeyValuePair<string, object>>)parameters)
                                {
                                    if (!(parameter.Key == string.Empty))
                                        command.Parameters.AddWithValue("@" + parameter.Key, parameter.Value ?? (object)DBNull.Value);
                                }
                            }
                            sqlDataAdapter.Fill(ds);
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        return null;
                    }
                }
            }
            return ds;
        }

        public static void ExcuteStored(SqlConnection connection, string strCommand, FPT.Framework.Data.DataObject parameters)
        {
            if (ExIsOpen(connection))
            {
                using (SqlCommand command = new SqlCommand(strCommand, connection))
                {
                    try
                    {
                        using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command))
                        {
                            command.CommandText = strCommand;
                            command.CommandType = CommandType.StoredProcedure;
                            if (parameters != null)
                            {
                                foreach (KeyValuePair<string, object> parameter in (IEnumerable<KeyValuePair<string, object>>)parameters)
                                {
                                    if (!(parameter.Key == string.Empty))
                                        command.Parameters.AddWithValue("@" + parameter.Key, parameter.Value ?? (object)DBNull.Value);
                                }
                            }
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }
        public static bool ExIsOpen(SqlConnection connection)
        {
            if (connection == null) { return false; }
            if (connection.State == ConnectionState.Open) { return true; }

            try
            {
                connection.Open();
                return true;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            return false;
        }
        public static void CloseConn(SqlConnection connection)
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }
}
