using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;

using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;




namespace Attendance.Controllers
{
    public class HomeController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\kaysh\source\repos\TimeInAttendance\TimeInAttendance\App_Data\AttendanceDB.mdf;Integrated Security=True";
        public ActionResult Index()
        {
            return RedirectToAction("Login", "auth");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Admin()
        {
            Session["attendance"] = null;
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "auth");
            }


            return View();
        }

        [HttpPost]
        public ActionResult Admin(FormCollection collection, HttpPostedFileBase img)
        {
            // Check if image is uploaded
            if (img == null || img.ContentLength <= 0)
            {
                Response.Write("<script>alert('Please upload an image.')</script>");
                return View();
            }

            string imag = Path.GetFileName(img.FileName);
            string logpath = "c:\\attendance";  // Ensure this directory exists on your server
            string filepath = Path.Combine(logpath, imag);
            img.SaveAs(filepath);

            var code = collection["code"];
            var title = collection["title"];
            var description = collection["description"];
            var course_type = collection["courseType"];
            var units = collection["units"];
            var scheduleDays = collection.GetValues("schedule");
            var block = collection["block"];

            var startTimes = new List<string>();
            var endTimes = new List<string>();

            // Check if any required field is missing
            if (string.IsNullOrEmpty(code) ||
                string.IsNullOrEmpty(title) ||
                string.IsNullOrEmpty(description) ||
                string.IsNullOrEmpty(course_type) ||
                string.IsNullOrEmpty(units) ||
                scheduleDays == null ||
                scheduleDays.Length == 0 ||
                string.IsNullOrEmpty(block))
            {
                Response.Write("<script>alert('Please input all required data.')</script>");
                return View();
            }

            // Process schedule and times
            foreach (var day in scheduleDays)
            {
                var startTime = collection[$"time-{day}-start"];
                var endTime = collection[$"time-{day}-end"];

                if (string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
                {
                    Response.Write($"<script>alert('Please input both start and end times for {day}.')</script>");
                    return View();
                }

                startTimes.Add(startTime);
                endTimes.Add(endTime);
            }

            var timePairs = startTimes.Zip(endTimes, (start, end) => $"{start} - {end}");

            // Save course data to database
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT INTO COURSE (COURSE_CODE, COURSE_TITLE, COURSE_DESCRIPTION, COURSE_TYPE, COURSE_UNITS, COURSE_SCHED, COURSE_TIME, COURSE_SECTION, COURSE_IMAGE_URL) " +
                                      "VALUES (@code, @title, @description, @course_type, @units, @schedule, @time, @block, @file)";
                    cmd.Parameters.AddWithValue("@code", code);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@course_type", course_type);
                    cmd.Parameters.AddWithValue("@units", units);
                    cmd.Parameters.AddWithValue("@schedule", string.Join(", ", scheduleDays));
                    cmd.Parameters.AddWithValue("@time", string.Join(", ", timePairs));
                    cmd.Parameters.AddWithValue("@block", block);
                    cmd.Parameters.AddWithValue("@file", imag);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "Successfully Created.";
                        return RedirectToAction("Admin"); // Redirect to avoid form resubmission
                    }
                    else
                    {
                        // Insert failed
                        Response.Write("<script>alert('Failed to create the course. Please try again.')</script>");
                        return View();
                    }
                }
            }
        }



        private bool CheckIfCodeExists(string code)
        {
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT COUNT(*) FROM COURSE WHERE COURSE_CODE = @code"; // Correct table and column name
                    cmd.Parameters.AddWithValue("@code", code);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        [HttpGet]
        public FileResult Image(string filename)
        {
            var folder = "c:\\attendance";
            var filepath = Path.Combine(folder, filename);
            if (!System.IO.File.Exists(filepath))
            {
                // Return a default image or handle the error
            }
            var mime = System.Web.MimeMapping.GetMimeMapping(Path.GetFileName(filepath));
            return new FilePathResult(filepath, mime);
        }


        // UPDATE
        public ActionResult CourseUpdate()
        {
            var data = new List<object>();
            var code = Request["code"];
            var title = Request["title"];
            var course_type = Request["courseType"];
            var units = Request["units"];
            var time = Request["time"];
            var block_section = Request["block"];
            var description = Request["description"];
            var schedule = Request["schedule"] ?? ""; // Default to an empty string if schedule is null

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "UPDATE COURSE SET" +
                                      " COURSE_TITLE = @title, " +
                                      " COURSE_TYPE = @course_type, " +
                                      " COURSE_UNITS = @units, " +
                                      " COURSE_TIME = @time, " +
                                      " COURSE__SECTION = @block_section, " +
                                      " COURSE_DESCRIPTION = @description, " +
                                      " COURSE_SCHED = @schedule" +
                                      " WHERE COURSE_CODE ='" + code + "'";
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@course_type", course_type);
                    cmd.Parameters.AddWithValue("@units", units);
                    cmd.Parameters.AddWithValue("@time", time);
                    cmd.Parameters.AddWithValue("@block_section", block_section);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@schedule", schedule);

                    var ctr = cmd.ExecuteNonQuery();
                    if (ctr > 0)
                    {
                        data.Add(new
                        {
                            mess = 0
                        });
                    }
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }




        public ActionResult CourseSearch()
        {
            var data = new List<object>();
            var code = Request["code"];

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"SELECT * FROM COURSE"
                                    + " WHERE COURSE_CODE='" + code + "'";
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        data.Add(new
                        {
                            mess = 0,
                            code = reader["course_code"].ToString(),
                            title = reader["course_title"].ToString(),
                            course_type = reader["course_type"].ToString(),
                            units = reader["course_units"].ToString(),
                            schedule = reader["course_sched"].ToString(),
                            time = reader["course_time"].ToString(),
                            block = reader["course_section"].ToString(),
                            description = reader["course_description"].ToString()
                        });
                    }
                    else
                    {
                        data.Add(new
                        {
                            mess = 1
                        });
                    }
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // DELETE
        public ActionResult deleteCourse()
        {
            var data = new List<object>();
            var code = Request["code"];

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = $"DELETE FROM COURSE WHERE COURSE_ID='" + code + "'";
                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr > 0)
                        {
                            data.Add(new
                            {
                                mess = 0
                            });
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                data.Add(new
                {
                    mess = 1
                });
            }
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Unenroll(string student_id, string course_code, string course_section)
        {
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = @"
                            DELETE FROM Student_Course
                            WHERE STUDENT_ID = @stud_id
                                AND COURSE_ID IN (
                                    SELECT Course.COURSE_ID
                                    FROM Course
                                    WHERE Course.COURSE_CODE = @course_code
                                        AND Course.COURSE_SECTION = @course_section
                                )";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@course_code", course_code);
                        cmd.Parameters.AddWithValue("@course_section", course_section);
                        cmd.Parameters.AddWithValue("@stud_id", student_id);

                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true, message = "Student Application Denied" });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }

        [HttpPost]
        public async Task<ActionResult> Enroll(string student_id, string course_code, string course_section, string contact)
        {
            try
            {
                contact = contact.StartsWith("0") ? contact.Substring(1) : contact;

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = @"
                            UPDATE Student_Course
                            SET STUCOURSE_STATUS = 'ENROLLED'
                            WHERE STUDENT_ID = @stud_id
                                AND COURSE_ID IN (
                                    SELECT Course.COURSE_ID
                                    FROM Course
                                    WHERE Course.COURSE_CODE = @course_code
                                        AND Course.COURSE_SECTION = @course_section
                                )";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@course_code", course_code);
                        cmd.Parameters.AddWithValue("@course_section", course_section);
                        cmd.Parameters.AddWithValue("@stud_id", student_id);

                        cmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Student Application Approved" });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }


        [HttpPost]
        public ActionResult Attendance(string course_id, string date)
        {
            try
            {
                var attendanceList = new List<Dictionary<string, object>>();

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = @"
                                    SELECT 
                                        s.STUDENT_ID,
                                        s.STUDENT_LASTNAME,
                                        s.STUDENT_FIRSTNAME,
                                        s.STUDENT_MIDNAME,
                                        a.ATTENDANCE_DATE,
                                        a.ATTENDANCE_TIME_IN,
                                        a.ATTENDANCE_STATUS,
                                        a.ATTENDANCE_SUPPORTING_DOCS
                                    FROM 
                                        STUDENT_COURSE sc
                                        JOIN ATTENDANCE a ON sc.COURSE_ID = a.COURSE_ID AND a.STUDENT_ID = sc.STUDENT_ID AND a.ATTENDANCE_DATE = @date AND sc.COURSE_ID = @course_id
                                        JOIN STUDENT s ON s.STUDENT_ID = a.STUDENT_ID
                                    ORDER BY 
                                        s.STUDENT_LASTNAME, s.STUDENT_FIRSTNAME;
                                ";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@course_id", course_id);
                        cmd.Parameters.AddWithValue("@date", date);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var list = new Dictionary<string, object>()
                                {
                                    {"student_id", reader["student_id"].ToString() },
                                    {"student_lastname", reader["student_lastname"].ToString() },
                                    {"student_firstname", reader["student_firstname"].ToString() },
                                    {"student_midname", reader["student_midname"].ToString() },
                                    {"time_in", reader["attendance_time_in"].ToString() },
                                    {"remarks", reader["attendance_status"].ToString() },
                                    {"docs", reader["attendance_supporting_docs"].ToString() }
                                };

                                attendanceList.Add(list);
                            }
                        }
                    }
                }
                return Json(attendanceList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }

        [HttpPost]
        public ActionResult Start(string course_id)
        {
            try
            {

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = @"
                                    INSERT INTO attendance (student_id, course_id, attendance_date)
                                    SELECT student_id, course_id, GETDATE() AS attendance_date
                                    FROM student_course
                                    WHERE course_id = @course_id;
                                ";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@course_id", course_id);

                        cmd.ExecuteNonQuery();
                        Session["attendance"] = "start";
                        Session["course_id"] = course_id;
                    }
                }
                return Json(new { success = true });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }
    }
}