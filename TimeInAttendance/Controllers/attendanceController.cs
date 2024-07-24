using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        public ActionResult TimeIn(string student_id, string course_id)
        {
            try
            {

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = @"
                            SELECT 
                                s.student_id,
                                CASE 
                                    WHEN a.attendance_time_in IS NOT NULL THEN 1
                                    ELSE 0
                                END AS status
                            FROM 
                                student s
                            INNER JOIN 
                                attendance a ON s.student_id = a.student_id AND a.course_id = @course_id AND a.attendance_date = CAST(GETDATE() AS DATE)
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
                                                  AND attendance_date = CAST(GETDATE() AS DATE)";

                                    using (var updateCmd = new SqlCommand(updateQuery, db))
                                    {
                                        updateCmd.Parameters.AddWithValue("@current_time", DateTime.Now.TimeOfDay);
                                        updateCmd.Parameters.AddWithValue("@course_id", course_id);
                                        updateCmd.Parameters.AddWithValue("@student_id", student_id);

                                        updateCmd.ExecuteNonQuery();
                                        return Json(new { success = true, message = "Successfully Time In" });

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

                        return Json(new { success = true});
                    }

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });

            }
        }

        [HttpPost]
        public ActionResult Absent(string course_id)
        {
            try
            {

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    string query = @"UPDATE attendance 
                                    SET attendance_status = 'ABSENT'
                                    WHERE 
                                        course_id = @course_id
                                        AND attendance_time_in is null
                                        AND attendance_date = CAST(GETDATE() AS DATE)";

                    using (var cmd = new SqlCommand(query, db))
                    {
                        cmd.Parameters.AddWithValue("@course_id", course_id);

                        cmd.ExecuteNonQuery();

                        Session["attendance"] = null;
                        Session["course_id"] = null;
                        return Json(new { success = true });
                    }

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });

            }
        }
    }
}