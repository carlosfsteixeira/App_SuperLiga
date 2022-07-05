using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace App_SuperLiga
{
    public partial class Form2 : Form
    {
        DataClasses1DataContext dc;

        Equipa equipa;

        Jogo jogoSelecionado;

        List<Equipa> listaEquipas;

        List<Equipa> listaEquipasReverse;

        int contadorJornadas;
        int contadorResultados;
        bool alterado = false;
        DateTime dataJornada;

        public Form2()
        {
            dc = new DataClasses1DataContext();
            equipa = new Equipa();
            listaEquipas = new List<Equipa>();
            listaEquipasReverse = new List<Equipa>();
            jogoSelecionado = new Jogo();

            InitializeComponent();

            contadorJornadas = 1;
            contadorResultados = 1;
            dataJornada = DateTime.Now.AddDays(20);

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            lbldatetime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            HidePanels();

            DataGridViewEquipaShow();

            //Chamar metodo ADICIONAR METODO DE MOSTRAR CALENDARIO
            var pesquisaJogosBD = from Jogo in dc.Jogos
                                  select Jogo;

            var pesquisaJornadasBD = from Jornada in dc.Jornadas
                                     select Jornada;

            if (pesquisaJogosBD.Count() != 0 && pesquisaJornadasBD.Count() != 0)
            {
                //mostrar dados já existentes em bd do calendario
                DataGridViewJogosShow();
                DataGridViewResultadosShow();
                DataGridViewResultadosShowData();

                btGerarJogos.Enabled = false;
            }
            else
            {
                btGerarJogos.Enabled = true;
            }
        }

        #region Open&Close_Panels
        private void btEquipas_Click(object sender, EventArgs e)
        {
            panelEquipas.Visible = true;
            panelJogos.Visible = false;
            panelClassificacao.Visible = false;
            panelEstatisticas.Visible = false;
        }

        private void btJogos_Click(object sender, EventArgs e)
        {
            panelJogos.Visible = true;
            panelEquipas.Visible = false;
            panelClassificacao.Visible = false;
            panelEstatisticas.Visible = false;
        }

        private void btClassificacao_Click(object sender, EventArgs e)
        {
            panelClassificacao.Visible = true;
            panelEquipas.Visible = false;
            panelJogos.Visible = false;
            panelEstatisticas.Visible = false;

            dataGridViewClassificacao.Rows.Clear();
            DataGridViewClassificaoShow();
        }

        private void btEstatisticas_Click(object sender, EventArgs e)
        {
            panelEstatisticas.Visible = true;
            panelEquipas.Visible = false;
            panelClassificacao.Visible = false;
            panelJogos.Visible = false;

            LimparLabelsEstatisticas();
            MostrarEstatisticas();
        }

        private void HidePanels()
        {
            panelEquipas.Visible = false;
            panelJogos.Visible = false;
            panelClassificacao.Visible = false;
            panelEstatisticas.Visible = false;
        }

        #endregion

        #region PanelEquipas



        #region CRUD_BD

        public void DataGridViewEquipaShow()
        {
            this.dataGridViewEquipas.DefaultCellStyle.ForeColor = Color.White;

            dataGridViewEquipas.Columns.Add("colID", "");
            dataGridViewEquipas.Columns.Add("colNome", "Nome");
            dataGridViewEquipas.Columns[0].Visible = false;

            var listaEquipas = from Equipa in dc.Equipas select Equipa;

            int linha = 0;

            foreach (Equipa equipa in listaEquipas)
            {
                DataGridViewRow eqp = new DataGridViewRow();
                dataGridViewEquipas.Rows.Add(eqp);

                dataGridViewEquipas.Rows[linha].Cells[0].Value = equipa.id_equipa;
                dataGridViewEquipas.Rows[linha].Cells[1].Value = equipa.nome;

                linha++;
            }

            dataGridViewEquipas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void dataGridViewEquipas_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int id;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dataGridViewEquipas.Rows[e.RowIndex];

                //guarda o id da equipa selecionada na datagrid
                id = Convert.ToInt16(row.Cells[0].Value);
                txtNomeEquipa.Text = row.Cells[1].Value.ToString();

                //preenche os dados do estadio da Equipa no Info
                var pesquisaEquipa = from Equipa in dc.Equipas
                                     where Equipa.id_equipa == id
                                     select Equipa;

                equipa = pesquisaEquipa.Single();

                txtEstadio.Text = equipa.estadio.ToString();

                //preenche os dados do Treinador e Presidente no Info
                var listaStaff = from Staff in dc.Staffs
                                 where Staff.id_equipa == id
                                 select Staff;

                foreach (var s in listaStaff)
                {
                    if (s.funcao == "Treinador")
                    {
                        txtTreinador.Text = s.nome.ToString();

                    }

                    if (s.funcao == "Presidente")
                    {
                        txtPresidente.Text = s.nome.ToString();
                    }
                }

                var imagemEquipa = from Imagen in dc.Imagens
                                   where Imagen.id_equipa == id
                                   select Imagen.imagem;

                Image m = (Bitmap)((new ImageConverter()).ConvertFrom(imagemEquipa.Single().ToArray()));

                pictureBox2.Image = m;

                RefreshAllGrids();
            }

        }

        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            Image returnImage = null;
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                returnImage = Image.FromStream(ms);
            }
            return returnImage;
        }

        private void DataGridViewStaffShow(int id)
        {
            this.dataGridViewStaff.DefaultCellStyle.ForeColor = Color.White;

            //limpa o datagrid do Staff da ultima selecao do user
            dataGridViewStaff.Columns.Clear();

            //preenche a datagrid Staff
            dataGridViewStaff.Columns.Add("colId", "ID");
            dataGridViewStaff.Columns.Add("colNome", "Nome");
            dataGridViewStaff.Columns.Add("colFuncao", "Função");
            dataGridViewStaff.Columns[0].Visible = false;

            var listaStaff = from Staff in dc.Staffs
                             where Staff.id_equipa == id
                             select Staff;

            int linha = 0;

            foreach (Staff membro in listaStaff)
            {
                DataGridViewRow mb = new DataGridViewRow();
                dataGridViewStaff.Rows.Add(mb);

                dataGridViewStaff.Rows[linha].Cells[0].Value = membro.id_staff;
                dataGridViewStaff.Rows[linha].Cells[1].Value = membro.nome;
                dataGridViewStaff.Rows[linha].Cells[2].Value = membro.funcao;

                linha++;
            }

            dataGridViewStaff.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void dataGridViewStaff_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                //obter um conjunto que contem todas as linhas
                DataGridViewRow row = this.dataGridViewStaff.Rows[e.RowIndex];
                //popular as textbox de acordo com as linhas e colunas
                txtIdStaff.Text = row.Cells[0].Value.ToString();
                txtNomeStaff.Text = row.Cells[1].Value.ToString();
                comboBoxFuncao.Text = row.Cells[2].Value.ToString();
            }
        }

        private void dataGridViewJogadoresShow(int id)
        {
            this.dataGridViewJogadores.DefaultCellStyle.ForeColor = Color.White;

            dataGridViewJogadores.Columns.Clear();
            dataGridViewJogadores.Columns.Add("colId", "ID");
            dataGridViewJogadores.Columns.Add("colNome", "Nome");
            dataGridViewJogadores.Columns.Add("colNumero", "Numero");
            dataGridViewJogadores.Columns.Add("colPosicao", "Posição");
            dataGridViewJogadores.Columns[0].Visible = false;

            var listaJogadores = from Jogadore in dc.Jogadores
                                 where Jogadore.id_equipa == id
                                 select Jogadore;

            int linha = 0;

            foreach (Jogadore jogador in listaJogadores)
            {
                DataGridViewRow jg = new DataGridViewRow();
                dataGridViewJogadores.Rows.Add(jg);

                dataGridViewJogadores.Rows[linha].Cells[0].Value = jogador.id_jogador;
                dataGridViewJogadores.Rows[linha].Cells[1].Value = jogador.nome;
                dataGridViewJogadores.Rows[linha].Cells[2].Value = jogador.numero;
                dataGridViewJogadores.Rows[linha].Cells[3].Value = jogador.posicao;

                linha++;
            }

            dataGridViewJogadores.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void dataGridViewJogadores_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dataGridViewJogadores.Rows[e.RowIndex];
                txtIdJogador.Text = row.Cells[0].Value.ToString();
                txtNomeJogador.Text = row.Cells[1].Value.ToString();
                comboBoxNumCam.Text = row.Cells[2].Value.ToString();
                comboBoxPosicao.Text = row.Cells[3].Value.ToString();
            }
        }

        private void TreeViewPlantelShow(int id)
        {
            treeViewPlantel.Nodes.Clear();

            //preencher os nodes da treeview com cada uma das posicoes possiveis
            var posicoes = dc.Jogadores.GroupBy(c => c.posicao);

            foreach (IGrouping<string, Jogadore> grupoPosicao in posicoes)
            {
                treeViewPlantel.Nodes.Add(grupoPosicao.Key);
            }

            //preencher os nodes da treeview com os jogadores de cada posicao (em cada node correspondente)
            var jogadores = from Jogadore in dc.Jogadores
                            where Jogadore.id_equipa == id
                            select Jogadore;

            foreach (Jogadore j in jogadores)
            {
                foreach (TreeNode node in treeViewPlantel.Nodes)
                {
                    if (node.Text == j.posicao)
                    {
                        node.Nodes.Add($"{j.nome}");
                    }
                }
            }
        }

        private void TreeViewStaffShow(int id)
        {
            treeViewStaff.Nodes.Clear();

            //preencher os nodes da treeview com cada uma das posicoes possiveis
            var funcoes = dc.Staffs.GroupBy(c => c.funcao);

            foreach (IGrouping<string, Staff> grupoFuncao in funcoes)
            {
                treeViewStaff.Nodes.Add(grupoFuncao.Key);
            }

            //preencher os nodes da treeview com os jogadores de cada posicao (em cada node correspondente)
            var membros = from Staff in dc.Staffs
                          where Staff.id_equipa == id
                          select Staff;

            foreach (Staff m in membros)
            {
                foreach (TreeNode node in treeViewStaff.Nodes)
                {
                    if (node.Text == m.funcao)
                    {
                        node.Nodes.Add($"{m.nome}");
                    }
                }
            }
        }

        private void btAddEquipa_Click(object sender, EventArgs e)
        {
            if (ContarEquipas())
            {
                // validar se existem 6 equipas. Se sim, é necessario eliminar uma delas antes de prosseguir para o form5
                MessageBox.Show("Competição com máximo de 6 equipas", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                Form5 form5 = new Form5(this);
                form5.ShowDialog();
            }
        }

        private void lbl_AddStaff_Click(object sender, EventArgs e)
        {
            // validar se existem 50 elementos no staff. Se sim, é necessario eliminar um deles antes de prosseguir para o form3
            if (ContarStaff())
            {
                MessageBox.Show("Capacidade máxima de elementos atingida", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                Form3 form3 = new Form3(this, equipa.id_equipa);
                form3.ShowDialog();
            }
        }

        private void lbl_AddJogador_Click(object sender, EventArgs e)
        {
            // validar se existem 25 jogadores. Se sim, é necessario eliminar um deles antes de prosseguir para o form4
            if (ContarJogadores())
            {
                MessageBox.Show("Plantel com numero máximo de 25 jogadores", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                Form4 form4 = new Form4(this, equipa.id_equipa);
                form4.ShowDialog();
            }
        }

        private void lbl_RemoverEquipa_Click(object sender, EventArgs e)
        {
            // ELIMINAR EQUIPA DA BD
            DialogResult dialogResult = MessageBox.Show("Esta acção removerá tambem todo o Staff e Jogadores\n\nTem a certeza?", "Eliminar equipa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                var staffEliminar = from Staff in dc.Staffs
                                    where Staff.id_equipa == equipa.id_equipa
                                    select Staff;

                foreach (var staff in staffEliminar)
                {
                    dc.Staffs.DeleteOnSubmit(staff);
                }

                var jogadoresEliminar = from Jogadore in dc.Jogadores
                                        where Jogadore.id_equipa == equipa.id_equipa
                                        select Jogadore;

                foreach (var jogador in jogadoresEliminar)
                {
                    dc.Jogadores.DeleteOnSubmit(jogador);
                }

                var imagemEliminar = from Imagen in dc.Imagens
                                     where Imagen.id_equipa == equipa.id_equipa
                                     select Imagen;

                foreach (var imagem in imagemEliminar)
                {
                    dc.Imagens.DeleteOnSubmit(imagem);
                }


                Equipa x = new Equipa();

                var equipaEliminar = from Equipa in dc.Equipas
                                     where Equipa.id_equipa == equipa.id_equipa
                                     select Equipa;

                x = equipaEliminar.Single();

                dc.Equipas.DeleteOnSubmit(x);

                try
                {
                    dc.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                dataGridViewEquipas.Columns.Clear();
                DataGridViewEquipaShow();

                RefreshAllGrids();

                txtNomeEquipa.ResetText();
                txtEstadio.ResetText();
                txtTreinador.ResetText();
                txtPresidente.ResetText();
                pictureBox2.Image = null;

            }
            else
            {
                return;
            }
        }

        private void lbl_RemoverStaff_Click(object sender, EventArgs e)
        {
            int IDaRemover = Convert.ToInt32(txtIdStaff.Text.ToString());
            string name = txtNomeStaff.Text;

            // ELIMINAR MEMBRO DO STAFF DA BD
            DialogResult dialogResult = MessageBox.Show($"Eliminar {name} do Staff?", "Eliminar elemento do Staff", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                Staff x = new Staff();

                var pesquisa = from Staff in dc.Staffs
                               where Staff.id_staff == IDaRemover
                               select Staff;

                x = pesquisa.Single();

                dc.Staffs.DeleteOnSubmit(x);

                try
                {
                    dc.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                MessageBox.Show("Removido com sucesso");

                RefreshAllGrids();

                txtIdStaff.ResetText();
                txtNomeStaff.ResetText();
                comboBoxFuncao.ResetText();

            }
            else
            {
                return;
            }
        }

        private void lbl_RemoverJogador_Click(object sender, EventArgs e)
        {
            int IDaRemover = Convert.ToInt32(txtIdJogador.Text.ToString());
            string name = txtNomeJogador.Text;

            // ELIMINAR JOGADOR DA BD
            DialogResult dialogResult = MessageBox.Show($"Eliminar {name} do plantel?", "Eliminar jogador", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                Jogadore x = new Jogadore();

                var pesquisa = from Jogadore in dc.Jogadores
                               where Jogadore.id_jogador == IDaRemover
                               select Jogadore;

                x = pesquisa.Single();

                dc.Jogadores.DeleteOnSubmit(x);

                try
                {
                    dc.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                RefreshAllGrids();

                txtIdJogador.ResetText();
                txtNomeJogador.ResetText();
                comboBoxNumCam.ResetText();
                comboBoxPosicao.ResetText();

            }
            else
            {
                return;
            }
        }

        private void lbl_UpdateEquipa_Click(object sender, EventArgs e)
        {
            if (!ValidarInfoEquipa())
            {
                return;
            }

            if (!txtNomeEquipa_TextCheck())
            {
                return;
            }

            if (!txtEstadio_TextCheck())
            {
                return;
            }

            equipa.nome = txtNomeEquipa.Text;
            equipa.estadio = txtEstadio.Text;


            try
            {
                dc.SubmitChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            RefreshAllGrids();
            RefreshTeamGrid();
        }

        private void lbl_UpgradeJogador_Click(object sender, EventArgs e)
        {

            if (!ValidarInfoJogadores() || !txtNomeJogador_TextCheck())
            {
                return;
            }


            // SALVAR ALTERAÇOES AO JOGADOR NA BASE DADOS
            int idJogador = Convert.ToInt32(txtIdJogador.Text.ToString());

            Jogadore jogadorEditado = new Jogadore();

            var pesquisa = from Jogadore in dc.Jogadores
                           where Jogadore.id_jogador == idJogador
                           select Jogadore;

            jogadorEditado = pesquisa.Single();

            if (jogadorEditado.nome != txtIdJogador.Text)
            {
                jogadorEditado.nome = txtNomeJogador.Text;
            }

            if (comboBoxPosicao.SelectedItem.ToString() != jogadorEditado.posicao)
            {
                if (CheckNumeroCamisola())
                {
                    return;
                }
                else
                {
                    jogadorEditado.numero = Convert.ToInt32(comboBoxNumCam.SelectedItem);
                }

                jogadorEditado.posicao = comboBoxPosicao.SelectedItem.ToString();
            }

            try
            {
                dc.SubmitChanges();
                MessageBox.Show("Alterado com sucesso");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            RefreshAllGrids();
        }

        private void lbl_UpdateStaff_Click(object sender, EventArgs e)
        {
            if (!ValidarInfoStaff() || !txtNomeStaff_TextCheck())
            {
                return;
            }

            // SALVAR ALTERAÇOES AO STAFF NA BASE DADOS
            int idStaff = Convert.ToInt32(txtIdStaff.Text.ToString());

            Staff staffEditado = new Staff();

            var pesquisa = from Staff in dc.Staffs
                           where Staff.id_staff == idStaff
                           select Staff;

            staffEditado = pesquisa.Single();

            if (staffEditado.nome != txtNomeStaff.Text)
            {
                staffEditado.nome = txtNomeStaff.Text;
            }


            if (comboBoxFuncao.SelectedItem.ToString() != staffEditado.funcao)
            {
                if (comboBoxFuncao.SelectedItem.ToString() == "Treinador")
                {
                    if (ValidarExistenciaTreinador())
                    {
                        MessageBox.Show("Ja existe um Treinador", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (comboBoxFuncao.SelectedItem.ToString() == "Presidente")
                {
                    if (ValidarExistenciaPresidente())
                    {
                        MessageBox.Show("Ja existe um Presidente", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                staffEditado.funcao = comboBoxFuncao.SelectedItem.ToString();
            }


            try
            {
                dc.SubmitChanges();
                MessageBox.Show("Alterado com sucesso");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            RefreshAllGrids();
            RefreshTeamGrid();
        }

        public bool ValidarExistenciaTreinador()
        {
            bool output = false;

            var procurarTreinador = from Staff in dc.Staffs
                                    where Staff.funcao == "Treinador"
                                    && Staff.id_equipa == equipa.id_equipa
                                    select Staff;

            if (procurarTreinador.Count() == 1)
            {
                output = true;
            }


            return output;
        }

        public bool ValidarExistenciaPresidente()
        {
            bool output = false;

            var procurarPresidente = from Staff in dc.Staffs
                                     where Staff.funcao == "Presidente"
                                     && Staff.id_equipa == equipa.id_equipa
                                     select Staff;

            if (procurarPresidente.Count() == 1)
            {

                output = true;
            }

            return output;
        }

        public void RefreshAllGrids()
        {
            dataGridViewJogadores.Columns.Clear();
            dataGridViewJogadoresShow(equipa.id_equipa);

            treeViewPlantel.Nodes.Clear();
            TreeViewPlantelShow(equipa.id_equipa);

            dataGridViewStaff.Columns.Clear();
            DataGridViewStaffShow(equipa.id_equipa);

            treeViewStaff.Nodes.Clear();
            TreeViewStaffShow(equipa.id_equipa);

        }

        public void RefreshTeamGrid()
        {
            dataGridViewEquipas.Columns.Clear();
            DataGridViewEquipaShow();
        }

        private void lbl_UploadImagem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();

            open.InitialDirectory = @"C:\Pictures";
            open.Filter = "Image Files(*.jpg; *.jpeg; *.png;) | *.jpg; *.jpeg; *.png;";
            open.ValidateNames = true;
            open.Multiselect = false;

            if (open.ShowDialog() == DialogResult.OK)
            {
                DeleteImagemDB();
                pictureBox2.Image = new Bitmap(open.FileName);
                pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                EditImagem();
            }
        }

        private void lbl_RemoverImagem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show($"Eliminar imagem da equipa?", "Eliminar imagem", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                pictureBox2.Image = null;
            }
            else
            {
                return;
            }
        }

        private void EditImagem()
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
            byte[] file_byte = ImageToByteArray(pictureBox2.Image);

            // Criar uma System.Data.Linq.Binary 
            System.Data.Linq.Binary file_binary = new System.Data.Linq.Binary(file_byte);
            Imagen img = new Imagen
            {
                id_imagem = (maxImgID + 1),
                imagem = file_binary,
                id_equipa = equipa.id_equipa,
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

        public void DeleteImagemDB()
        {
            Imagen x = new Imagen();

            var imagemEliminar = from Imagen in dc.Imagens
                                 where Imagen.id_equipa == equipa.id_equipa
                                 select Imagen;

            x = imagemEliminar.Single();

            dc.Imagens.DeleteOnSubmit(x);

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

        #endregion

        #region Validacoes

        public bool txtNomeEquipa_TextCheck()
        {
            bool output = true;

            foreach (char car in txtNomeEquipa.Text)
            {
                if (!(char.IsLetter(car)) && (!char.IsWhiteSpace(car)))
                {
                    MessageBox.Show("Introduza apenas letras", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNomeEquipa.Text = txtNomeEquipa.Text.Remove(txtNomeEquipa.Text.Length - 1, 1);
                    txtNomeEquipa.Focus();
                    output = false;
                }
            }

            return output;
        }

        public bool txtEstadio_TextCheck()
        {
            bool output = true;

            foreach (char car in txtEstadio.Text)
            {
                if (!(char.IsLetter(car)) && (!char.IsWhiteSpace(car)))
                {
                    MessageBox.Show("Introduza apenas letras", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEstadio.Text = txtEstadio.Text.Remove(txtEstadio.Text.Length - 1, 1);
                    txtEstadio.Focus();
                    output = false;
                }
            }

            return output;
        }

        private bool txtNomeStaff_TextCheck()
        {
            bool output = true;

            foreach (char car in txtNomeStaff.Text)
            {
                if (!(char.IsLetter(car)) && (!char.IsWhiteSpace(car)))
                {
                    MessageBox.Show("Introduza apenas letras", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNomeStaff.Text = txtNomeStaff.Text.Remove(txtNomeStaff.Text.Length - 1, 1);
                    txtNomeStaff.Focus();
                    output = false;
                }
            }

            return output;
        }

        private bool txtNomeJogador_TextCheck()
        {
            bool output = true;

            foreach (char car in txtNomeJogador.Text)
            {
                if (!(char.IsLetter(car)) && (!char.IsWhiteSpace(car)))
                {
                    MessageBox.Show("Introduza apenas letras", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNomeJogador.Text = txtNomeJogador.Text.Remove(txtNomeJogador.Text.Length - 1, 1);
                    txtNomeJogador.Focus();
                    output = false;
                }
            }

            return output;
        }

        public bool ValidarInfoEquipa()
        {
            bool output = true;

            // validar panel info
            if (string.IsNullOrEmpty(txtNomeEquipa.Text) || string.IsNullOrEmpty(txtEstadio.Text) || pictureBox2.Image == null)
            {
                MessageBox.Show("Existem campos por preencher", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                output = false;
            }

            return output;
        }

        public bool ValidarInfoStaff()
        {
            bool output = true;

            // validar panel info
            if (string.IsNullOrEmpty(txtNomeStaff.Text) || comboBoxFuncao.SelectedItem == null)
            {
                MessageBox.Show("Existem campos por preencher", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                output = false;
            }

            return output;
        }

        public bool ValidarInfoJogadores()
        {
            bool output = true;

            // validar panel info
            if (string.IsNullOrEmpty(txtNomeJogador.Text))
            {
                MessageBox.Show("Existem campos por preencher", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                output = false;
            }

            return output;
        }

        public bool ContarEquipas()
        {
            bool output = false;

            var contarEquipas = from Equipa in dc.Equipas select Equipa;

            if (contarEquipas.Count() == 6)
            {
                output = true;
            }

            return output;
        }

        public bool ContarStaff()
        {
            bool output = false;

            var contarStaff = from Staff in dc.Staffs select Staff;

            if (contarStaff.Count() == 50)
            {
                output = true;
            }

            return output;
        }

        public bool ContarJogadores()
        {
            bool output = false;

            var contarJogadores = from Jogadore in dc.Jogadores select Jogadore;

            if (contarJogadores.Count() == 25)
            {
                output = true;
            }

            return output;
        }

        public bool CheckNumeroCamisola()
        {
            bool output = false;

            var listaNum = from Jogadore in dc.Jogadores
                           where Jogadore.id_equipa == equipa.id_equipa
                           select Jogadore.numero;

            foreach (var num in listaNum)
            {
                if (num == (Convert.ToInt16(comboBoxNumCam.SelectedItem)))
                {
                    MessageBox.Show($"Já existe um jogador com o numero {num} nesta equipa", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    output = true;
                }
            }

            return output;
        }

        #endregion

        #endregion

        #region PanelJogos
        private void btGerarJogos_Click(object sender, EventArgs e)
        {
            if (!ContarEquipas())
            {
                // validar se existem 6 equipas. Se não, é necessario criar.
                MessageBox.Show("Não existem equipas suficientes para iniciar a época", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                btAddEquipa.Enabled = false;
                btGerarJogos.Enabled = false;
                circularProgressBar1.Visible = true;
                progressBar();
                circularProgressBar1.Visible = false;

                btGerarJogos.Enabled = false;
                //cria novo calendario
                PopularListaEquipas();
                GerarCalendarioVolta(listaEquipas);

                //reordena as equipas na listaEquipas
                listaEquipas.Reverse();

                //chama de novo o metodo gerarCalendario para gerar a SEGUNDA VOLTA
                GerarCalendarioVolta(listaEquipas);
                DataGridViewJogosShow();
            }
        }

        private void PopularListaEquipas()
        {
            var pesquisa = from Equipas in dc.Equipas
                           select Equipas;

            foreach (var e in pesquisa)
            {
                listaEquipas.Add(e);
            }
        }

        private int criarIdJogo()
        {
            var maxIdQuery = from Jogo in dc.Jogos
                             orderby Jogo.id_jogo descending
                             select Jogo.id_jogo;

            int maxIDJogo;

            if (maxIdQuery.Count() == 0)
            {
                maxIDJogo = 0;
            }
            else
            {
                maxIDJogo = maxIdQuery.Max();
            }

            return (maxIDJogo + 1);
        }

        public void GerarCalendarioVolta(List<Equipa> listaEquipas)
        {
            int numDays = (listaEquipas.Count - 1);
            int halfSize = listaEquipas.Count / 2;

            List<Equipa> teams = new List<Equipa>();

            teams.AddRange(listaEquipas.Skip(halfSize).Take(halfSize));
            teams.AddRange(listaEquipas.Skip(1).Take(halfSize - 1).ToArray().Reverse());

            int teamsSize = teams.Count;

            for (int day = 0; day < numDays; day++)
            {
                int teamIdx = day % teamsSize;

                // adicionar a criaçao dum objecto jornada
                Jornada jornada = new Jornada();
                {
                    jornada.id_jornada = contadorJornadas;
                    jornada.descricao = "Jornada " + (contadorJornadas);
                }

                dc.Jornadas.InsertOnSubmit(jornada);

                Jogo jogo = new Jogo
                {
                    id_jogo = criarIdJogo(),
                    equipa_casa = teams[teamIdx].id_equipa,
                    equipa_fora = listaEquipas[0].id_equipa,
                    id_jornada = jornada.id_jornada,
                    data_jogo = dataJornada,
                };

                dc.Jogos.InsertOnSubmit(jogo);

                try
                {
                    dc.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                for (int idx = 1; idx < halfSize; idx++)
                {
                    int firstTeam = (day + idx) % teamsSize;
                    int secondTeam = (day + teamsSize - idx) % teamsSize;

                    Jogo novoJogo = new Jogo
                    {
                        id_jogo = criarIdJogo(),
                        equipa_casa = teams[firstTeam].id_equipa,
                        equipa_fora = teams[secondTeam].id_equipa,
                        id_jornada = jornada.id_jornada,
                        data_jogo = dataJornada,
                    };

                    dc.Jogos.InsertOnSubmit(novoJogo);

                    try
                    {
                        dc.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

                dataJornada = dataJornada.AddDays(7);
                contadorJornadas++;
            }
        }

        private void DataGridViewJogosShow()
        {
            //preenche a datagrid com os Jogos da epoca
            dataGridViewJogos.Columns.Add("colIdJornada", "Jornada");
            dataGridViewJogos.Columns.Add("colCasa", "Equipa Casa   ");
            dataGridViewJogos.Columns.Add("colVisitante", "Visitante   ");
            dataGridViewJogos.Columns.Add("colJogos", "Jogos");
            dataGridViewJogos.Columns[3].Visible = false;
            dataGridViewJogos.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewJogos.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewJogos.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            DataGridViewCellStyle estilo1 = new DataGridViewCellStyle();
            estilo1.BackColor = Color.FromArgb(83, 104, 120);


            DataGridViewCellStyle estilo2 = new DataGridViewCellStyle();
            estilo2.BackColor = Color.SlateGray;

            var jogos = from Jogo in dc.Jogos
                        select Jogo;

            int linha = 0;

            foreach (Jogo j in jogos)
            {
                DataGridViewRow mb = new DataGridViewRow();
                dataGridViewJogos.Rows.Add(mb);

                dataGridViewJogos.Rows[linha].Cells[0].Value = j.id_jornada;
                dataGridViewJogos.Rows[linha].Cells[3].Value = j.id_jogo;


                var nomeEquipaCasa = (from e in dc.Equipas
                                      where e.id_equipa == j.equipa_casa
                                      select e.nome).Single().ToString();

                var nomeEquipaFora = (from e in dc.Equipas
                                      where e.id_equipa == j.equipa_fora
                                      select e.nome).Single().ToString();


                dataGridViewJogos.Rows[linha].Cells[1].Value = nomeEquipaCasa;
                dataGridViewJogos.Rows[linha].Cells[2].Value = nomeEquipaFora;


                if ((int)dataGridViewJogos.Rows[linha].Cells[0].Value % 2 == 0)
                {
                    dataGridViewJogos.Rows[linha].DefaultCellStyle = estilo1;
                }

                linha++;
            }

            dataGridViewJogos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void dataGridViewJogos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            lblVersus.Visible = true;
            lblAvisoCalendario.Visible = true;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dataGridViewJogos.Rows[e.RowIndex];

                lblJornada.Text = row.Cells[0].Value.ToString();
                lblJogo.Text = row.Cells[3].Value.ToString();
                lblEquipaCasa.Text = row.Cells[1].Value.ToString();
                lblEquipaFora.Text = row.Cells[2].Value.ToString();

                int id_jogo = Convert.ToInt16(lblJogo.Text);

                var pesquisaJogo = from Jogo in dc.Jogos
                                   where Jogo.id_jogo == id_jogo
                                   select Jogo;

                jogoSelecionado = pesquisaJogo.Single();

                //preencher logotipos das equipas do jogo
                var imagemEquipaCasa = from Imagen in dc.Imagens
                                       where Imagen.id_equipa == jogoSelecionado.equipa_casa
                                       select Imagen.imagem;

                Image x = (Bitmap)((new ImageConverter()).ConvertFrom(imagemEquipaCasa.Single().ToArray()));

                pictureBox16.Image = x;

                var imagemEquipaFora = from Imagen in dc.Imagens
                                       where Imagen.id_equipa == jogoSelecionado.equipa_fora
                                       select Imagen.imagem;

                Image y = (Bitmap)((new ImageConverter()).ConvertFrom(imagemEquipaFora.Single().ToArray()));

                pictureBox17.Image = y;

                //preencher o estadio onde vai ser o jogo
                var estadioJogo = (from a in dc.Equipas
                                   where a.id_equipa == jogoSelecionado.equipa_casa
                                   select a.estadio).Single().ToString();

                lblNomeEstadio.Text = estadioJogo.ToString();

                //preencher data do jogo
                var dataJogo = from Jogo in dc.Jogos
                               where Jogo.id_jogo == jogoSelecionado.id_jogo
                               select Jogo.data_jogo;

                DateTime dt = new DateTime();

                dt = (DateTime)dataJogo.Single();

                CultureInfo pt = new CultureInfo("pt-PT");

                lblDataJogo.Text = dt.ToString("d MMMM yyyy", pt);

                //preencher golos caso ja tenham sido inseridos
                var golostxt1 = from f in dc.Resultados
                                where f.id_jogo == jogoSelecionado.id_jogo
                                select f.golos_casa;

                var golostxt2 = from f in dc.Resultados
                                where f.id_jogo == jogoSelecionado.id_jogo
                                select f.golos_fora;


                if (golostxt1.Any() && golostxt2.Any())
                {
                    txtGolosCasa.Text = golostxt1.Single().ToString();
                    txtGolosFora.Text = golostxt2.Single().ToString();
                }
                else
                {
                    txtGolosCasa.Text = string.Empty;
                    txtGolosFora.Text = string.Empty;
                    btSubmeter.Enabled = true;
                }

            }
        }

        private void progressBar()
        {
            for (int i = 0; i <= 100; i++)
            {
                Thread.Sleep(5);
                circularProgressBar1.Value = i;
                circularProgressBar1.Update();
            }
        }

        private void btSubmeter_Click(object sender, EventArgs e)
        {
            //pesquisar na bd se ja existe resultado para o jogo selecionado na datagrid
            var pesquisaResultado = from f in dc.Resultados
                                    where f.id_jogo == jogoSelecionado.id_jogo
                                    select f;


            if (pesquisaResultado.Any())
            {
                //permite fazer update de um resultado para um jogo ja existente
                Resultado resultadoSelecionado = new Resultado();

                resultadoSelecionado = pesquisaResultado.Single();

                resultadoSelecionado.golos_casa = Convert.ToInt16(txtGolosCasa.Text);
                resultadoSelecionado.golos_fora = Convert.ToInt16(txtGolosFora.Text);

                try
                {
                    dc.SubmitChanges();

                    MessageBox.Show("Resultado atualizado com sucesso", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(txtGolosCasa.Text) || string.IsNullOrEmpty(txtGolosFora.Text))
                {
                    MessageBox.Show("Existem campos por preencher", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int golosCasa = Convert.ToInt16(txtGolosCasa.Text);
                int golosFora = Convert.ToInt16(txtGolosFora.Text);


                //gerar um id novo para os resultados
                var maxIdQuery = from t in dc.Resultados
                                 orderby t.id_resultado descending
                                 select t.id_resultado;

                int maxIDResultado;

                if (maxIdQuery.Count() == 0)
                {
                    maxIDResultado = 0;
                }
                else
                {
                    maxIDResultado = maxIdQuery.Max();
                }

                //cria o novo resultado
                Resultado resultado = new Resultado()
                {
                    id_resultado = maxIDResultado + 1,
                    id_jogo = jogoSelecionado.id_jogo,
                    id_jornada = jogoSelecionado.id_jornada,
                    golos_casa = golosCasa,
                    golos_fora = golosFora,
                    equipa_casa = jogoSelecionado.equipa_casa,
                    equipa_fora = jogoSelecionado.equipa_fora,
                };

                dc.Resultados.InsertOnSubmit(resultado);

                try
                {
                    dc.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            //refresca a grid com os resultados
            DataGridViewResultadosShowData();
        }

        private void DataGridViewResultadosShow()
        {
            dataGridViewResultados.Columns.Add("colIdJogo", " Jogo");
            dataGridViewResultados.Columns.Add("colCasa", "Casa");
            dataGridViewResultados.Columns.Add("colGolosCasa", "G");
            dataGridViewResultados.Columns.Add("colGolosFora", "G");
            dataGridViewResultados.Columns.Add("colVisitante", "Visitante");
            dataGridViewResultados.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewResultados.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewResultados.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void DataGridViewResultadosShowData()
        {
            var resultados = from Resultado in dc.Resultados
                             select Resultado;

            int linha = 0;

            foreach (Resultado r in resultados)
            {
                DataGridViewRow mb = new DataGridViewRow();
                dataGridViewResultados.Rows.Add(mb);

                dataGridViewResultados.Rows[linha].Cells[0].Value = r.id_jogo;

                var nomeEquipaCasa = (from t1 in dc.Jogos
                                      join t2 in dc.Equipas on t1.equipa_casa equals t2.id_equipa
                                      where t1.id_jogo == r.id_jogo
                                      select t2.nome).Single().ToString();

                var nomeEquipaFora = (from t1 in dc.Jogos
                                      join t2 in dc.Equipas on t1.equipa_fora equals t2.id_equipa
                                      where t1.id_jogo == r.id_jogo
                                      select t2.nome).Single().ToString();

                dataGridViewResultados.Rows[linha].Cells[1].Value = nomeEquipaCasa;
                dataGridViewResultados.Rows[linha].Cells[2].Value = r.golos_casa;
                dataGridViewResultados.Rows[linha].Cells[3].Value = r.golos_fora;
                dataGridViewResultados.Rows[linha].Cells[4].Value = nomeEquipaFora;


                RemoverLinhasDaGrid(dataGridViewResultados);

                linha++;
            }

        }

        private void txtGolosCasa_TextChanged(object sender, EventArgs e)
        {
            foreach (char car in txtGolosCasa.Text)
            {
                if ((!char.IsDigit(car)))
                {
                    MessageBox.Show("Insira apenas numeros", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGolosCasa.Text = txtGolosCasa.Text.Remove(txtGolosCasa.Text.Length - 1, 1);
                    txtGolosCasa.Focus();
                    break;
                }
            }
        }

        private void txtGolosFora_TextChanged(object sender, EventArgs e)
        {
            foreach (char car in txtGolosFora.Text)
            {
                if ((!char.IsDigit(car)))
                {
                    MessageBox.Show("Insira apenas numeros", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGolosFora.Text = txtGolosFora.Text.Remove(txtGolosFora.Text.Length - 1, 1);
                    txtGolosFora.Focus();
                    break;
                }
            }
        }

        #endregion

        #region PanelClassificacao

        private void DataGridViewClassificaoShow()
        {
            this.dataGridViewClassificacao.Columns["Nome"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            var equipas = from Equipa in dc.Equipas
                          select Equipa;

            int linha = 0;

            foreach (Equipa e in equipas)
            {
                //golos
                int totalJogos = 0;
                int golosMarcadosCasa = 0;
                int golosMarcadosFora = 0;
                int golosMarcadosTotal = 0;
                int golosSofridosEquipaCasa = 0;
                int golosSofridosEquipaFora = 0;
                int golosSofridosTotal = 0;
                int vitorias = 0;
                int derrotas = 0;
                int empates = 0;
                int pontos = 0;

                DataGridViewRow mb = new DataGridViewRow();
                dataGridViewClassificacao.Rows.Add(mb);

                // imagem de cada equipa
                var imagemEquipa = from Imagen in dc.Imagens
                                   where Imagen.id_equipa == e.id_equipa
                                   select Imagen.imagem;

                Image a = (Bitmap)((new ImageConverter()).ConvertFrom(imagemEquipa.Single().ToArray()));

                //Resultados da equipa em JOGOS EM CASA
                var listaResultadosJogos = from r in dc.Resultados
                                           where r.equipa_casa == e.id_equipa || r.equipa_fora == e.id_equipa
                                           select r;

                foreach (var resultado in listaResultadosJogos)
                {
                    //golos marcados pela equipa "e" casa
                    if (resultado.equipa_casa == e.id_equipa)
                    {
                        golosMarcadosCasa += resultado.golos_casa;

                        golosSofridosEquipaCasa += (resultado.golos_casa + resultado.golos_fora) - resultado.golos_casa;

                        if (resultado.golos_casa > resultado.golos_fora)
                        {
                            vitorias++;
                            pontos += 3;
                        }
                        else if (resultado.golos_casa < resultado.golos_fora)
                        {
                            derrotas++;
                            pontos += 0;
                        }
                        else
                        {
                            empates++;
                            pontos += 1;
                        }
                    }

                    //golos marcados pela equipa "e" fora
                    if (resultado.equipa_fora == e.id_equipa)
                    {
                        golosMarcadosFora += resultado.golos_fora;

                        golosSofridosEquipaFora += (resultado.golos_casa + resultado.golos_fora) - resultado.golos_fora;

                        if (resultado.golos_fora > resultado.golos_casa)
                        {
                            vitorias++;
                            pontos += 3;
                        }
                        else if (resultado.golos_fora < resultado.golos_casa)
                        {
                            derrotas++;
                            pontos += 0;
                        }
                        else
                        {
                            empates++;
                            pontos += 1;
                        }
                    }

                    golosMarcadosTotal = golosMarcadosCasa + golosMarcadosFora;

                    golosSofridosTotal = golosSofridosEquipaCasa + golosSofridosEquipaFora;

                    totalJogos++;

                }

                var pesquisaEstatistica = from f in dc.Estatisticas
                                          where f.id_equipa == e.id_equipa
                                          select f;

                Estatistica estatistica = new Estatistica();

                //verificar se ja existe uma estatistica para esta equipa
                if (pesquisaEstatistica.Any())
                {
                    //atribui ao obj estatistica a estatistica ja existente e faz replace dos dados
                    estatistica = pesquisaEstatistica.Single();

                    estatistica.vitorias = vitorias;
                    estatistica.empates = empates;
                    estatistica.derrotas = derrotas;
                    estatistica.pontos = pontos;
                    estatistica.golos_marcados = golosMarcadosTotal;
                    estatistica.golos_sofridos = golosSofridosTotal;
                    estatistica.total_jogos = totalJogos;

                    try
                    {
                        dc.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }
                else
                {
                    //obter um novo id para a nova estatistica
                    var maxIdQuery = from t in dc.Estatisticas
                                     orderby t.id_estatistica descending
                                     select t.id_estatistica;

                    int maxIDEstatistica;

                    if (maxIdQuery.Count() == 0)
                    {
                        maxIDEstatistica = 0;
                    }
                    else
                    {
                        maxIDEstatistica = maxIdQuery.Max();
                    }

                    //coloca no novo objecto estatistica os novos dados da nova estatistica                    
                    estatistica.id_estatistica = maxIDEstatistica + 1;
                    estatistica.id_equipa = e.id_equipa;
                    estatistica.vitorias = vitorias;
                    estatistica.empates = empates;
                    estatistica.derrotas = derrotas;
                    estatistica.pontos = pontos;
                    estatistica.golos_marcados = golosMarcadosTotal;
                    estatistica.golos_sofridos = golosSofridosTotal;
                    estatistica.total_jogos = totalJogos;

                    dc.Estatisticas.InsertOnSubmit(estatistica);

                    try
                    {
                        dc.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }

                PopularDataGridViewClassificacao(e, a, totalJogos, estatistica, linha);

                linha++;


            }

            SortingDataGridViewClassificacao();

            foreach (DataGridViewRow r in dataGridViewClassificacao.Rows)
            {
                r.Cells[0].Value = (r.Index + 1 + " º");
            }

            foreach (DataGridViewRow row in dataGridViewClassificacao.Rows)
            {
                row.Height = 70;
            }


        }

        private void PopularDataGridViewClassificacao(Equipa e, Image img, int jogosTotal, Estatistica estatistica, int linha)
        {
            dataGridViewClassificacao.Rows[linha].Cells[1].Value = img;
            dataGridViewClassificacao.Rows[linha].Cells[2].Value = e.nome;
            dataGridViewClassificacao.Rows[linha].Cells[3].Value = estatistica.pontos;
            dataGridViewClassificacao.Rows[linha].Cells[4].Value = jogosTotal;
            dataGridViewClassificacao.Rows[linha].Cells[5].Value = estatistica.vitorias;
            dataGridViewClassificacao.Rows[linha].Cells[6].Value = estatistica.empates;
            dataGridViewClassificacao.Rows[linha].Cells[7].Value = estatistica.derrotas;
            dataGridViewClassificacao.Rows[linha].Cells[8].Value = estatistica.golos_marcados;
            dataGridViewClassificacao.Rows[linha].Cells[9].Value = estatistica.golos_sofridos;

        }

        private void SortingDataGridViewClassificacao()
        {
            var maisPontos = (from t1 in dc.Estatisticas
                              select t1.pontos).Max();

            var pesquisaEquipaComMaisPontos = from t1 in dc.Estatisticas
                                              where t1.pontos == maisPontos
                                              select t1;

            if (pesquisaEquipaComMaisPontos.Count() == 1)
            {
                //se exitir mais que uma equipa com o mesmo numero de pontos sorting da lista por equipa com vitorias pontos
                dataGridViewClassificacao.Sort(dataGridViewClassificacao.Columns[3], ListSortDirection.Descending);
            }
            else
            {
                //sorting da list
                //a por equipa com mais pontos
                dataGridViewClassificacao.Sort(dataGridViewClassificacao.Columns[5], ListSortDirection.Descending);
            }
        }

        private void RemoverLinhasDaGrid(DataGridView datagrid)
        {
            for (int i = datagrid.Rows.Count - 1; i > -1; i--)
            {
                DataGridViewRow row = datagrid.Rows[i];
                if (!row.IsNewRow && row.Cells[0].Value == null)
                {
                    datagrid.Rows.RemoveAt(i);
                }
            }
        }

        #endregion

        #region PanelEstatisticas

        private void MostrarEstatisticas()
        {
            PieChartShow();
            MostrarEstatisticasGlobais();
            MostrarVencedor();
            PesquisarAtaque();
            PesquisarDefesa();
            PesquisarVitorias();
            PesquisarEmpates();
            PesquisarDerrotas();
            PesquisarPontos();
        }

        private void PieChartShow()
        {
            int totalVitorias = (int)dc.Estatisticas.Sum(x => x.vitorias);
            int totalEmpates = (int)dc.Estatisticas.Sum(x => x.empates);
            int totalDerrotas = (int)dc.Estatisticas.Sum(x => x.derrotas);

            chart1.Series["s1"].Points.AddXY("Vitorias", totalVitorias);
            chart1.Series["s1"].Points.AddXY("Empates", totalEmpates);
            chart1.Series["s1"].Points.AddXY("Derrotas", totalDerrotas);
            chart1.Series["s1"].IsValueShownAsLabel = true;
        }

        private void MostrarEstatisticasGlobais()
        {
            var golosMarcados = dc.Estatisticas.Sum(x => x.golos_marcados);
            var golosSofridos = dc.Estatisticas.Sum(x => x.golos_sofridos);
            var totalJogos = dc.Estatisticas.Sum(x => x.total_jogos);

            var totalGolos = golosMarcados + golosSofridos;
            decimal mediaGolosJogo = (decimal)(totalGolos / totalJogos);

            lbl_totalJogosEpoca.Text = dc.Estatisticas.Sum(x => x.total_jogos).ToString();
            lbl_totalGolosEpoca.Text = totalGolos.ToString();
            lbl_mediaGolosEpoca.Text = $"{mediaGolosJogo:0.00}".ToString();
            lbl_totalVitoriasEpoca.Text = dc.Estatisticas.Sum(x => x.vitorias).ToString();
            lbl_totalEmpatesEpoca.Text = dc.Estatisticas.Sum(x => x.empates).ToString();
            lbl_totalDerrotasEpoca.Text = dc.Estatisticas.Sum(x => x.derrotas).ToString();
        }

        private void MostrarVencedor()
        {
            int maxPontos = (int)dc.Estatisticas.Max(x => x.pontos);

            var equipaComMaisPontos = from t1 in dc.Estatisticas
                                      join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                      where t1.pontos == maxPontos
                                      select t2;

            Equipa equipaVencedora = new Equipa();

            if (equipaComMaisPontos.Count() == 1)
            {
                equipaVencedora = equipaComMaisPontos.Single();
            }
            else
            {
                foreach (var e in equipaComMaisPontos)
                {
                    int maxVitorias = (int)dc.Estatisticas.Max(x => x.vitorias);
                    int maxGolos = (int)dc.Estatisticas.Max(x => x.golos_marcados);

                    var estatisticasDaEquipa = from t1 in dc.Estatisticas
                                               join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                               where t1.id_equipa == e.id_equipa
                                               select t1;

                    Estatistica estatistica = new Estatistica();

                    estatistica = estatisticasDaEquipa.Single();

                    if (estatistica.vitorias == maxVitorias)
                    {
                        if (estatistica.golos_marcados == maxGolos)
                        {
                            equipaVencedora = e;
                        }
                    }
                }
            }


            lbl_Vencedor.Text = equipaVencedora.nome;

            var imagemEquipa = from Imagen in dc.Imagens
                               where Imagen.id_equipa == equipaVencedora.id_equipa
                               select Imagen.imagem;

            Image a = (Bitmap)((new ImageConverter()).ConvertFrom(imagemEquipa.Single().ToArray()));

            pictureBox_Vencedor.Image = a;
        }



        private void PesquisarAtaque()
        {
            int maisGolosMarcados = (int)dc.Estatisticas.Max(x => x.golos_marcados);

            var equipaComMaisGolos = from t1 in dc.Estatisticas
                                     join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                     where t1.golos_marcados == maisGolosMarcados
                                     select t2;

            lbl_maisGolosMarcados.Text = maisGolosMarcados.ToString();

            foreach (var e in equipaComMaisGolos)
            {
                lbl_melhorAtaque.Text += $"{e.nome} ";
            }

            int menosGolosMarcados = (int)dc.Estatisticas.Min(x => x.golos_marcados);

            var equipaComMenosGolos = from t1 in dc.Estatisticas
                                      join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                      where t1.golos_marcados == menosGolosMarcados
                                      select t2;

            lbl_menosGolosMarcados.Text = menosGolosMarcados.ToString();

            foreach (var e in equipaComMenosGolos)
            {
                lbl_piorAtaque.Text += $"{e.nome} ";
            }
        }
        private void PesquisarDefesa()
        {
            int menosGolosSofridos = (int)dc.Estatisticas.Min(x => x.golos_sofridos);

            var equipaComMenosGolos = from t1 in dc.Estatisticas
                                      join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                      where t1.golos_sofridos == menosGolosSofridos
                                      select t2;

            lbl_menosGolosSofridos.Text = menosGolosSofridos.ToString();

            foreach (var e in equipaComMenosGolos)
            {
                lbl_MelhorDefesa.Text += $"{e.nome} ";
            }

            int maisGolosSofridos = (int)dc.Estatisticas.Max(x => x.golos_sofridos);

            var equipaComMaisGolos = from t1 in dc.Estatisticas
                                     join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                     where t1.golos_sofridos == maisGolosSofridos
                                     select t2;

            lbl_maisGolosSofridos.Text = maisGolosSofridos.ToString();

            foreach (var e in equipaComMaisGolos)
            {
                lbl_piorDefesa.Text += $"{e.nome} ";
            }

        }
        private void PesquisarVitorias()
        {

            int maisVitorias = (int)dc.Estatisticas.Max(x => x.vitorias);

            var equipaMaisVitorias = from t1 in dc.Estatisticas
                                     join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                     where t1.vitorias == maisVitorias
                                     select t2;

            lbl_maisVitorias.Text = maisVitorias.ToString();

            foreach (var e in equipaMaisVitorias)
            {
                lbl_equipaMaisVitorias.Text += $"{e.nome} ";
            }

            int menosVitorias = (int)dc.Estatisticas.Min(x => x.vitorias);

            var equipaMenosVitorias = from t1 in dc.Estatisticas
                                      join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                      where t1.vitorias == menosVitorias
                                      select t2;

            lbl_menosVitorias.Text = menosVitorias.ToString();

            foreach (var e in equipaMenosVitorias)
            {
                lbl_equipaMenosVitorias.Text += $"{e.nome} ";
            }
        }

        private void PesquisarEmpates()
        {
            int maisEmpates = (int)dc.Estatisticas.Max(x => x.empates);

            var equipaMaisEmpates = from t1 in dc.Estatisticas
                                    join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                    where t1.empates == maisEmpates
                                    select t2;

            lbl_maisEmpates.Text = maisEmpates.ToString();

            foreach (var e in equipaMaisEmpates)
            {
                lbl_equipaMaisEmpates.Text += $"{e.nome} ";
            }

            int menosEmpates = (int)dc.Estatisticas.Min(x => x.empates);

            var equipaMenosEmpates = from t1 in dc.Estatisticas
                                     join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                     where t1.empates == menosEmpates
                                     select t2;

            lbl_menosEmpates.Text = menosEmpates.ToString();

            foreach (var e in equipaMenosEmpates)
            {
                lbl_equipaMenosEmpates.Text += $"{e.nome} ";
            }

        }

        private void PesquisarDerrotas()
        {
            int maisDerrotas = (int)dc.Estatisticas.Max(x => x.derrotas);

            var equipaMaisDerrotas = from t1 in dc.Estatisticas
                                     join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                     where t1.derrotas == maisDerrotas
                                     select t2;

            lbl_maisDerrotas.Text = maisDerrotas.ToString();

            foreach (var e in equipaMaisDerrotas)
            {
                lbl_equipaMaisDerrotas.Text += $"{e.nome} ";
            }

            int menosDerrotas = (int)dc.Estatisticas.Min(x => x.derrotas);

            var equipaMenosDerrotas = from t1 in dc.Estatisticas
                                      join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                      where t1.derrotas == menosDerrotas
                                      select t2;

            lbl_menosDerrotas.Text = menosDerrotas.ToString();

            foreach (var e in equipaMenosDerrotas)
            {
                lbl_equipaMenosDerrotas.Text += $"{e.nome} ";
            }
        }

        private void PesquisarPontos()
        {
            int maisPontos = (int)dc.Estatisticas.Max(x => x.pontos);

            var equipaMaisPontos = from t1 in dc.Estatisticas
                                   join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                   where t1.pontos == maisPontos
                                   select t2;

            lbl_maisPontos.Text = maisPontos.ToString();

            foreach (var e in equipaMaisPontos)
            {
                lbl_equipaMaisPontos.Text += $"{e.nome} ";
            }

            int menosPontos = (int)dc.Estatisticas.Min(x => x.pontos);

            var equipaMenosPontos = from t1 in dc.Estatisticas
                                    join t2 in dc.Equipas on t1.id_equipa equals t2.id_equipa
                                    where t1.pontos == menosPontos
                                    select t2;

            lbl_menosPontos.Text = menosPontos.ToString();

            foreach (var e in equipaMenosPontos)
            {
                lbl_equipaMenosPontos.Text += $"{e.nome} ";
            }
        }

        private void LimparLabelsEstatisticas()
        {
            lbl_totalJogosEpoca.Text = null;
            lbl_totalGolosEpoca.Text = null;
            lbl_mediaGolosEpoca.Text = null;
            lbl_totalVitoriasEpoca.Text = null;
            lbl_totalEmpatesEpoca.Text = null;
            lbl_totalDerrotasEpoca.Text = null;
            lbl_Vencedor.Text = null;
            pictureBox_Vencedor.Text = null;
            lbl_melhorAtaque.Text = null;
            lbl_maisGolosMarcados.Text = null;
            lbl_piorAtaque.Text = null;
            lbl_menosGolosMarcados.Text = null;
            lbl_MelhorDefesa.Text = null;
            lbl_menosGolosSofridos.Text = null;
            lbl_piorDefesa.Text = null;
            lbl_maisGolosSofridos.Text = null;
            lbl_equipaMaisVitorias.Text = null;
            lbl_maisVitorias.Text = null;
            lbl_equipaMenosVitorias.Text = null;
            lbl_menosVitorias.Text = null;
            lbl_equipaMaisDerrotas.Text = null;
            lbl_maisDerrotas.Text = null;
            lbl_equipaMenosDerrotas.Text = null;
            lbl_menosDerrotas.Text = null;
            lbl_equipaMaisEmpates.Text = null;
            lbl_maisEmpates.Text = null;
            lbl_equipaMenosEmpates.Text = null;
            lbl_menosEmpates.Text = null;
            lbl_equipaMaisPontos.Text = null;
            lbl_maisPontos.Text = null;
            lbl_equipaMenosPontos.Text = null;
            lbl_menosPontos.Text = null;
            chart1.Series["s1"].Points.Clear();
            chart1.Series["s1"].Points.Clear();
            chart1.Series["s1"].Points.Clear();

        }

        #endregion

        private void lbl_about_Click(object sender, EventArgs e)
        {

        }

        private void lbl_settings_Click(object sender, EventArgs e)
        {

        }

        private void bt_sairApp_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Fechar aplicação?", "Sair", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                this.Close();
            }
            else
            {
                return;
            }
        }






    }
}
