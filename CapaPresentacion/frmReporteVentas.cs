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
using PdfiumViewer;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

// Alias para iText
using iTextPdfDocument = iText.Kernel.Pdf.PdfDocument;

// Alias para PdfiumViewer
using PdfiumPdfDocument = PdfiumViewer.PdfDocument;


namespace CapaPresentacion
{

    public partial class frmReporteVentas : Form
    {
        private string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True";

        public frmReporteVentas()
        {
            InitializeComponent();
            dgvdata.CellClick += dgvdata_CellClick;
            dgvdata.CellMouseEnter += dgvdata_CellMouseEnter;
            dgvdata.CellMouseLeave += dgvdata_CellMouseLeave;
        }

        private void frmReporteVentas_Load(object sender, EventArgs e)
        {
            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                cbobusqueda.Items.Add(new { Valor = columna.Name, Texto = columna.HeaderText });
            }
            cbobusqueda.DisplayMember = "Texto";
            cbobusqueda.ValueMember = "Valor";
            cbobusqueda.SelectedIndex = 0;
        }

        private void btnbuscarreporte_Click(object sender, EventArgs e)
        {
            string fechainicio = txtfechainicio.Value.Date.ToString("yyyy-MM-dd");
            string fechafin = txtfechafin.Value.Date.AddDays(1).AddTicks(-1).ToString("yyyy-MM-dd HH:mm:ss");

            // Ajusta la consulta SQL según la estructura de la base de datos
            string query = "SELECT v.FechaRegistro AS RegistrationDate, " +
                           "v.TipoDocumento AS DocumentType, " +
                           "v.NumeroDocumento AS DocumentNumber, " +
                           "v.MontoTotal AS TotalAmount, " +
                           "v.IdUsuario AS UsuarioRegistro, " + // Columna añadida
                           "v.DocumentoCliente AS ClientDocument, " +
                           "v.NombreCliente AS ClientName, " +
                           "p.Codigo AS ProductCode, " +
                           "p.Nombre AS ProductName, " +
                           "c.Descripcion AS Category, " +
                           "dv.PrecioVenta AS RentalPrice, " +
                           "dv.Cantidad AS Quantity, " +
                           "dv.SubTotal AS SubTotal " +
                           "FROM VENTA v " +
                           "INNER JOIN DETALLE_VENTA dv ON v.IdVenta = dv.IdVenta " +
                           "INNER JOIN PRODUCTO p ON dv.IdProducto = p.Codigo " +
                           "INNER JOIN CATEGORIA c ON p.IdCategoria = c.IdCategoria " +
                           "WHERE v.FechaRegistro BETWEEN @fechainicio AND @fechafin";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@fechainicio", fechainicio);
                cmd.Parameters.AddWithValue("@fechafin", fechafin);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                dgvdata.Rows.Clear();

                while (reader.Read())
                {
                    dgvdata.Rows.Add(new object[]
                    {
                reader["RegistrationDate"],
                reader["DocumentType"],
                reader["DocumentNumber"],
                reader["TotalAmount"],
                reader["UsuarioRegistro"], // Columna añadida
                reader["ClientDocument"],
                reader["ClientName"],
                reader["ProductCode"],
                reader["ProductName"],
                reader["Category"],
                reader["RentalPrice"],
                reader["Quantity"],
                reader["SubTotal"]
                    });
                }
            }
        }



        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string columnaFiltro = ((dynamic)cbobusqueda.SelectedItem).Valor.ToString();

            if (dgvdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    if (row.Cells[columnaFiltro].Value != null &&
                        row.Cells[columnaFiltro].Value.ToString().Trim().ToUpper().Contains(txtbusqueda.Text.Trim().ToUpper()))
                    {
                        row.Visible = true;
                    }
                    else
                    {
                        row.Visible = false;
                    }
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
                FileName = $"ReporteVentas_{DateTime.Now:ddMMyyyyHHmmss}.xlsx",
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

                        // Mover los datos a partir de la fila 6
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

}
