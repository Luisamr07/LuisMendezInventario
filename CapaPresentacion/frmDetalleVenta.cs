using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace CapaPresentacion
{

    public partial class frmDetalleVenta : Form
    {
        private string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True";

        public frmDetalleVenta()
        {
            InitializeComponent();
            InitializeDataGridView();
        }

        private void InitializeDataGridView()
        {
            dgvdata.Columns.Clear();
            dgvdata.Columns.Add("Producto", "Producto");
            dgvdata.Columns.Add("Precio", "Precio");
            dgvdata.Columns.Add("Cantidad", "Cantidad");
            dgvdata.Columns.Add("SubTotal", "SubTotal");

            dgvdata.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void frmDetalleVenta_Load(object sender, EventArgs e)
        {
            txtbusqueda.Select();
        }

        private void btnbuscar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtbusqueda.Text))
            {
                MessageBox.Show("Por favor ingrese un número de documento para buscar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string queryVenta = "SELECT v.NumeroDocumento, v.FechaRegistro, v.TipoDocumento, u.NombreCompleto, " +
                                 "v.DocumentoCliente, v.NombreCliente, v.MontoTotal, v.MontoPago, v.MontoCambio " +
                                 "FROM VENTA v " +
                                 "INNER JOIN USUARIO u ON v.IdUsuario = u.Documento " +
                                 "WHERE v.NumeroDocumento = @NumeroDocumento";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(queryVenta, conn);
                    cmd.Parameters.AddWithValue("@NumeroDocumento", txtbusqueda.Text);
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtnumerodocumento.Text = reader["NumeroDocumento"].ToString();
                        txtfecha.Text = Convert.ToDateTime(reader["FechaRegistro"]).ToString("yyyy-MM-dd");
                        txttipodocumento.Text = reader["TipoDocumento"].ToString();
                        txtusuario.Text = reader["NombreCompleto"].ToString();
                        txtdoccliente.Text = reader["DocumentoCliente"].ToString();
                        txtnombrecliente.Text = reader["NombreCliente"].ToString();
                        txtmontototal.Text = Convert.ToDecimal(reader["MontoTotal"]).ToString("0.00");
                        txtmontopago.Text = Convert.ToDecimal(reader["MontoPago"]).ToString("0.00");
                        txtmontocambio.Text = Convert.ToDecimal(reader["MontoCambio"]).ToString("0.00");

                        // Llamada a LoadVentaDetails con NumeroDocumento como cadena
                        LoadVentaDetails(txtbusqueda.Text);
                    }
                    else
                    {
                        MessageBox.Show("No se encontró ninguna venta con el número de documento proporcionado.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnborrar_Click(null, null);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar la venta: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadVentaDetails(string numeroDocumento)
        {
            string queryDetalles = "SELECT p.Nombre, dv.PrecioVenta, dv.Cantidad, dv.SubTotal " +
                                   "FROM DETALLE_VENTA dv " +
                                   "INNER JOIN PRODUCTO p ON dv.IdProducto = p.Codigo " +
                                   "WHERE dv.IdVenta = (SELECT IdVenta FROM VENTA WHERE NumeroDocumento = @NumeroDocumento)";

            dgvdata.Rows.Clear();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(queryDetalles, conn);
                    cmd.Parameters.AddWithValue("@NumeroDocumento", numeroDocumento);
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        dgvdata.Rows.Add(
                            reader["Nombre"].ToString(),
                            Convert.ToDecimal(reader["PrecioVenta"]).ToString("0.00"),
                            Convert.ToInt32(reader["Cantidad"]).ToString(),
                            Convert.ToDecimal(reader["SubTotal"]).ToString("0.00")
                        );
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los detalles de la venta: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnborrar_Click(object sender, EventArgs e)
        {
            txtfecha.Text = "";
            txttipodocumento.Text = "";
            txtusuario.Text = "";
            txtnombrecliente.Text = "";
            dgvdata.Rows.Clear();
            txtmontototal.Text = "0.00";
            txtmontopago.Text = "0.00";
            txtmontocambio.Text = "0.00";
        }


        private void btndescargar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txttipodocumento.Text))
            {
                MessageBox.Show("No se encontraron resultados", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string textoHtml = Properties.Resources.PlantillaVenta; // Asegúrate de tener esta plantilla en los recursos del proyecto

            // Obtener datos del negocio desde la base de datos
            string nombreNegocio;
            string rifNegocio;
            string direccionNegocio;
            byte[] logoImage = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Obtener datos del negocio
                string queryNegocio = "SELECT Nombre, RIF, Direccion, Logo FROM NEGOCIO";
                using (SqlCommand cmd = new SqlCommand(queryNegocio, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        nombreNegocio = reader["Nombre"].ToString();
                        rifNegocio = reader["RIF"].ToString();
                        direccionNegocio = reader["Direccion"].ToString();
                        logoImage = reader["Logo"] as byte[];
                    }
                    else
                    {
                        // Si no hay datos del negocio, usa valores predeterminados
                        nombreNegocio = "Nombre del Negocio";
                        rifNegocio = "RIF del Negocio";
                        direccionNegocio = "Dirección del Negocio";
                    }
                    reader.Close();
                }
            }

            // Reemplazar marcadores de posición en la plantilla HTML
            textoHtml = textoHtml.Replace("@nombrenegocio", nombreNegocio.ToUpper());
            textoHtml = textoHtml.Replace("@docnegocio", rifNegocio);
            textoHtml = textoHtml.Replace("@direcnegocio", direccionNegocio);
            textoHtml = textoHtml.Replace("@tipodocumento", txttipodocumento.Text.ToUpper());
            textoHtml = textoHtml.Replace("@numerodocumento", txtnumerodocumento.Text);
            textoHtml = textoHtml.Replace("@doccliente", txtdoccliente.Text);
            textoHtml = textoHtml.Replace("@nombrecliente", txtnombrecliente.Text);
            textoHtml = textoHtml.Replace("@fecharegistro", txtfecha.Text);
            textoHtml = textoHtml.Replace("@usuarioregistro", txtusuario.Text);
            textoHtml = textoHtml.Replace("@montototal", txtmontototal.Text);
            textoHtml = textoHtml.Replace("@pagocon", txtmontopago.Text);
            textoHtml = textoHtml.Replace("@cambio", txtmontocambio.Text);

            // Construir las filas para el DataGridView
            string filas = string.Empty;
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.IsNewRow) continue;

                filas += "<tr>";
                filas += $"<td>{row.Cells["Producto"].Value}</td>";
                filas += $"<td>{row.Cells["Precio"].Value}</td>";
                filas += $"<td>{row.Cells["Cantidad"].Value}</td>";
                filas += $"<td>{row.Cells["SubTotal"].Value}</td>";
                filas += "</tr>";
            }
            textoHtml = textoHtml.Replace("@filas", filas);

            // Guardar archivo PDF
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.FileName = $"Venta_{txtnumerodocumento.Text}.pdf";
                saveFileDialog.Filter = "PDF Files|*.pdf";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                        {
                            Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 25);
                            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                            pdfDoc.Open();

                            // Opcional: agregar logo al PDF
                            if (logoImage != null)
                            {
                                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(logoImage);
                                img.ScaleToFit(60, 60);
                                img.Alignment = iTextSharp.text.Image.UNDERLYING;
                                img.SetAbsolutePosition(pdfDoc.Left, pdfDoc.GetTop(51));
                                pdfDoc.Add(img);
                            }

                            using (StringReader sr = new StringReader(textoHtml))
                            {
                                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                            }

                            pdfDoc.Close();
                            stream.Close();

                            MessageBox.Show("Documento generado con éxito.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al generar el documento: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }




        private void btnimprimir_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txttipodocumento.Text))
            {
                MessageBox.Show("No se han generado resultados para imprimir", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string textoHtml = Properties.Resources.PlantillaVenta; // Asegúrate de tener esta plantilla en los recursos del proyecto

            // Obtener datos del negocio y generar el HTML como antes
            string nombreNegocio;
            string rifNegocio;
            string direccionNegocio;
            byte[] logoImage = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string queryNegocio = "SELECT Nombre, RIF, Direccion, Logo FROM NEGOCIO";
                using (SqlCommand cmd = new SqlCommand(queryNegocio, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        nombreNegocio = reader["Nombre"].ToString();
                        rifNegocio = reader["RIF"].ToString();
                        direccionNegocio = reader["Direccion"].ToString();
                        logoImage = reader["Logo"] as byte[];
                    }
                    else
                    {
                        nombreNegocio = "Nombre del Negocio";
                        rifNegocio = "RIF del Negocio";
                        direccionNegocio = "Dirección del Negocio";
                    }
                    reader.Close();
                }
            }

            // Reemplazar marcadores de posición en la plantilla HTML
            textoHtml = textoHtml.Replace("@nombrenegocio", nombreNegocio.ToUpper());
            textoHtml = textoHtml.Replace("@docnegocio", rifNegocio);
            textoHtml = textoHtml.Replace("@direcnegocio", direccionNegocio);
            textoHtml = textoHtml.Replace("@tipodocumento", txttipodocumento.Text.ToUpper());
            textoHtml = textoHtml.Replace("@numerodocumento", txtnumerodocumento.Text);
            textoHtml = textoHtml.Replace("@doccliente", txtdoccliente.Text);
            textoHtml = textoHtml.Replace("@nombrecliente", txtnombrecliente.Text);
            textoHtml = textoHtml.Replace("@fecharegistro", txtfecha.Text);
            textoHtml = textoHtml.Replace("@usuarioregistro", txtusuario.Text);
            textoHtml = textoHtml.Replace("@montototal", txtmontototal.Text);
            textoHtml = textoHtml.Replace("@pagocon", txtmontopago.Text);
            textoHtml = textoHtml.Replace("@cambio", txtmontocambio.Text);

            // Construir las filas para el DataGridView
            string filas = string.Empty;
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.IsNewRow) continue;

                filas += "<tr>";
                filas += $"<td>{row.Cells["Producto"].Value}</td>";
                filas += $"<td>{row.Cells["Precio"].Value}</td>";
                filas += $"<td>{row.Cells["Cantidad"].Value}</td>";
                filas += $"<td>{row.Cells["SubTotal"].Value}</td>";
                filas += "</tr>";
            }
            textoHtml = textoHtml.Replace("@filas", filas);

            // Guardar archivo PDF temporal
            string rutaPdf = Path.Combine(Path.GetTempPath(), $"Venta_{txtnumerodocumento.Text}.pdf");
            using (FileStream stream = new FileStream(rutaPdf, FileMode.Create))
            {
                Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();

                // Opcional: agregar logo al PDF
                if (logoImage != null)
                {
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(logoImage);
                    img.ScaleToFit(60, 60);
                    img.Alignment = iTextSharp.text.Image.UNDERLYING;
                    img.SetAbsolutePosition(pdfDoc.Left, pdfDoc.GetTop(51));
                    pdfDoc.Add(img);
                }

                using (StringReader sr = new StringReader(textoHtml))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                }

                pdfDoc.Close();
                stream.Close();
            }

            // Mostrar el PDF en una ventana emergente
            var pdfViewerForm = new PdfViewerForm(rutaPdf);
            pdfViewerForm.ShowDialog();
        }



    }

}
