using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Drawing.Printing;

namespace CafeShopManagementSystem
{
    public partial class CashierOrderForm: UserControl
    {
        public static int getCustID;

        SqlConnection connect = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Windows\Documents\cafe.mdf;Integrated Security=True;Connect Timeout=30");
        public CashierOrderForm()
        {
            InitializeComponent();

            displayAvailableProducts();
            displayAllOrders();
            displaySum();
        }

        public void refreshData()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)refreshData);
                return;
            }

            displayAvailableProducts();
            displayAllOrders();
            displaySum();
        }

        public void displayAvailableProducts()
        {
            CashierOrderFormProdData allProds = new CashierOrderFormProdData();
            List<CashierOrderFormProdData> listData = allProds.availableProductsData();

            cashierOrderForm_menuTable.DataSource = listData;
        }

        private float totalAmt;
        public void displaySum()
        {
            idGenerator();

            if (connect.State != ConnectionState.Open)
            {
                try
                {
                    connect.Open();

                    string selectData = "SELECT SUM(total_amt) FROM orders WHERE customer_id = @custId";

                    using(SqlCommand cmd = new SqlCommand(selectData, connect))
                    {
                        cmd.Parameters.AddWithValue("@custId", idGen);

                        object result = cmd.ExecuteScalar();

                        if(result != DBNull.Value && result != null)
                        {
                            totalAmt = Convert.ToSingle(result);

                            cashierOrderForm_orderPrice.Text = totalAmt.ToString("0.00");
                        }
                        else
                        {
                            //totalAmt = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection failed: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        public void displayAllOrders()
        {
            CashierOrdersData allOrders = new CashierOrdersData();
            List<CashierOrdersData> listData = allOrders.orderListData();

            cashierOrderForm_orderTable.DataSource = listData;

            if (cashierOrderForm_orderTable.Rows.Count > 0)
            {
                getOrderID = Convert.ToInt32(cashierOrderForm_orderTable.Rows[0].Cells[0].Value);
            }
        }

        private void cashierOrderForm_addBtn_Click(object sender, EventArgs e)
        {
            idGenerator();

            if(cashierOrderForm_type.SelectedIndex == -1 || cashierOrderForm_productID.SelectedIndex == -1 || cashierOrderForm_prodName.Text == "" ||
                cashierOrderForm_quantity.Value ==0 || cashierOrderForm_price.Text == "")
            {
                MessageBox.Show("Please select an product.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (connect.State != ConnectionState.Open)
                {
                    try
                    {
                        float getPrice = 0;

                        connect.Open();

                        string selectOrder = "SELECT * FROM products WHERE prod_id = @prodId";

                        using (SqlCommand getOrder = new SqlCommand(selectOrder, connect))
                        {
                            getOrder.Parameters.AddWithValue("@prodId", cashierOrderForm_productID.Text.Trim());

                            using (SqlDataReader reader = getOrder.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    object rawValue = reader["prod_price"];

                                    if(rawValue != DBNull.Value)
                                    {
                                        getPrice = Convert.ToSingle(rawValue);
                                    }
                                    else
                                    {
                                        getPrice = 0.0f;
                                    }
                                }
                                else
                                {
                                    getPrice = 0.0f;
                                }

                                reader.Close();
                            }
                        }

                        string insertOrder = "INSERT INTO orders (customer_id, prod_id, prod_name, prod_type, prod_price, qty, total_amt, order_date)" +
                        "VALUES (@customerID, @prodId, @prodName, @prodType, @prodPrice, @qty, @totalAmt, @orderDate)";

                        DateTime today = DateTime.Today;

                        using (SqlCommand cmd = new SqlCommand(insertOrder, connect))
                        {
                            cmd.Parameters.AddWithValue("@customerID", idGen);
                            cmd.Parameters.AddWithValue("@prodID", cashierOrderForm_productID.Text.Trim());
                            cmd.Parameters.AddWithValue("@prodName", cashierOrderForm_prodName.Text.Trim());
                            cmd.Parameters.AddWithValue("@prodType", cashierOrderForm_type.Text.Trim());
                            cmd.Parameters.AddWithValue("@prodPrice", cashierOrderForm_price.Text.Trim());
                            cmd.Parameters.AddWithValue("@qty", cashierOrderForm_quantity.Value);

                            float totalAmt = getPrice * ((int)cashierOrderForm_quantity.Value);
                            cmd.Parameters.AddWithValue("@totalAmt", totalAmt);
                            cmd.Parameters.AddWithValue("@orderDate", today);

                            cmd.ExecuteNonQuery();

                            displayAllOrders();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection failed: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        connect.Close();
                    }
                }
                displaySum();
            }
        }

        private int idGen = 0;
        public void idGenerator()
        {
            if (connect.State != ConnectionState.Open)
            {
                try
                {
                    connect.Open();

                    string selectId = "SELECT MAX(customer_id) FROM customers";

                    using (SqlCommand cmd = new SqlCommand(selectId, connect))
                    {
                        object result = cmd.ExecuteScalar();

                        if(result != DBNull.Value)
                        {
                            int temp = Convert.ToInt32(result);

                            if (temp == 0)
                            {
                                idGen = 1;
                            }
                            else
                            {
                                idGen = temp + 1;
                            }
                        }
                        else
                        {
                            idGen = 1;
                        }
                        getCustID = idGen;
                    }
                }
                catch (Exception connectEx)
                {
                    MessageBox.Show("Connection failed: " + connectEx, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        private void cashierOrderForm_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            cashierOrderForm_productID.SelectedIndex = -1;
            cashierOrderForm_productID.Items.Clear();
            cashierOrderForm_prodName.Text = "";
            cashierOrderForm_price.Text = "";

            string selectedValue = cashierOrderForm_type.SelectedItem as string;

            if (selectedValue != null)
            {
                try
                {
                    if (selectedValue != null)
                    {
                        try
                        {
                            connect.Open();
                            string selectData = $"SELECT * FROM products WHERE prod_type = '{selectedValue}' AND prod_status = @status AND date_delete IS NULL";

                            using (SqlCommand cmd = new SqlCommand(selectData, connect))
                            {
                                cmd.Parameters.AddWithValue("@status", "Available");

                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string value = reader["prod_id"].ToString();

                                        cashierOrderForm_productID.Items.Add(value);
                                    }
                                }
                            }
                        }
                        catch (Exception connectEx)
                        {
                            MessageBox.Show("Connection failed: " + connectEx, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            connect.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Item Selection Error: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void cashierOrderForm_productID_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedValue = cashierOrderForm_productID.SelectedItem as string;

            if (selectedValue != null)
            {
                try
                {
                    if (connect.State != ConnectionState.Open)
                    {
                        try
                        {
                            connect.Open();

                            string selectData = $"SELECT * FROM products WHERE prod_id = '{selectedValue}' AND prod_status = @status AND date_delete IS NULL";

                            using (SqlCommand cmd = new SqlCommand(selectData, connect))
                            {
                                cmd.Parameters.AddWithValue("@status", "Available");

                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string prodName = reader["prod_name"].ToString();
                                        string price = reader["prod_price"].ToString();

                                        cashierOrderForm_prodName.Text = prodName;
                                        cashierOrderForm_price.Text = price;
                                    }
                                }
                            }
                        }
                        catch (Exception connectEx)
                        {
                            MessageBox.Show("Connection failed: " + connectEx, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            connect.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Item Selection Error: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cashierOrderForm_amount_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                try
                {
                    float getAmount = Convert.ToSingle(cashierOrderForm_amount.Text);

                    float getChange = (getAmount - totalAmt);

                    if (getChange < 0)
                    {
                        MessageBox.Show("Amount paid is not enough!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        cashierOrderForm_change.Text = getChange.ToString();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid payment amount keyed." + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cashierOrderForm_payBtn_Click(object sender, EventArgs e)
        {

            if (cashierOrderForm_amount.Text == "" || cashierOrderForm_orderTable.Rows.Count < 1)
            {
                MessageBox.Show("No items / pay amount has been entered.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if(MessageBox.Show("Confirm payment ?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    idGenerator();

                    displaySum();

                    if (connect.State != ConnectionState.Open)
                    {
                        try
                        {
                            connect.Open();

                            string insertData = "INSERT INTO customers (customer_id, total_price, pay_amt, change, date) " +
                                "VALUES(@custId, @totalPrice, @payAmt, @change, @date)";

                            DateTime today = DateTime.Today;

                            using (SqlCommand cmd = new SqlCommand(insertData, connect))
                            {
                                cmd.Parameters.AddWithValue("@custId", idGen);
                                cmd.Parameters.AddWithValue("@totalPrice", totalAmt);
                                cmd.Parameters.AddWithValue("@payAmt", cashierOrderForm_amount.Text.Trim());
                                cmd.Parameters.AddWithValue("@change", cashierOrderForm_change.Text.Trim());
                                cmd.Parameters.AddWithValue("@date", today);

                                cmd.ExecuteNonQuery();

                                MessageBox.Show("Payment successfully made.", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                paymentMade = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Connection failed: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            connect.Close();
                        }
                    }
                }
            }
            //displaySum();
        }

        private int rowIndex = 0;

        private void cashierOrderForm_receiptBtn_Click(object sender, EventArgs e)
        {
            printDocument1.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);
            printDocument1.BeginPrint += new PrintEventHandler(printDocument1_BeginPrint);

            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.ShowDialog();
        }

        private void printDocument1_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            rowIndex = 0;
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            displaySum();

            float y = 0;
            int count = 0;
            int colWidth = 120;
            int headerMargin = 10;
            int tableMargin = 20;

            Font font = new Font("Arial", 12);
            Font bold = new Font("Arial", 12, FontStyle.Bold);
            Font headerFont = new Font("Arial", 16, FontStyle.Bold);
            Font labelFont = new Font("Arial", 14, FontStyle.Bold);

            float margin = e.MarginBounds.Top;

            StringFormat alignCenter = new StringFormat();
            alignCenter.Alignment = StringAlignment.Center;
            alignCenter.LineAlignment = StringAlignment.Center;

            string headerText = "Winterdream's Cafe";
            y = (margin + count * headerFont.GetHeight(e.Graphics) + headerMargin);
            e.Graphics.DrawString(headerText, headerFont, Brushes.Black, 
                e.MarginBounds.Left + (cashierOrderForm_orderTable.Columns.Count / 2) * colWidth,
                y, alignCenter);

            count++;
            y += tableMargin;

            string[] header = { "CID", "ProdID", "ProdName", "ProdType", "Qty", "Price" };

            for (int i = 0; i < header.Length; i++)
            {
                y = margin + count * bold.GetHeight(e.Graphics) + tableMargin;
                e.Graphics.DrawString(header[i], bold, Brushes.Black, e.MarginBounds.Left + i * colWidth, y, alignCenter);
            }
            
            count++;

            float rSpace = e.MarginBounds.Bottom - y;

            while(rowIndex < cashierOrderForm_orderTable.Rows.Count)
            {
                DataGridViewRow row = cashierOrderForm_orderTable.Rows[rowIndex];

                for(int i = 0; i < cashierOrderForm_orderTable.Columns.Count; i++)
                {
                    object cellValue = row.Cells[i].Value;
                    string cell = (cellValue != null) ? cellValue.ToString() : string.Empty;

                    y = margin + count * font.GetHeight(e.Graphics) + tableMargin;
                    e.Graphics.DrawString(cell, font, Brushes.Black, e.MarginBounds.Left + i * colWidth, y, alignCenter);
                }
                count++;
                rowIndex++;

                if(y + font.GetHeight(e.Graphics) > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            int labelMargin = (int)Math.Min(rSpace, 90);

            DateTime today = DateTime.Now;

            float labelX = e.MarginBounds.Right - e.Graphics.MeasureString("-------------------------", labelFont).Width;

            y = e.MarginBounds.Bottom - labelMargin - labelFont.GetHeight(e.Graphics);

            e.Graphics.DrawString("Total Price: \t$ " + totalAmt + "\nAmount : \t$ " + cashierOrderForm_amount.Text + "\n\t\t----------\nChange : \t$ " + cashierOrderForm_change.Text,
                labelFont, Brushes.Black, labelX, y);

            labelMargin = (int)Math.Min(rSpace, -40);

            string labelText = today.ToString();

            y = e.MarginBounds.Bottom - labelMargin - labelFont.GetHeight(e.Graphics);

            e.Graphics.DrawString(labelText, labelFont, Brushes.Black, e.MarginBounds.Right - e.Graphics.MeasureString("-------------------------", labelFont).Width, y);
        }

        public void clearFields()
        {
            cashierOrderForm_type.SelectedIndex = -1;
            cashierOrderForm_productID.SelectedIndex = -1;
            cashierOrderForm_prodName.Text = "";
            cashierOrderForm_price.Text = "";
            cashierOrderForm_quantity.Value = 0;
        }

        public void clearTotal()
        {
            cashierOrderForm_orderTable.DataSource = "";
            cashierOrderForm_orderPrice.Text = "0.00";
            cashierOrderForm_amount.Text = "";
            cashierOrderForm_change.Text = "";
            totalAmt = 0;
        }

        private void cashierOrderForm_clearBtn_Click(object sender, EventArgs e)
        {
            clearFields();
        }

        private int getOrderID = 0;
        private void cashierOrderForm_orderTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = cashierOrderForm_orderTable.Rows[e.RowIndex];
            getOrderID = (int)row.Cells[0].Value;
        }

        private void cashierOrderForm_removeBtn_Click(object sender, EventArgs e)
        {
            if (getOrderID == 0)
            {
                MessageBox.Show("Select item first", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to Remove the Order ID: " + getOrderID + "?", "Confirmation Message",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (connect.State != ConnectionState.Open)
                    {
                        try
                        {
                            connect.Open();

                            string deleteData = "DELETE FROM orders WHERE id = @id";

                            using (SqlCommand cmd = new SqlCommand(deleteData, connect))
                            {
                                cmd.Parameters.AddWithValue("@id", getOrderID);

                                cmd.ExecuteNonQuery();

                                MessageBox.Show("Removed successfully!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            displayAllOrders();

                            if (cashierOrderForm_orderTable.Rows.Count < 1)
                            {
                                clearTotal();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Connection failed: " + ex, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            connect.Close();
                        }
                    }
                }
            }
        }

        private bool paymentMade = false;
        private void cashierOrderForm_newBtn_Click(object sender, EventArgs e)
        {
            if(paymentMade == true)
            {
                clearTotal();

                displayAllOrders();
            }
            else if (cashierOrderForm_orderTable.Rows.Count < 1)
            {
                MessageBox.Show("Please select an item first.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("There is item(s) in inventory, please remove or proceed to payment first!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
