using CapaPresentacion.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Modales
{

    public partial class mdProducto : Form
    {
        string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True;TrustServerCertificate=True;";
        public Producto SelectedProducto { get; private set; }

        public mdProducto()
        {
            InitializeComponent();
            // Configura las columnas del DataGridView aquí para asegurarte de que tengan los nombres correctos
            dgvdata.Columns.Add("Codigo", "Código");
            dgvdata.Columns.Add("Nombre", "Nombre");
            dgvdata.Columns.Add("Categoria", "Categoría");
            dgvdata.Columns.Add("Stock", "Stock");
            dgvdata.Columns.Add("PrecioCompra", "Precio Compra");
            dgvdata.Columns.Add("PrecioVenta", "Precio Venta");
        }

        private void mdProducto_Load(object sender, EventArgs e)
        {
            // Llenar el ComboBox de búsqueda
            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                cbobusqueda.Items.Add(new { Valor = columna.Name, Texto = columna.HeaderText });
            }

            cbobusqueda.DisplayMember = "Texto";
            cbobusqueda.ValueMember = "Valor";
            cbobusqueda.SelectedIndex = 0;

            // Cargar datos desde la base de datos
            LoadProductos();
        }

        private void LoadProductos()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT p.Codigo, p.Nombre, c.Descripcion AS Categoria, p.Stock, p.PrecioCompra, p.PrecioVenta " +
                               "FROM PRODUCTO p " +
                               "INNER JOIN CATEGORIA c ON p.IdCategoria = c.IdCategoria";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                dgvdata.Rows.Clear(); // Limpiar filas existentes antes de añadir nuevas
                foreach (DataRow row in dataTable.Rows)
                {
                    dgvdata.Rows.Add(row["Codigo"], row["Nombre"], row["Categoria"], row["Stock"], row["PrecioCompra"], row["PrecioVenta"]);
                }
            }
        }

        private void dgvdata_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int iRow = e.RowIndex;
            if (iRow >= 0)
            {
                string codigo = dgvdata.Rows[iRow].Cells["Codigo"].Value.ToString();
                string nombre = dgvdata.Rows[iRow].Cells["Nombre"].Value.ToString();
                int stock = Convert.ToInt32(dgvdata.Rows[iRow].Cells["Stock"].Value);
                decimal precioCompra = Convert.ToDecimal(dgvdata.Rows[iRow].Cells["PrecioCompra"].Value);
                decimal precioVenta = Convert.ToDecimal(dgvdata.Rows[iRow].Cells["PrecioVenta"].Value);

                SelectedProducto = new Producto
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    Stock = stock,
                    PrecioCompra = precioCompra,
                    PrecioVenta = precioVenta
                };

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string columnaFiltro = ((dynamic)cbobusqueda.SelectedItem).Valor.ToString();
            string busqueda = txtbusqueda.Text.Trim().ToUpper();

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.IsNewRow) continue; // Evita procesar la fila nueva
                if (row.Cells[columnaFiltro].Value != null &&
                    row.Cells[columnaFiltro].Value.ToString().Trim().ToUpper().Contains(busqueda))
                {
                    row.Visible = true;
                }
                else
                {
                    row.Visible = false;
                }
            }
        }

        private void btnlimpiarbuscador_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.IsNewRow) continue; // Evita procesar la fila nueva
                row.Visible = true;
            }
        }
    }


}
