using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CafeShopManagementSystem
{
    class CashierOrdersData
    {
        SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Windows\Documents\cafe.mdf;Integrated Security=True;Connect Timeout=30");

        public int ID { set; get; }
        public int CID { get; set; }
        public string ProdID { get; set; }
        public string ProdName { get; set; }
        public string ProdType { get; set; }
        public int Qty { get; set; }
        public string Price { get; set; }

        public List<CashierOrdersData> orderListData()
        {
            List<CashierOrdersData> listData = new List<CashierOrdersData>();

            if (connect.State != ConnectionState.Open)
            {
                try
                {
                    connect.Open();

                    int custId = 0;

                    string selectCustData = "SELECT MAX(customer_id) FROM customers";

                    using (SqlCommand getCustData = new SqlCommand(selectCustData, connect))
                    {
                        object result = getCustData.ExecuteScalar();

                        if(result != DBNull.Value)
                        {
                            int temp = Convert.ToInt32(result);

                            if(temp == 0)
                            {
                                custId = 1;
                            }
                            else
                            {
                                custId = temp + 1;
                            }
                        }
                        else 
                        {
                            custId = 1;
                        }
                    }

                    string selectData = "SELECT * FROM orders WHERE customer_id = @customerId";

                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        cmd.Parameters.AddWithValue("@customerId", custId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CashierOrdersData ordersData = new CashierOrdersData();

                                ordersData.ID = (int)reader["id"];
                                ordersData.CID = (int)reader["customer_id"];
                                ordersData.ProdID = reader["prod_id"].ToString();
                                ordersData.ProdName = reader["prod_name"].ToString();
                                ordersData.ProdType = reader["prod_type"].ToString();
                                ordersData.Qty = (int)reader["qty"];
                                ordersData.Price = reader["total_amt"].ToString();

                                listData.Add(ordersData);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection Failed: " + ex);
                }
                finally
                {
                    connect.Close();
                }
            }
            return listData;
        }
    }
}
