using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace CafeShopManagementSystem
{
	class AdminAddProductsData
	{
        SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Windows\Documents\cafe.mdf;Integrated Security=True;Connect Timeout=30");

        public int ID { get; set; }
        public string ProductID { get; set; }
		public string ProductName { get; set; }
		public string Type { get; set; }
		public string Stock { get; set; }
		public string Price { get; set; }
		public string Status { get; set; }
		public string Image { get; set; }
		public string DateInsert { get; set; }
		public string DateUpdate { get; set; }

		public List<AdminAddProductsData> productsListData()
		{
            List<AdminAddProductsData> listData = new List<AdminAddProductsData>();

            if (connect.State != ConnectionState.Open)
            {
                try
                {
                    connect.Open();

                    string selectData = "SELECT * FROM products WHERE date_delete IS NULL";

                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            AdminAddProductsData productData = new AdminAddProductsData();

                            productData.ID = (int)reader["id"];
                            productData.ProductID = reader["prod_id"].ToString(); ;
                            productData.ProductName = reader["prod_name"].ToString();
                            productData.Type = reader["prod_type"].ToString();
                            productData.Stock = reader["prod_stock"].ToString();
                            productData.Price = reader["prod_price"].ToString();
                            productData.Status = reader["prod_status"].ToString();
                            productData.Image = reader["prod_image"].ToString();
                            productData.DateInsert = reader["date_insert"].ToString();
                            productData.DateUpdate = reader["date_update"].ToString();

                            listData.Add(productData);
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

        public List<AdminAddProductsData> availableProductsData()
        {
            List<AdminAddProductsData> listData = new List<AdminAddProductsData>();

            if (connect.State != ConnectionState.Open)
            {
                try
                {
                    connect.Open();

                    string selectData = "SELECT * FROM products WHERE prod_status = @stats";

                    using (SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        cmd.Parameters.AddWithValue("@stats", "Available");

                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            AdminAddProductsData productData = new AdminAddProductsData();

                            productData.ID = (int)reader["id"];
                            productData.ProductID = reader["prod_id"].ToString(); ;
                            productData.ProductName = reader["prod_name"].ToString();
                            productData.Type = reader["prod_type"].ToString();
                            productData.Stock = reader["prod_stock"].ToString();
                            productData.Price = reader["prod_price"].ToString();

                            listData.Add(productData);
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
