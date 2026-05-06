using DocumentFormat.OpenXml.Drawing.Charts;
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

namespace CapaPresentacion
{
    public partial class Inventario : Form
    {
        public Inventario()
        {
            InitializeComponent();
            // Suscribirse al evento CellFormatting del DataGridView
            dgvdata.CellFormatting += Dgvdata_CellFormatting;
        }

        private void Inventario_Load(object sender, EventArgs e)
        {
            // Cargar datos iniciales en el DataGridView
            CargarDatos();

            // Configurar ComboBox
            cbobusqueda.Items.Add("Código");
            cbobusqueda.Items.Add("Nombre");
            cbobusqueda.SelectedIndex = 0; // Seleccionar un ítem por defecto si lo deseas
        }

        private void CargarDatos(string filtro = "", string busqueda = "")
        {
            // Cadena de conexión a la base de datos
            string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True";
            // Consulta SQL para obtener los datos de inventario con el filtro aplicado
            string query = "SELECT Codigo AS 'Código del Producto', Nombre AS 'Nombre del Producto', Stock AS 'Cantidad en Stock' FROM PRODUCTO WHERE Estado = 1";

            // Agregar el filtro a la consulta si existe
            if (!string.IsNullOrEmpty(filtro))
            {
                query += filtro;
            }

            // Crear un DataTable para almacenar los datos
            System.Data.DataTable dataTable = new System.Data.DataTable();

            try
            {
                // Conectar a la base de datos y llenar el DataTable
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);

                    // Agregar el parámetro si se proporciona un término de búsqueda
                    if (!string.IsNullOrEmpty(busqueda))
                    {
                        command.Parameters.AddWithValue("@busqueda", $"%{busqueda}%");
                    }

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    dataAdapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos: {ex.Message}");
            }

            // Asignar el DataTable al DataGridView
            dgvdata.DataSource = dataTable;

            // Verificar stock y mostrar mensaje de alerta si es necesario
            VerificarStock(dataTable);
        }

        private void VerificarStock(System.Data.DataTable dataTable)
        {
            bool alertaMostrada = false;

            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    int stock = Convert.ToInt32(row["Cantidad en Stock"]);

                    if (stock < 5)
                    {
                        if (!alertaMostrada)
                        {
                            MessageBox.Show("Tienes productos que se están agotando por debajo de la cantidad mínima.");
                            alertaMostrada = true;
                        }
                        break; // Solo mostrar el mensaje una vez
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al procesar el stock: {ex.Message}");
                }
            }
        }


        private void Dgvdata_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvdata.Columns[e.ColumnIndex].HeaderText == "Cantidad en Stock" && e.RowIndex >= 0)
            {
                int stock;
                if (int.TryParse(e.Value?.ToString(), out stock))
                {
                    if (stock <= 5)
                    {
                        dgvdata.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (stock < 10)
                    {
                        dgvdata.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                    }
                    else
                    {
                        dgvdata.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LawnGreen;
                    }
                }
                else
                {
                    e.CellStyle.BackColor = Color.White; // Color predeterminado si el stock no es un número válido
                }
            }
        }


        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string filtro = "";
            string busqueda = txtbusqueda.Text.Trim();

            // Verificar si el ComboBox tiene una selección válida
            if (cbobusqueda.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un campo de búsqueda.");
                return;
            }

            // Obtener el campo de búsqueda seleccionado
            string campoBusqueda = cbobusqueda.SelectedItem.ToString();

            // Construir el filtro basado en el campo de búsqueda y el texto ingresado
            if (!string.IsNullOrEmpty(busqueda))
            {
                switch (campoBusqueda)
                {
                    case "Código":
                        filtro = " AND Codigo LIKE @busqueda";
                        break;
                    case "Nombre":
                        filtro = " AND Nombre LIKE @busqueda";
                        break;
                    default:
                        MessageBox.Show("Campo de búsqueda no válido.");
                        return; // Salir del método para evitar la búsqueda con un campo no válido
                }
            }

            // Llamar al método para llenar la tabla con el filtro aplicado
            CargarDatos(filtro, busqueda);
        }
    }
}
