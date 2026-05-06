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

using CapaPresentacion.Modales;
using FontAwesome.Sharp;


namespace CapaPresentacion
{
    public partial class Inicio : Form
    {
        private static string documentoUsuarioActual;
        private static IconMenuItem MenuActivo = null;
        private static Form FormularioActivo = null;

        // Cadena de conexión a tu base de datos
        private string connectionString = "Data Source=.\\SQLEXPRESS03;Initial Catalog=DBSISTEMA_INVENTARIO;Integrated Security=True";

        public Inicio(string documentoUsuario = null)
        {
            // Asigna un valor predeterminado si no se proporciona un documento
            documentoUsuarioActual = documentoUsuario ?? "ADMIN";
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }

        private void Inicio_Load(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show($"Documento del usuario actual: {documentoUsuarioActual}"); // Depuración
                                                                                            // Conectar a la base de datos y obtener los permisos del usuario
                List<string> listaPermisos = ObtenerPermisos(documentoUsuarioActual);

                foreach (IconMenuItem iconMenu in menu.Items)
                {
                    // Verifica si el ítem del menú está en la lista de permisos
                    bool encontrado = listaPermisos.Contains(iconMenu.Name);

                    // Muestra u oculta el ítem del menú basado en los permisos
                    iconMenu.Visible = encontrado;
                }

                lblusuario.Text = documentoUsuarioActual;
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Error de SQL: {sqlEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los permisos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> ObtenerPermisos(string documentoUsuario)
        {
            List<string> permisos = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Primero obtenemos el IdRol del usuario
                string queryUsuario = "SELECT IdRol FROM USUARIO WHERE Documento = @Documento";
                int idRol;

                using (SqlCommand cmdUsuario = new SqlCommand(queryUsuario, connection))
                {
                    cmdUsuario.Parameters.AddWithValue("@Documento", documentoUsuario);
                    object result = cmdUsuario.ExecuteScalar();
                    if (result != null)
                    {
                        idRol = Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new Exception($"Usuario con documento {documentoUsuario} no encontrado.");
                    }
                }

                // Ahora obtenemos los permisos según el IdRol
                string queryPermisos = "SELECT NombreMenu FROM PERMISO WHERE IdRol = @IdRol";
                using (SqlCommand cmdPermisos = new SqlCommand(queryPermisos, connection))
                {
                    cmdPermisos.Parameters.AddWithValue("@IdRol", idRol);
                    using (SqlDataReader reader = cmdPermisos.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permisos.Add(reader["NombreMenu"].ToString());
                        }
                    }
                }
            }

            return permisos;
        }

        private void AbrirFormulario(IconMenuItem menu, Form formulario)
        {
            if (MenuActivo != null)
            {
                MenuActivo.BackColor = Color.FromArgb(46, 66, 87);
            }
            menu.BackColor = Color.Silver;
            MenuActivo = menu;

            if (FormularioActivo != null)
            {
                FormularioActivo.Close();
            }

            FormularioActivo = formulario;
            formulario.TopLevel = false;
            formulario.FormBorderStyle = FormBorderStyle.None;
            formulario.Dock = DockStyle.Fill;
            formulario.BackColor = Color.FromArgb(29, 29, 32);

            contenedor.Controls.Add(formulario);
            formulario.Show();
        }

        private void menuusuarios_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconMenuItem)sender, new frmUsuarios());
        }

        private void submenucategoria_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menumantenedor, new frmCategoria());
        }

        private void submenuproducto_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menumantenedor, new frmProductos());
        }

        private void menuventas_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuventas, new frmVentas(documentoUsuarioActual));
        }

        private void menucompras_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menucompras, new frmCompras(documentoUsuarioActual));
        }

        private void menuclientes_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconMenuItem)sender, new frmClientes());
        }

        private void menuproveedores_Click_1(object sender, EventArgs e)
        {
            AbrirFormulario((IconMenuItem)sender, new frmProveedores());
        }
        private void menuinventario_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconMenuItem)sender, new Inventario());
        }

        private void submenunegocio_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menumantenedor, new frmNegocio());
        }

        private void Respaldo_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menumantenedor, new Respaldo());
        }

        private void submenureportecompras_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menureportes, new frmReporteCompras());
        }

        private void submenureporteventas_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menureportes, new frmReporteVentas());
        }

        private void DetalleCompra_Click_1(object sender, EventArgs e)
        {
            AbrirFormulario(menureportes, new frmDetalleCompra());
        }

        private void DetalleVenta_Click_1(object sender, EventArgs e)
        {
            AbrirFormulario(menureportes, new frmDetalleVenta());
        }

            private void menuacercade_Click(object sender, EventArgs e)
        {
            mdAcercade md = new mdAcercade();
            md.ShowDialog();
        }
        private void menuayuda_Click(object sender, EventArgs e)
        {
            mdAyuda md = new mdAyuda();
            md.ShowDialog();
        }
        private void btnsalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea salir?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }

      
    }
 }