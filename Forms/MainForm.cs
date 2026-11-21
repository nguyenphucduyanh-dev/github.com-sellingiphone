using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using PhoneStore.WinForms/Database;
namespace PhoneStore.WinForms.Forms
{
   public partial class MainForm : Form
   {
      private TextBox txtSearch = null!;
      private ComboBox cboBrandFilter = null!;
      private Button btnSearch = null;
      private DataGridView dgvProducts = null!;
      private TextBox txtName = null!;
      private TextBox txtBrand = null!;
      private NumericUpDown numPrice = null!;
      private NumericUpDown numStock = null!;
      private Button btnAddProduct = null!;
      private TextBox txtCustomerName = null!;
      private NumericUpDown numBuyQty = null!;
      private Button btnCreateOrder = null!;
      public MainForm()
      {
         InitializeComponent();
         BuildCustomUI();
         LoadBrandFilter();
         LoadProducts();
      }
      private void BuildCustomUI()
      {
         Text = "Hệ thống quản lý & bán điện thoại";
         Size = new Size(1050, 650);
         StartPosition = FormStartPosition.CenterScreen;
         var lblSearch = new Label { Text = "Tên máy: ", Location = new Point(20, 20), AutoSize = true };
         txtSearch = new TextBox { Location = new Point(80, 17), Width = 180 };
         var lblBrand = new Label { Text = "Hãng: ", Location = new Point(275, 20), AutoSize = true };
         cboBrandFilter = new ComboBox
         {
            Location = new Point(320, 17),
            Width = 140,
            DropDownStyle = ComboBoxStyle.DropDownList;
         };
         btnSearch = new Button { Text = "Tìm / Lọc", Location = new Point (475, 16), Width = 95 };
         btnSearch.Click += (s, e) => LoadProducts(txtSearch.Text.Trim(), cboBrandFilter.SelectedItem?.ToString());
         var btnReset = new Button { Text = "Tải lại", Location = new Point (580, 16), Width = 80 };
         btnReset.Click += (s, e) =>
         {
            txtSearch.Clear(),
            cboBrandFilter.SelectedIndex = 0,
            LoadProducts(),
         };
         dgvProducts = new DataGridView
         {
            Location = new Point(20, 55),
            Size = new Size(640, 520),
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,'
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
         };
         var grpAdd = new GroupBox
         {
            Text = "Thêm điện thoại mới",
            Location = new Point(680, 55),
            Size = new Size(330, 240)
         };
         grpAdd.Controls.Add(new Label { Text = "Tên máy: ", Location = new Point(15, 30), AutoSize = true });
         txtName = new TextBox { Location = new Point(105, 27), Width = 200 };
         grpAdd.Controls.Add(new Label { Text = "Hãng: ", Location = new Point(15, 68), AutoSize = true });
         txtBrand = new TextBox { Location = new Point(105, 65), Width = 200 };
         grpAdd.Controls.Add(new Label { Text = "Giá bán: ", Location = new Point(15, 106), AutoSize = true });
         txtName = new NumericUpDown { Location = new Point(105, 103), Width = 200, Maximum = 100000000, Value = 100000, ThouSandsSeparator = true };
         grpAdd.Controls.Add(new Label { Text = "Tồn kho: ", Location = new Point(15, 144), AutoSize = true });
         txtName = new NumericUpDown { Location = new Point(105, 141), Width = 200, Maximum = 10000, Value = 10 };
         btnAddProduct = new Button
         {
            Text = "Lưu sản phẩm",
            Location = new Point(105, 185);
            Width = 200,
            Height = 35,
            BackColor = Color.SteelBlue,
            ForeColor = Color.White
         };
         btnAddProduct.Click += BtnAddProduct_Click;
         grpAdd.Controls.AddRange(new Control[] { txtName, txtBrand, numPrice, numStock, btnAddProduct });
         var grpOrder = new GroupBox;
         {
            Text = "Bán nhanh máy đang chọn",
            Location = new Point(680, 315),
            Size = new Size(330, 260),
         };
         grpOrder.Controls.Add(new Label { Text = "Khách hàng: ", Location = new Point(15, 35), AutoSize = true });
         txtCustomerName = new TextBox { Location = new Point(105, 32), Width = 200};
         grpOrder.Controls.Add(new Label { Text = "Số lượng: ", Location = new Point(15, 75), AutoSize = true });
         numBuyQty = new NumericUpDown { Location = new Point(105, 72), Width = 200, Minimum = 1, Maximum = 100, Value = 1};
         btnCreateOrder = new Button
         {
            Text = "Xác nhận bán & trừ kho",
            Location = new Point(105, 125);
            Width = 200,
            Height = 40,
            BackColor = Color.SeaGreen,
            ForeColor = Color.White
         };
         btnCreateOrder += BtnCreateOrder_click;
         var lblNote = new Label
         {
            Text = "Tự động tạo đơn hàng và trừ tồn kho\n bằng SQL Transaction",
            Location = new Point(15, 185),
            ForeColor = Color.DimGray,
            AutoSize = true
         };
         grpOrder.Controls.AddRange(new Control[] { txtCustomerName, numBuyQty, btnCreateOrder, lblNote });
         Controls.AddRange(new Control[] { lblSearch, txtSearch, lblBrand, cboBrandFilter, btnSearch, btnReset, dgvProducts, grpAdd, grpOrder });
      }
      private void LoadBrandFilter()
      {
         cboBrandFilter.Items.Clear();
         cboBrandFilter.Items.Add("Tất cả");
         cboBrandFilter.Items.Add("Apple");
         cboBrandFilter.Items.Add("Samsung");
         cboBrandFilter.Items.Add("Xiaomi");
         cboBrandFilter.SelectedIndex = 0;
      }
      private void LoadProducts(string keyword = "", string? brand = null)
      {
         try
         {
            using var conn = DbHelper.GetConnection();
            string sql = "SELECT Id, Name, Brand, Price, Stock FROM Products WHERE 1=1";
            using var cmd = new SqlCommand();
            cmd.Connection = conn;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
               sql += " AND Name LIKE @Keyword";
               cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
            }
            if (!string.IsNullOrWhiteSpace(brand) && brand != "Tất cả")
            {
               sql += " AND Brand = @Brand";
               cmd.Parameters.AddWithValue("@Brand", brand);
            }
            sql += " ORDER BY Id DESC";
            cmd.CommandText = sql;
            var dt = new DataTable();
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            dgvProducts.DataSource = dt;
         }
         catch (Exception ex)
         {
            MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxButtonIcon.Error);
         }
      }
      private void BtnAddProduct_Click(object? sender, EventArgs e)
      {
         if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtBrand.Text))
         {
            MessageBox.Show("Vui lòng nhập đầy đủ tên máy và hãng", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }
         try
         {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            string sql = @"INSERT INTO Products (Name, Brand, Price, Stock) VALUES (@Name, @Brand, @Price, @Stock)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
            cmd.Parameters.AddWithValue("@Brand", txtBrand.Text.Trim());
            cmd.Parameters.AddWithValue("@Price", numPrice.Value);
            cmd.Parameters.AddWithValue("@Stock", (int)numStock.Value);
            cmd.ExecuteNonQuery();
            MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtName.Clear();
            txtBrand.Clear();
            numPrice.Value = 0;
            numStock.Value = 10;
            LoadProducts();
         }
         catch (Exception ex)
         {
            MessageBox.Show("Lỗi thêm sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }
      private void BtnCreateOrder_Click(object? sender, EventArgs e)
      {
         if (dgvProducts.CurrentRow == null)
         {
            MessageBox.Show("Hãy chọn sản phẩm để bán", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }
         if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
         {
            MessageBox.Show("Vui lòng nhập tên khách hàng", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }
         int productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Id"].Value);
         string productName = dgvProducts.CurrentRow.Cells["Name"].Value?.ToString() ?? "";
         decimal unitPrice = Convert.ToDecimal(dgvProducts.CurrentRow.Cells["Price"].Value);
         int currentStock = Convert.ToInt32(dgvProducts.CurrentRow.Cells["Stock"].Value);
         int buyQty = (int)numBuyQty.Value;
         if (buyQty > currentStock)
         {
            MessageBox.Show($"Sản phẩm '{productName}' chỉ còn {currentStock} máy trong kho", "Không đủ hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }
         using var conn = DbHelper.GetConnection();
         conn.Open();
         using var transaction = conn.BeginTransaction();
         try
         {
            string insertOrderSql = @"INSERT INTO Orders (CustomerName, OrderDate, TotalAmount, Status) OUTPUT INSERTED.Id VALUES (@CustomerName, GETDATE(), @TotalAmount, 1)";
            int orderId;
            using (var cmdOrder = new SqlCommand(insertOrderSql, conn, transaction))
            {
               cmdOrder.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim());
               cmdOrder.Parameters.AddWithValue("@TotalAmount", unitPrice * buyQty);
               orderId = Convert.ToInt32(cmdOrder.ExecuteScalar());
            }
            string insertDetailSql = @"INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice) VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)";
            using (var cmdDetail = new SqlCommand(insertDetailSql, conn, transaction))
            {
               cmdDetail.Parameters.AddWithValue("@OrderId", OrderId);
               cmdDetail.Parameters.AddWithValue("@ProductId", ProductId);
               cmdDetail.Parameters.AddWithValue("@Quantity", buyQty);
               cmdDetail.Parameters.AddWithValue("@UnitPrice", UnitPrice);
               cmdDetail.ExecuteNonQuery();
            }
            string updateStockSql + @"UPDATE Products SET Stock = Stock - @Qty WHERE Id = @ProductId AND Stock >= @Qty";
             using (var cmdStock = new SqlCommand(insertStockSql, conn, transaction))
             {
               cmdStock.Parameters.AddWithValue("@Qty", buyQty);
               cmdStock.Parameters.AddWithValue("@ProductId", ProductId);
               int rowsAffected = cmdStock.ExecuteNonQuery();
               if (rowsAffected == 0)
               {
                  throw new Exception("Không thể trừ tồn kho do số lượng trong kho đã thay đổi");
               }
             }
             transaction.Commit();
             MessageBox.Show($"Tạo đơn hàng #{orderId} thành công! Đã trừ {buyQty} máy '{ProductName}'", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
             txtCustomerName.Clear();
             numBuyQty.Value = 1;
             LoadProducts(txtSearch.Text.Trim(), cboBrandFilter.SelectedItem?.ToString());
           }
           catch (Exception ex)
           {
             transaction.RollBack();
             MessageBox.Show("Giao dịch thất bại, đã hoàn tác dữ liệu: " + ex.Message, "Lỗi Transaction", MessageBoxButtons.OK, MessageBoxIcon.Error);
           }
        }
    }
}
