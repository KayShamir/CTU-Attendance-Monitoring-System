using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Attendance_System.Controllers
{
    public class authController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\kaysh\source\repos\TimeInAttendance\TimeInAttendance\App_Data\AttendanceDB.mdf;Integrated Security=True";

        public ActionResult Login()
        {
            Session["user"] = null;
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection, HttpPostedFileBase img)
        {
            try
            {
                string imag = Path.GetFileName(img.FileName);
                string logpath = "c:\\attendance";
                string filepath = Path.Combine(logpath, imag);
                img.SaveAs(filepath);

                var stud_id = collection["stud_id"];
                var stud_lastname = collection["stud_lastname"];
                var stud_firstname = collection["stud_firstname"];
                var stud_midname = collection["stud_midname"];
                var stud_contact = collection["stud_contact"];
                var stud_password = collection["stud_password"];

                var guard_name = collection["guard_name"];
                var guard_rel = collection["guard_rel"];
                var guard_contact = collection["guard_contact"];
                int guard_id;

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = "SELECT GUARDIAN_ID" +
                    " FROM GUARDIAN" +
                    " WHERE GUARDIAN_NAME = @guard_name AND GUARDIAN_REL = @guard_rel AND GUARDIAN_CONTACT = @guard_contact";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@guard_name", guard_name);
                        cmd.Parameters.AddWithValue("@guard_rel", guard_rel);
                        cmd.Parameters.AddWithValue("@guard_contact", guard_contact);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                guard_id = (int)reader["GUARDIAN_ID"];
                                reader.Close();
                            }
                            else
                            {

                                reader.Close();

                                using (var cmd1 = db.CreateCommand())
                                {
                                    cmd1.CommandType = CommandType.Text;
                                    cmd1.CommandText = "INSERT INTO GUARDIAN (GUARDIAN_NAME, GUARDIAN_REL, GUARDIAN_CONTACT) " +
                                       "OUTPUT INSERTED.GUARDIAN_ID " +
                                       "VALUES (@guard_name, @guard_rel, @guard_contact)";
                                    cmd1.Parameters.AddWithValue("@guard_name", guard_name);
                                    cmd1.Parameters.AddWithValue("@guard_rel", guard_rel);
                                    cmd1.Parameters.AddWithValue("@guard_contact", guard_contact);

                                    guard_id = (int)cmd1.ExecuteScalar();

                                }
                            }

                            using (var cmd2 = db.CreateCommand())
                            {
                                cmd2.CommandType = CommandType.Text;
                                cmd2.CommandText = "INSERT INTO STUDENT (STUDENT_ID, STUDENT_LASTNAME, STUDENT_FIRSTNAME, STUDENT_MIDNAME, STUDENT_CONTACT_NO, STUDENT_PASSWORD, STUDENT_PHOTO_URL, GUARDIAN_ID) " +
                                                  "VALUES (@stud_id, @stud_lastname, @stud_firstname, @stud_midname, @stud_contact, @stud_password, @imag, @guard_id)";
                                cmd2.Parameters.AddWithValue("@stud_id", stud_id);
                                cmd2.Parameters.AddWithValue("@stud_lastname", stud_lastname);
                                cmd2.Parameters.AddWithValue("@stud_firstname", stud_firstname);
                                cmd2.Parameters.AddWithValue("@stud_midname", stud_midname);
                                cmd2.Parameters.AddWithValue("@stud_contact", stud_contact);
                                cmd2.Parameters.AddWithValue("@stud_password", stud_password);
                                cmd2.Parameters.AddWithValue("@imag", imag);
                                cmd2.Parameters.AddWithValue("@guard_id", guard_id);

                                int rowsAffected = cmd2.ExecuteNonQuery();

                                return Json(new { success = true, message = "Registration successful!" });
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Verify(FormCollection collection)
        {
            var stud_id = collection["stud_id"];
            var password = collection["stud_password"];

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    string query = "SELECT STUDENT_ID, STUDENT_FIRSTNAME, STUDENT_LASTNAME" +
                        " FROM STUDENT" +
                        " WHERE STUDENT_ID = @stud_id AND STUDENT_PASSWORD = @stud_pass";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@stud_id", stud_id);
                        cmd.Parameters.AddWithValue("@stud_pass", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Session["user"] = "student";
                                Session["id"] = reader["STUDENT_ID"];
                                Session["firstname"] = reader["STUDENT_FIRSTNAME"];
                                Session["lastname"] = reader["STUDENT_LASTNAME"];

                                return Json(new { success = true, user = "Student" });
                            }

                        }
                    }

                    string query1 = "SELECT ADMIN_ID, ADMIN_NAME" +
                                " FROM ADMIN" +
                                " WHERE ADMIN_ID = @id AND ADMIN_PASSWORD = @password";


                    using (var cmd1 = new SqlCommand(query1, db))
                    {
                        cmd1.Parameters.AddWithValue("@id", stud_id);
                        cmd1.Parameters.AddWithValue("@password", password);

                        using (var reader1 = cmd1.ExecuteReader())
                        {
                            if (reader1.Read())
                            {
                                Session["user"] = "admin";
                                Session["name"] = reader1["ADMIN_NAME"];

                                return Json(new { success = true, user = "Admin" });
                            }

                        }
                    }
                    return Json(new { success = false, message = "Id or Password is not correct" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}