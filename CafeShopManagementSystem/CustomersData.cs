using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace CafeShopManagementSystem
{
    class CustomersData
    {
        SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Windows\Documents\cafe.mdf;Integrated Security=True;Connect Timeout=30");

        public int CustomerID { get; set; }
        public string TotalPrice { get; set; }
        public string Date {  get; set; }
        public string PayAmt { get; set; }
        public string Change {  get; set; }
        public string Users { get; set; }

        public List<CustomersData> allCustomersData()
        {
            List<CustomersData> listData = new List<CustomersData>();

            if (connect.State != ConnectionState.Open)
            {
                try
                {
                    connect.Open();

                    string selectData = "SELECT * FROM customers";

                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CustomersData customerData = new CustomersData();

                                customerData.CustomerID = (int)reader["customer_id"];
                                customerData.TotalPrice = reader["total_price"].ToString();
                                customerData.PayAmt = reader["pay_amt"].ToString();
                                customerData.Change = reader["change"].ToString();
                                customerData.Date = reader["date"].ToString();
                                customerData.Users = reader["users"].ToString();

                                listData.Add(customerData);
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
