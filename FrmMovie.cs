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

namespace MovieProjectTest
{
    public partial class FrmMovie : Form

    {
        private byte[] selectedMovieImage = null;  // ใช้เก็บรูปภาพภาพยนตร์
        private byte[] selectedDirImage = null;    // ใช้เก็บรูปภาพผู้กำกับ

        public FrmMovie()
        {
            InitializeComponent();
        }

        private void FrmMovie_Load(object sender, EventArgs e)
        {
            LoadMovieList();  // โหลดข้อมูลภาพยนตร์ทั้งหมด
            LoadMovieTypes(); // โหลดหมวดหมู่ภาพยนตร์
            ClearInputFields(); // รีเซ็ตค่าฟอร์ม
        }

        private void LoadMovieList()
        {
            List<DBHelper.Movie> movies = DBHelper.LoadAllMovies();
            dgvMovieShowAll.Rows.Clear(); // ล้างข้อมูลเก่า

            foreach (var movie in movies)
            {
                dgvMovieShowAll.Rows.Add(movie.movieId, movie.movieName, movie.movieDetail,
                                         movie.movieDateSale.ToString("yyyy-MM-dd"), movie.movieTypeName);
            }
        }

        // 📌 โหลดหมวดหมู่ภาพยนตร์
        private void LoadMovieTypes()
        {
            using (SqlConnection conn = new SqlConnection(DBHelper.connStr))
            {
                string query = "SELECT movieTypeId, movieTypeName FROM movie_type_tb";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                cbbMovieType.Items.Clear();
                while (reader.Read())
                {
                    cbbMovieType.Items.Add(reader["movieTypeName"].ToString());
                }
                reader.Close();
            }
        }


        // 📌 ฟังก์ชันล้างค่าฟอร์มและรีเซ็ตปุ่ม
        private void ClearInputFields()
        {
            // รีเซ็ตข้อความ
            lbMovieId.Text = "";
            tbMovieName.Clear();
            tbMovieDetail.Clear();
            dtpMovieDateSale.Value = DateTime.Now; // คืนค่าเป็นวันปัจจุบัน
            cbbMovieType.SelectedIndex = -1; // คืนค่าให้ไม่มีตัวเลือก
            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;
            tbMovieDVDTotal.Text = "0";
            tbMovieDVDPrice.Text = "0.00";

            // รีเซ็ตรูปภาพ
            pcbMovieImg.Image = null;
            pcbDirMovie.Image = null;
            selectedMovieImage = null;
            selectedDirImage = null;

            // ปิดใช้งานปุ่มที่ไม่ควรใช้
            btSaveAddEdit.Enabled = false;
            btEdit.Enabled = false;
            btDel.Enabled = false;
            btAdd.Enabled = true; // เปิดปุ่มเพิ่มให้ใช้งาน
        }

        private void btSelectImg1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Image Files|*.jpg;*.jpeg;*.png";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                pcbMovieImg.Image = Image.FromFile(openFile.FileName);
                selectedMovieImage = DBHelper.ConvertImageToBinary(openFile.FileName);
            }
        }

        private void btSelectImg2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Image Files|*.jpg;*.jpeg;*.png";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                pcbMovieImg.Image = Image.FromFile(openFile.FileName);
                selectedMovieImage = DBHelper.ConvertImageToBinary(openFile.FileName);
            }
        }

        private void btSaveAddEdit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbMovieName.Text) || string.IsNullOrWhiteSpace(tbMovieDetail.Text))
            {
                MessageBox.Show("กรุณากรอกข้อมูลให้ครบถ้วน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DBHelper.Movie movie = new DBHelper.Movie
            {
                movieId = lbMovieId.Text,
                movieName = tbMovieName.Text,
                movieDetail = tbMovieDetail.Text,
                movieDateSale = dtpMovieDateSale.Value,
                movieTypeName = cbbMovieType.SelectedItem.ToString(),
                movieLengthHour = (int)nudMovieHour.Value,
                movieLengthMinute = (int)nudMovieMinute.Value,
                movieDVDTotal = int.Parse(tbMovieDVDTotal.Text),
                movieDVDPrice = decimal.Parse(tbMovieDVDPrice.Text),
                movieImg = selectedMovieImage,
                movieDirImg = selectedDirImage
            };

            if (btAdd.Enabled == false)
                DBHelper.AddMovie(movie);
            else
                DBHelper.UpdateMovie(movie);

            LoadMovieList();
            ClearInputFields();
        }

        private void EnableInputs(bool enable)
        {
            tbMovieName.Enabled = enable;
            tbMovieDetail.Enabled = enable;
            dtpMovieDateSale.Enabled = enable;
            cbbMovieType.Enabled = enable;
            nudMovieHour.Enabled = enable;
            nudMovieMinute.Enabled = enable;
            tbMovieDVDTotal.Enabled = enable;
            tbMovieDVDPrice.Enabled = enable;

            // เปิด/ปิดปุ่มเลือกรูป
            btSelectImg1.Enabled = enable;
            btSelectImg2.Enabled = enable;

            // เปิดปุ่มบันทึกเฉพาะเมื่ออนุญาตให้แก้ไขข้อมูล
            btSaveAddEdit.Enabled = enable;

            // ปิดปุ่มเพิ่มเมื่อแก้ไขข้อมูล
            btAdd.Enabled = !enable;
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            ClearInputFields(); // ล้างค่าก่อนเริ่ม
            lbMovieId.Text = GenerateNewMovieId(); // สร้างรหัสใหม่
            EnableInputs(true); // เปิดให้กรอกข้อมูล
            btSaveAddEdit.Enabled = true;
            btAdd.Enabled = false;
        }

        private string GenerateNewMovieId()
        {
            using (SqlConnection conn = new SqlConnection(DBHelper.connStr))
            {
                string query = "SELECT TOP 1 movieId FROM movie_tb ORDER BY movieId DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                var result = cmd.ExecuteScalar();

                string lastId = result != null ? result.ToString() : "mv000";
                int newIdNum = int.Parse(lastId.Substring(2)) + 1;
                return $"mv{newIdNum:D3}";
            }
        }

        private void btEdit_Click(object sender, EventArgs e)
        {
            EnableInputs(true);
            btEdit.Enabled = false;
            btSaveAddEdit.Enabled = true;
        }


        private void btMovieSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbMovieSearch.Text))
            {
                MessageBox.Show("กรุณาป้อนรหัสหรือชื่อภาพยนตร์ที่ต้องการค้นหา", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool searchById = rdMovieId.Checked; // ค้นหาด้วยรหัสหรือชื่อ
            List<DBHelper.Movie> movies = DBHelper.SearchMovies(tbMovieSearch.Text, searchById);

            lsMovieShow.Items.Clear();
            int index = 1;
            foreach (var movie in movies)
            {
                ListViewItem item = new ListViewItem(index.ToString());
                item.SubItems.Add(movie.movieName);
                item.Tag = movie;
                lsMovieShow.Items.Add(item);
                index++;
            }
        }

        private void lsMovieShow_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                DBHelper.Movie movie = (DBHelper.Movie)e.Item.Tag;

                lbMovieId.Text = movie.movieId;
                tbMovieName.Text = movie.movieName;
                tbMovieDetail.Text = movie.movieDetail;
                dtpMovieDateSale.Value = movie.movieDateSale;
                cbbMovieType.SelectedItem = movie.movieTypeName;
                nudMovieHour.Value = movie.movieLengthHour;
                nudMovieMinute.Value = movie.movieLengthMinute;
                tbMovieDVDTotal.Text = movie.movieDVDTotal.ToString();
                tbMovieDVDPrice.Text = movie.movieDVDPrice.ToString();

                pcbMovieImg.Image = DBHelper.ConvertBinaryToImage(movie.movieImg);
                pcbDirMovie.Image = DBHelper.ConvertBinaryToImage(movie.movieDirImg);

                btAdd.Enabled = false;
                btEdit.Enabled = true;
                btDel.Enabled = true;
            }
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("คุณแน่ใจหรือไม่ที่จะลบภาพยนตร์นี้?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                DBHelper.DeleteMovie(lbMovieId.Text);
                LoadMovieList();
                ClearInputFields();
                MessageBox.Show("ลบข้อมูลสำเร็จ", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            ClearInputFields();
        }

        private void btExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("คุณต้องการออกจากโปรแกรมหรือไม่?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btMovieSearch_Click_1(object sender, EventArgs e)
        {
            // ตรวจสอบว่ามีค่าที่ต้องการค้นหาหรือไม่
            if (string.IsNullOrWhiteSpace(tbMovieSearch.Text))
            {
                MessageBox.Show("กรุณาป้อนรหัสหรือชื่อภาพยนตร์ที่ต้องการค้นหา", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool searchById = rdMovieId.Checked; // ค้นหาด้วยรหัสหรือชื่อ
            List<DBHelper.Movie> movies = DBHelper.SearchMovies(tbMovieSearch.Text, searchById);

            // ล้างรายการก่อนแสดงผลใหม่
            lsMovieShow.Items.Clear();
            int index = 1;

            foreach (var movie in movies)
            {
                ListViewItem item = new ListViewItem(index.ToString());
                item.SubItems.Add(movie.movieName);
                item.Tag = movie;
                lsMovieShow.Items.Add(item);
                index++;
            }

            // แจ้งเตือนถ้าไม่พบข้อมูล
            if (movies.Count == 0)
            {
                MessageBox.Show("ไม่พบภาพยนตร์ที่ค้นหา", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
    }
}
