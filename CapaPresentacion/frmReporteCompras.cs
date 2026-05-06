using CapaPresentacion.Utilidades;
using ClosedXML.Excel;
using System;
using System.IO;
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

    public partial class frmReporteCompras : Form
    {
        private string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True";

        public frmReporteCompras()
        {
            InitializeComponent();
            dgvdata.CellClick += dgvdata_CellClick;
            dgvdata.CellMouseEnter += dgvdata_CellMouseEnter;
            dgvdata.CellMouseLeave += dgvdata_CellMouseLeave;
        }

        private void frmReporteCompras_Load(object sender, EventArgs e)
        {
            CargarProveedores();
            CargarBusquedaColumnas();
        }

        private void CargarProveedores()
        {
            string query = "SELECT Documento, RazonSocial FROM PROVEEDOR WHERE Estado = 1";
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                cboproveedor.Items.Add(new OpcionCombo() { Valor = "0", Texto = "TODOS" });
                while (reader.Read())
                {
                    cboproveedor.Items.Add(new OpcionCombo() { Valor = reader["Documento"].ToString(), Texto = reader["RazonSocial"].ToString() });
                }
                cboproveedor.DisplayMember = "Texto";
                cboproveedor.ValueMember = "Valor";
                cboproveedor.SelectedIndex = 0;
            }
        }

        private void CargarBusquedaColumnas()
        {
            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = columna.Name, Texto = columna.HeaderText });
            }
            cbobusqueda.DisplayMember = "Texto";
            cbobusqueda.ValueMember = "Valor";
            cbobusqueda.SelectedIndex = 0;
        }

        private void btnbuscarresultado_Click(object sender, EventArgs e)
        {
            string idProveedor = ((OpcionCombo)cboproveedor.SelectedItem).Valor.ToString();
            string fechaInicio = txtfechainicio.Value.ToString("yyyy-MM-dd");
            string fechaFin = txtfechafin.Value.Date.AddDays(1).AddTicks(-1).ToString("yyyy-MM-dd HH:mm:ss");

            string query = @"
        SELECT c.FechaRegistro, c.TipoDocumento, c.NumeroDocumento, c.MontoTotal, u.NombreCompleto AS UsuarioRegistro, 
               p.Documento AS DocumentoProveedor, p.RazonSocial, dc.IdProducto AS CodigoProducto, pr.Nombre AS NombreProducto, 
               ca.Descripcion AS Categoria, dc.PrecioCompra, dc.PrecioVenta, dc.Cantidad, dc.MontoTotal AS SubTotal
        FROM COMPRA c
        INNER JOIN USUARIO u ON c.IdUsuario = u.Documento
        INNER JOIN PROVEEDOR p ON c.IdProveedor = p.Documento
        INNER JOIN DETALLE_COMPRA dc ON c.IdCompra = dc.IdCompra
        INNER JOIN PRODUCTO pr ON dc.IdProducto = pr.Codigo
        INNER JOIN CATEGORIA ca ON pr.IdCategoria = ca.IdCategoria
        WHERE c.FechaRegistro BETWEEN @FechaInicio AND @FechaFin
        AND (@IdProveedor = '0' OR c.IdProveedor = @IdProveedor)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                cmd.Parameters.AddWithValue("@IdProveedor", idProveedor);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                dgvdata.Rows.Clear();

                while (reader.Read())
                {
                    dgvdata.Rows.Add(new object[] {
                    reader["FechaRegistro"],
                    reader["TipoDocumento"],
                    reader["NumeroDocumento"],
                    reader["MontoTotal"],
                    reader["UsuarioRegistro"],
                    reader["DocumentoProveedor"],
                    reader["RazonSocial"],
                    reader["CodigoProducto"],
                    reader["NombreProducto"],
                    reader["Categoria"],
                    reader["PrecioCompra"],
                    reader["PrecioVenta"],
                    reader["Cantidad"],
                    reader["SubTotal"]
                });
                }
            }
        }

        private void btnexportar_Click(object sender, EventArgs e)
        {
            if (dgvdata.Rows.Count < 1)
            {
                MessageBox.Show("No hay registros para exportar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DataTable dt = new DataTable();

            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                dt.Columns.Add(columna.HeaderText, typeof(string));
            }

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.Visible)
                {
                    dt.Rows.Add(row.Cells.Cast<DataGridViewCell>().Select(cell => cell.Value?.ToString()).ToArray());
                }
            }

            SaveFileDialog savefile = new SaveFileDialog
            {
                FileName = $"ReporteCompras_{DateTime.Now:ddMMyyyyHHmmss}.xlsx",
                Filter = "Excel Files | *.xlsx"
            };

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var hoja = wb.Worksheets.Add("Informe");

                        // Ruta relativa desde el directorio del proyecto
                        var projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        var logoPath = Path.Combine(projectDirectory, @"..\..\Resources\BannerReporte.png");

                        // Resolver la ruta a la dirección correcta
                        logoPath = Path.GetFullPath(logoPath);

                        // Verificar si el archivo de imagen existe
                        if (!File.Exists(logoPath))
                        {
                            MessageBox.Show($"El archivo de imagen no se encuentra en la ruta: {logoPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Añadir el logo
                        var logo = hoja.AddPicture(logoPath).MoveTo(hoja.Cell("A1"));

                        // Ajustar el tamaño de la imagen
                        logo.Width = 300; // Ajusta el ancho del logo (en puntos)
                        logo.Height = 95; // Ajusta la altura del logo (en puntos)


                        // Mover los datos a partir de la fila 3
                        hoja.Cell("A6").InsertTable(dt);

                        // Ajustar el ancho de las columnas para que el contenido sea visible
                        hoja.Columns().AdjustToContents();

                        wb.SaveAs(savefile.FileName);
                    }
                    MessageBox.Show("Reporte Generado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    // Mostrar detalles del error
                    MessageBox.Show($"Error al generar reporte: {ex.Message}", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }




        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();

            if (dgvdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    if (row.Cells[columnaFiltro]?.Value?.ToString().Trim().ToUpper().Contains(txtbusqueda.Text.Trim().ToUpper()) == true)
                        row.Visible = true;
                    else
                        row.Visible = false;
                }
            }
        }

        private void btnlimpiarbuscador_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                row.Visible = true;
            }
        }


        private void dgvdata_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (dgvdata.Columns[e.ColumnIndex].Name == "NumeroDocumento")
                {
                    foreach (DataGridViewRow row in dgvdata.Rows)
                    {
                        row.Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(14, 112, 243);
                        row.Cells[e.ColumnIndex].Style.ForeColor = Color.White;
                    }
                }
            }
        }

        private void dgvdata_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (dgvdata.Columns[e.ColumnIndex].Name == "NumeroDocumento")
                {
                    foreach (DataGridViewRow row in dgvdata.Rows)
                    {
                        row.Cells[e.ColumnIndex].Style.BackColor = Color.White; // O el color de fondo original de la columna
                        row.Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    }
                }
            }
        }


        private void dgvdata_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verifica que la fila y columna son válidas y que la columna es la de NumeroDocumento
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Aquí se asume que la columna de NumeroDocumento es la tercera (índice 2)
                if (dgvdata.Columns[e.ColumnIndex].Name == "NumeroDocumento")
                {
                    // Obtén el valor de la celda
                    string numeroDocumento = dgvdata.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();

                    if (!string.IsNullOrEmpty(numeroDocumento))
                    {
                        // Copia el valor al portapapeles
                        Clipboard.SetText(numeroDocumento);
                        MessageBox.Show("Número de Documento copiado al portapapeles.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }



    }

    public class OpcionCombo
    {
        public object Valor { get; set; }
        public string Texto { get; set; }
    }


}