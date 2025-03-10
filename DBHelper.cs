using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Drawing;
using System.Data;
using System.IO;


namespace MovieProjectTest
{
    internal class DBHelper
    {


        public static string connStr = @"Server=DESKTOP-MEKKHALA\SQLEXPRESS;Database=movie_record_db;Integrated Security=True;";

        // 📌 โหลดข้อมูลภาพยนตร์ (JOIN movie_type_tb และดึงรูป Binary)
        public static List<Movie> LoadAllMovies()
        {
            List<Movie> movies = new List<Movie>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        m.movieId, 
                        m.movieName, 
                        m.movieDetail, 
                        m.movieDateSale, 
                        mt.movieTypeName, 
                        m.movieLengthHour, 
                        m.movieLengthMinute, 
                        m.movieDVDTotal, 
                        m.movieDVDPrice, 
                        m.movieImg, 
                        m.movieDirImg
                    FROM movie_tb AS m
                    JOIN movie_type_tb AS mt ON m.movieTypeId = mt.movieTypeId";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    movies.Add(new Movie
                    {
                        movieId = reader["movieId"].ToString(),
                        movieName = reader["movieName"].ToString(),
                        movieDetail = reader["movieDetail"].ToString(),
                        movieDateSale = Convert.ToDateTime(reader["movieDateSale"]),
                        movieTypeName = reader["movieTypeName"].ToString(),
                        movieLengthHour = Convert.ToInt32(reader["movieLengthHour"]),
                        movieLengthMinute = Convert.ToInt32(reader["movieLengthMinute"]),
                        movieDVDTotal = Convert.ToInt32(reader["movieDVDTotal"]),
                        movieDVDPrice = Convert.ToDecimal(reader["movieDVDPrice"]),
                        movieImg = reader["movieImg"] == DBNull.Value ? null : (byte[])reader["movieImg"],
                        movieDirImg = reader["movieDirImg"] == DBNull.Value ? null : (byte[])reader["movieDirImg"]
                    });
                }
                reader.Close();
            }
            return movies;
        }

        // 📌 เพิ่มภาพยนตร์ใหม่ (บันทึกรูปเป็น Binary)
        public static void AddMovie(Movie movie)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    INSERT INTO movie_tb 
                    (movieId, movieName, movieDetail, movieDateSale, movieTypeId, movieLengthHour, movieLengthMinute, 
                    movieDVDTotal, movieDVDPrice, movieImg, movieDirImg)
                    VALUES 
                    (@movieId, @movieName, @movieDetail, @movieDateSale, @movieTypeId, @movieLengthHour, @movieLengthMinute, 
                    @movieDVDTotal, @movieDVDPrice, @movieImg, @movieDirImg)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@movieId", movie.movieId);
                cmd.Parameters.AddWithValue("@movieName", movie.movieName);
                cmd.Parameters.AddWithValue("@movieDetail", movie.movieDetail);
                cmd.Parameters.AddWithValue("@movieDateSale", movie.movieDateSale);
                cmd.Parameters.AddWithValue("@movieTypeId", GetMovieTypeId(movie.movieTypeName));
                cmd.Parameters.AddWithValue("@movieLengthHour", movie.movieLengthHour);
                cmd.Parameters.AddWithValue("@movieLengthMinute", movie.movieLengthMinute);
                cmd.Parameters.AddWithValue("@movieDVDTotal", movie.movieDVDTotal);
                cmd.Parameters.AddWithValue("@movieDVDPrice", movie.movieDVDPrice);
                cmd.Parameters.AddWithValue("@movieImg", movie.movieImg ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@movieDirImg", movie.movieDirImg ?? (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateMovie(Movie movie)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            UPDATE movie_tb
            SET movieName = @movieName,
                movieDetail = @movieDetail,
                movieDateSale = @movieDateSale,
                movieTypeId = @movieTypeId,
                movieLengthHour = @movieLengthHour,
                movieLengthMinute = @movieLengthMinute,
                movieDVDTotal = @movieDVDTotal,
                movieDVDPrice = @movieDVDPrice,
                movieImg = @movieImg,
                movieDirImg = @movieDirImg
            WHERE movieId = @movieId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@movieId", movie.movieId);
                cmd.Parameters.AddWithValue("@movieName", movie.movieName);
                cmd.Parameters.AddWithValue("@movieDetail", movie.movieDetail);
                cmd.Parameters.AddWithValue("@movieDateSale", movie.movieDateSale);
                cmd.Parameters.AddWithValue("@movieTypeId", GetMovieTypeId(movie.movieTypeName));
                cmd.Parameters.AddWithValue("@movieLengthHour", movie.movieLengthHour);
                cmd.Parameters.AddWithValue("@movieLengthMinute", movie.movieLengthMinute);
                cmd.Parameters.AddWithValue("@movieDVDTotal", movie.movieDVDTotal);
                cmd.Parameters.AddWithValue("@movieDVDPrice", movie.movieDVDPrice);
                cmd.Parameters.AddWithValue("@movieImg", movie.movieImg ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@movieDirImg", movie.movieDirImg ?? (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteMovie(string movieId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "DELETE FROM movie_tb WHERE movieId = @movieId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@movieId", movieId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


        // 📌 ฟังก์ชันแปลงรูปเป็น Binary (ใช้ก่อนบันทึก)
        public static byte[] ConvertImageToBinary(string imagePath)
        {
            return File.Exists(imagePath) ? File.ReadAllBytes(imagePath) : null;
        }

        // 📌 ฟังก์ชันแปลง Binary เป็น Image (ใช้ตอนดึงข้อมูลมาแสดง)
        public static Image ConvertBinaryToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            using (MemoryStream ms = new MemoryStream(imageData))
            {
                return Image.FromStream(ms);
            }
        }

        // 📌 ฟังก์ชันดึง movieTypeId จาก movieTypeName
        public static int GetMovieTypeId(string movieTypeName)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT movieTypeId FROM movie_type_tb WHERE movieTypeName = @MovieTypeName";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MovieTypeName", movieTypeName);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }


        public static byte[] LoadMovieImage(string movieId, string columnName)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = $"SELECT {columnName} FROM movie_tb WHERE movieId = @movieId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@movieId", movieId);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != DBNull.Value ? (byte[])result : null;
            }
        }


        public static List<Movie> SearchMovies(string searchText, bool searchById)
        {
            List<Movie> movies = new List<Movie>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT 
                m.movieId, 
                m.movieName, 
                m.movieDetail, 
                m.movieDateSale, 
                mt.movieTypeName, 
                m.movieLengthHour, 
                m.movieLengthMinute, 
                m.movieDVDTotal, 
                m.movieDVDPrice, 
                m.movieImg, 
                m.movieDirImg
            FROM movie_tb AS m
            JOIN movie_type_tb AS mt ON m.movieTypeId = mt.movieTypeId
            WHERE " + (searchById ? "m.movieId = @searchText" : "m.movieName LIKE '%' + @searchText + '%'");

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@searchText", searchText);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    movies.Add(new Movie
                    {
                        movieId = reader["movieId"].ToString(),
                        movieName = reader["movieName"].ToString(),
                        movieDetail = reader["movieDetail"].ToString(),
                        movieDateSale = Convert.ToDateTime(reader["movieDateSale"]),
                        movieTypeName = reader["movieTypeName"].ToString(),
                        movieLengthHour = Convert.ToInt32(reader["movieLengthHour"]),
                        movieLengthMinute = Convert.ToInt32(reader["movieLengthMinute"]),
                        movieDVDTotal = Convert.ToInt32(reader["movieDVDTotal"]),
                        movieDVDPrice = Convert.ToDecimal(reader["movieDVDPrice"]),
                        movieImg = reader["movieImg"] == DBNull.Value ? null : (byte[])reader["movieImg"],
                        movieDirImg = reader["movieDirImg"] == DBNull.Value ? null : (byte[])reader["movieDirImg"]
                    });
                }
                reader.Close();
            }
            return movies;
        }


        // 📌 โครงสร้างคลาส Movie (เก็บภาพเป็น byte[])
        public class Movie
        {
            public string movieId { get; set; }
            public string movieName { get; set; }
            public string movieDetail { get; set; }
            public DateTime movieDateSale { get; set; }
            public string movieTypeName { get; set; }
            public int movieLengthHour { get; set; }
            public int movieLengthMinute { get; set; }
            public int movieDVDTotal { get; set; }
            public decimal movieDVDPrice { get; set; }
            public byte[] movieImg { get; set; } // เก็บภาพเป็น Binary
            public byte[] movieDirImg { get; set; } // เก็บภาพผู้กำกับเป็น Binary
        }

    }

}
