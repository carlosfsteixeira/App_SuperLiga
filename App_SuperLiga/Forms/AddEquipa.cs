using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace App_SuperLiga
{
    public partial class AddEquipa : Form
    {
        DataClasses1DataContext dc;

        Dashboard form2;

        Equipa novaEquipa;

        public AddEquipa(Dashboard form)
        {
            dc = new DataClasses1DataContext();

            InitializeComponent();

            form2 = form;

            novaEquipa = new Equipa();
        }

        private void lbl_AddImagem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog open = new OpenFileDialog())
            {
                open.InitialDirectory = @"C:\Pictures";
                open.Filter = "Image Files(*.jpg; *.jpeg; *.png;) | *.jpg; *.jpeg; *.png;";
                open.ValidateNames = true;
                open.Multiselect = false;

                if (open.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = new Bitmap(open.FileName);
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        private void SubmeterImagemBD()
        {
            var maxIdQuery = from Imagen in dc.Imagens
                             orderby Imagen.id_imagem descending
                             select Imagen.id_imagem;

            int maxImgID;

            if (maxIdQuery.Count() == 0)
            {
                maxImgID = 0;
            }
            else
            {
                maxImgID = maxIdQuery.Max();
            }

            // Converter System.Drawing.Image para byte[]
            byte[] file_byte = ImageToByteArray(pictureBox1.Image);

            // Criar uma System.Data.Linq.Binary 
            System.Data.Linq.Binary file_binary = new System.Data.Linq.Binary(file_byte);
            
            Imagen img = new Imagen
            {
                id_imagem = (maxImgID + 1),
                imagem = file_binary,
                id_equipa = novaEquipa.id_equipa,
            };

            //adicionar nova Equipa a bd
            dc.Imagens.InsertOnSubmit(img);

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public byte[] ImageToByteArray(System.Drawing.Image imagem)
        {
            using (var ms = new MemoryStream())
            {
                imagem.Save(ms, imagem.RawFormat);
                return ms.ToArray();
            }
        }

        private void lbl_Confirmar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNomeEquipa.Text) || string.IsNullOrEmpty(txtEstadio.Text) || pictureBox1.Image == null)
            {
                MessageBox.Show("Existem campos por preencher", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                var maxIdQuery = from Equipa in dc.Equipas
                                 orderby Equipa.id_equipa descending
                                 select Equipa.id_equipa;

                int maxEquipaID;

                if (maxIdQuery.Count() == 0)
                {
                    maxEquipaID = 0;
                }
                else
                {
                    maxEquipaID = maxIdQuery.Max();
                }

                novaEquipa.id_equipa = (maxEquipaID + 1);
                novaEquipa.nome = txtNomeEquipa.Text;
                novaEquipa.estadio = txtEstadio.Text;

                //adicionar nova Equipa a bd
                dc.Equipas.InsertOnSubmit(novaEquipa);

                try
                {
                    dc.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                SubmeterImagemBD();

                form2.RefreshAllGrids();
                form2.RefreshTeamGrid();

                MessageBox.Show("Adicionada com sucesso", "Nova Equipa");

                this.Close();
            }
        }

        private void lbl_fechar_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
