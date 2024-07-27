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
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Web.Services.Description;





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
            string logpath = "c:\\attendance"; 
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
        public async Task<ActionResult> Deny(string student_id, string student_name, string course_code, string course_section, string contact)
        {
            try
            {
                contact = contact.StartsWith("0") ? contact.Substring(1) : contact;


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

                var message = $"Dear {student_name},\n\nWe regret to inform you that your request to join {course_code} {course_section} has been denied. Should you have any concerns, feel free to contact the department.\n\nBest regards,\nProf. Rey Caliao";

                long number = long.Parse("63" + contact);

                var client = new HttpClient();
                client.BaseAddress = new Uri("https://5y9mzy.api.infobip.com");

                client.DefaultRequestHeaders.Add("Authorization", "App d8abe1ab1300d60a2cd89527ec7a1572-18705815-11c2-4bfd-ae31-9152d3cd83e9");
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var body = $@"{{
                                ""messages"": [
                                    {{
                                        ""destinations"": [
                                            {{
                                                ""to"": ""{number}""
                                            }}
                                        ],
                                        ""from"": ""CCICT"",
                                        ""text"": ""{message.Replace("\"", "\\\"").Replace("\n", "\\n")}""
                                    }}
                                ]
                            }}";
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/sms/2/text/advanced", content);

                var responseContent = await response.Content.ReadAsStringAsync();


                return Json(new { success = true, message = "Student Application Denied" });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }

        [HttpPost]
        public async Task<ActionResult> Enroll(string student_id, string student_name, string course_code, string course_section, string contact)
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

                long number = long.Parse("63" + contact);
                string message = $"Dear {student_name},\n\nCongratulations! You have been approved for the course {course_code} {course_section}. We look forward to your active participation.\n\nBest regards,\nProf. Rey Caliao";

                var client = new HttpClient();
                client.BaseAddress = new Uri("https://5y9mzy.api.infobip.com");

                client.DefaultRequestHeaders.Add("Authorization", "App d8abe1ab1300d60a2cd89527ec7a1572-18705815-11c2-4bfd-ae31-9152d3cd83e9");
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var body = $@"{{
                                ""messages"": [
                                    {{
                                        ""destinations"": [
                                            {{
                                                ""to"": ""{number}""
                                            }}
                                        ],
                                        ""from"": ""CCICT"",
                                        ""text"": ""{message.Replace("\"", "\\\"").Replace("\n", "\\n")}""
                                    }}
                                ]
                            }}";
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/sms/2/text/advanced", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                return Json(new { success = true, message = "Student Application Approved " });


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
                                    WHERE course_id = @course_id AND stucourse_status = 'ENROLLED';
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

        [HttpPost]
        public ActionResult Profiles(string course_id)
        {
            try
            {
                var profileList = new List<Dictionary<string, object>>();

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = @"
                                    SELECT 
                                        'TO BE DROPPED' AS Status,
                                        s.student_id AS ID,
                                        CONCAT(
                                                s.student_lastname, ', ', 
                                                s.student_firstname, ' ', 
                                                LEFT(s.student_midname, 1), '.'
                                            ) AS Student_Fullname,
                                        s.student_contact_no AS Student_Contact,
                                        g.guardian_name AS Guardian_Fullname,
                                        g.guardian_contact AS Guardian_Contact,
                                        g.guardian_rel AS Relationship_to_Guardian,
                                        sc.stucourse_totallate AS Total_Late,
                                        sc.stucourse_totalabsent AS Total_Absent
                                    FROM 
                                        dbo.student_course sc
                                    JOIN 
                                        dbo.student s ON sc.student_id = s.student_id AND sc.course_id = @course_id
                                    JOIN 
                                        dbo.guardian g ON s.guardian_id = g.guardian_id
                                    JOIN 
                                        dbo.[drop] d ON sc.student_id = d.stud_id AND sc.course_id = d.course_id

                                    UNION

                                    SELECT 
                                        sc.stucourse_status AS Status,
                                        s.student_id AS ID,
                                        CONCAT(
                                                s.student_lastname, ', ', 
                                                s.student_firstname, ' ', 
                                                LEFT(s.student_midname, 1), '.'
                                            ) AS Student_Fullname,    
                                        s.student_contact_no AS Student_Contact,
                                        g.guardian_name AS Guardian_Fullname,
                                        g.guardian_contact AS Guardian_Contact,
                                        g.guardian_rel AS Relationship_to_Guardian,
                                        sc.stucourse_totallate AS Total_Late,
                                        sc.stucourse_totalabsent AS Total_Absent
                                    FROM 
                                        dbo.student_course sc
                                    JOIN 
                                        dbo.student s ON sc.student_id = s.student_id AND sc.course_id = @course_id
                                    JOIN 
                                        dbo.guardian g ON s.guardian_id = g.guardian_id
                                    LEFT JOIN 
                                        dbo.[drop] d ON sc.student_id = d.stud_id AND sc.course_id = d.course_id
                                    WHERE 
                                        d.drop_id IS NULL;
                                ";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@course_id", course_id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var list = new Dictionary<string, object>()
                                {
                                    {"Status", reader["Status"].ToString() },
                                    {"ID", reader["ID"].ToString() },
                                    {"Student_Fullname", reader["Student_Fullname"].ToString() },
                                    {"Student_Contact", reader["Student_Contact"].ToString() },
                                    {"Guardian_Fullname", reader["Guardian_Fullname"].ToString() },
                                    {"Guardian_Contact", reader["Guardian_Contact"].ToString() },
                                    {"Relationship_to_Guardian", reader["Relationship_to_Guardian"].ToString() },
                                    {"Total_Late", reader["Total_Late"].ToString() },
                                    {"Total_Absent", reader["Total_Absent"].ToString() }
                                };

                                profileList.Add(list);
                            }
                        }
                    }
                }
                return Json(profileList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }

        [HttpPost]
        public async Task<ActionResult> Unenroll(string student_id, string student_name, string course_code, string course_section, string contact)
        {
            try
            {
                contact = contact.StartsWith("0") ? contact.Substring(1) : contact;


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

                var message = $"Dear {student_name},\n\nWe regret to inform that you have been unenrolled on the class {course_code} {course_section}. Should you have any concerns, feel free to contact the department.\n\nBest regards,\nProf. Rey Caliao";

                long number = long.Parse("63" + contact);

                var client = new HttpClient();
                client.BaseAddress = new Uri("https://5y9mzy.api.infobip.com");

                client.DefaultRequestHeaders.Add("Authorization", "App d8abe1ab1300d60a2cd89527ec7a1572-18705815-11c2-4bfd-ae31-9152d3cd83e9");
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var body = $@"{{
                                ""messages"": [
                                    {{
                                        ""destinations"": [
                                            {{
                                                ""to"": ""{number}""
                                            }}
                                        ],
                                        ""from"": ""CCICT"",
                                        ""text"": ""{message.Replace("\"", "\\\"").Replace("\n", "\\n")}""
                                    }}
                                ]
                            }}";
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/sms/2/text/advanced", content);

                var responseContent = await response.Content.ReadAsStringAsync();


                return Json(new { success = true, message = "Student Application Denied" });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }

        [HttpPost]
        public ActionResult deleteDrop(string student_id, string course_id)
        {
            try
            {

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = @"
                            DELETE FROM dbo.[drop]
                            WHERE STUD_ID = @stud_id AND COURSE_ID = @course_id";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@course_id", course_id);
                        cmd.Parameters.AddWithValue("@stud_id", student_id);

                        cmd.ExecuteNonQuery();
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