using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Attendance_System.Controllers
{

    public class attendanceController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\kaysh\source\repos\TimeInAttendance\TimeInAttendance\App_Data\AttendanceDB.mdf;Integrated Security=True";

        public ActionResult attendance(string course_id, string course_code, string course_section, string time, string date)
        {
            if (Session["attendance"] == "start")
            {
                Session["user"] = null;
                ViewBag.course_code = course_code;
                ViewBag.course_section = course_section;
                ViewBag.sched_time = time;
                ViewBag.sched_date = date;
                return View();
            }

            return RedirectToAction("Admin", "Home");
        }

        [HttpPost]
        public async Task<ActionResult> TimeIn(string student_id, string course_id)
        {
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = @"
                            SELECT 
                                s.student_id,
                                s.student_firstname,
                                s.student_lastname,
                                s.student_photo_url,
                                g.guardian_name,
                                g.guardian_contact,
                                c.course_title,
                                c.course_section,
                                CASE 
                                    WHEN a.attendance_time_in IS NOT NULL THEN 1
                                    ELSE 0
                                END AS status
                            FROM 
                                student s
                            INNER JOIN 
                                attendance a ON s.student_id = a.student_id AND a.course_id = @course_id AND a.attendance_date = CAST(GETDATE() AS DATE)
                            INNER JOIN 
                                guardian g ON s.guardian_id = g.guardian_id
                            INNER JOIN 
                                course c ON a.course_id = c.course_id
                            WHERE s.student_id = @student_id
                            ";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@course_id", course_id);
                        cmd.Parameters.AddWithValue("@student_id", student_id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int status = (int)reader["status"];
                                var guardian_name = reader["guardian_name"].ToString();
                                var student_name = reader["student_firstname"].ToString() + " " + reader["student_lastname"].ToString();
                                var student_image = reader["student_photo_url"].ToString();
                                var title = reader["course_title"].ToString();
                                var section = reader["course_section"].ToString();
                                var contact = reader["guardian_contact"].ToString();

                                contact = contact.StartsWith("0") ? contact.Substring(1) : contact;

                                if (status == 0)
                                {
                                    reader.Close();
                                    string updateQuery = @"
                                                UPDATE attendance 
                                                SET attendance_time_in = @current_time,
                                                    attendance_status = CASE 
                                                                          WHEN attendance_status IS NULL THEN 'ON TIME' 
                                                                          ELSE attendance_status 
                                                                        END
                                                WHERE student_id = @student_id 
                                                  AND course_id = @course_id 
                                                  AND attendance_date = CAST(GETDATE() AS DATE);

                                                SELECT attendance_status
                                                    FROM attendance
                                                    WHERE student_id = @student_id 
                                                      AND course_id = @course_id 
                                                      AND attendance_date = CAST(GETDATE() AS DATE);
                                    ";

                                    using (var updateCmd = new SqlCommand(updateQuery, db))
                                    {
                                        updateCmd.Parameters.AddWithValue("@current_time", DateTime.Now.TimeOfDay);
                                        updateCmd.Parameters.AddWithValue("@course_id", course_id);
                                        updateCmd.Parameters.AddWithValue("@student_id", student_id);

                                        var attendanceStatus = updateCmd.ExecuteScalar()?.ToString();


                                        var client = new HttpClient();
                                        client.BaseAddress = new Uri("https://e1pd3n.api.infobip.com");

                                        client.DefaultRequestHeaders.Add("Authorization", "App 76cbc8aba3b943d5a2dae8264a17011c-2e3203d1-648f-4448-ba4e-eb95936702df");
                                        client.DefaultRequestHeaders.Add("Accept", "application/json");

                                        string message;
                                        long number = long.Parse("63" + contact);

                                        if (attendanceStatus == "ON TIME")
                                        {
                                            message = $"Dear {guardian_name},\n\nWe appreciate {student_name} consistently arriving on time for {title} {section}. This punctuality is valued.\n\nThank you,\nProf. Rey Caliao";

                                        }
                                        else if (attendanceStatus == "LATE")
                                        {
                                            message = $"Dear {guardian_name},\n\nWe would like to inform you that {student_name} was marked late for the class {title} {section} on {DateTime.Now.ToString("MMMM dd, yyyy")}. Please ensure that he/she arrives on time to avoid further issues.\n\nThank you,\nProf. Rey Caliao";

                                        }
                                        else
                                        {
                                            message = $"Dear {guardian_name},\n\nWe regret to inform you that {student_name} was marked absent for the class {title} {section} on {DateTime.Now.ToString("MMMM dd, yyyy")}. Please provide any supporting documents if he/she has a valid reason for the absence.\n\nThank you,\nProf. Rey Caliao";

                                        }

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


                                        return Json(new { success = true, message = "Successfully Time In", student_id = student_id, student_name = student_name, student_image = student_image });

                                    }

                                }
                                else
                                {
                                    return Json(new { success = false, message = "Already Timed In" });
                                }
                            }
                            else
                            {
                                return Json(new { success = false, message = "Not enrolled in this course" });

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
        public ActionResult Late(string course_id)
        {
            try
            {

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = @"UPDATE attendance 
                                    SET attendance_status = 'LATE'
                                    WHERE 
                                        course_id = @course_id
                                        AND attendance_time_in is null
                                        AND attendance_date = CAST(GETDATE() AS DATE)";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@course_id", course_id);

                        cmd.ExecuteNonQuery();

                        return Json(new { success = true });
                    }

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });

            }
        }

        [HttpPost]
        public async Task<ActionResult> Absent(string course_id)
        {
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    await db.OpenAsync(); // Use asynchronous open for better scalability

                    string updateQuery = @"
                                        UPDATE attendance 
                                        SET attendance_status = 'ABSENT'
                                        WHERE 
                                            course_id = @course_id
                                            AND attendance_time_in IS NULL
                                            AND attendance_date = CAST(GETDATE() AS DATE)";

                    using (var updateCmd = new SqlCommand(updateQuery, db))
                    {
                        updateCmd.Parameters.AddWithValue("@course_id", course_id);
                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    // Clear session variables if necessary
                    Session["attendance"] = null;
                    Session["course_id"] = null;

                    string selectQuery = @"
                                    SELECT 
                                        s.student_id,
                                        s.student_firstname,
                                        s.student_lastname,
                                        g.guardian_name,
                                        g.guardian_contact,
                                        c.course_title,
                                        c.course_section
                                    FROM 
                                        student s
                                    INNER JOIN 
                                        attendance a ON s.student_id = a.student_id 
                                    AND a.course_id = @course_id 
                                    AND a.attendance_date = CAST(GETDATE() AS DATE) 
                                    AND a.attendance_status = 'ABSENT'
                                    INNER JOIN 
                                        guardian g ON s.guardian_id = g.guardian_id
                                    INNER JOIN 
                                        course c ON a.course_id = c.course_id";

                    using (var selectCmd = new SqlCommand(selectQuery, db))
                    {
                        selectCmd.Parameters.AddWithValue("@course_id", course_id);

                        using (var reader = await selectCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var guardian_name = reader["guardian_name"].ToString();
                                var student_name = reader["student_firstname"].ToString() + " " + reader["student_lastname"].ToString();
                                var title = reader["course_title"].ToString();
                                var section = reader["course_section"].ToString();
                                var contact = reader["guardian_contact"].ToString();

                                // Adjust contact number format

                                contact = contact.StartsWith("0") ? contact.Substring(1) : contact;
                                long number = long.Parse("63" + contact);

                                string message = $"Dear {guardian_name},\n\nWe regret to inform you that {student_name} was marked absent for the class {title} {section} on {DateTime.Now:MMMM dd, yyyy}. Please provide any supporting documents if he/she has a valid reason for the absence.\n\nThank you,\nProf. Rey Caliao";

                                using (var client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri("https://e1pd3n.api.infobip.com");
                                    client.DefaultRequestHeaders.Add("Authorization", "App 76cbc8aba3b943d5a2dae8264a17011c-2e3203d1-648f-4448-ba4e-eb95936702df");
                                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                                    var body = new
                                    {
                                        messages = new[]
                                                {
                                            new
                                            {
                                                destinations = new[]
                                                {
                                                    new { to = number.ToString() }
                                                },
                                                from = "CCICT",
                                                text = message
                                            }
                                        }
                                    };

                                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                                    var response = await client.PostAsync("/sms/2/text/advanced", content);
                                    var responseContent = await response.Content.ReadAsStringAsync();

                                    if (!response.IsSuccessStatusCode)
                                    {
                                        return Json(new { success = false, message = "Failed to send SMS: " + responseContent });
                                    }
                                }

                            }
                        }
                    }

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


    }
}